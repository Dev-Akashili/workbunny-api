using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using WorkBunny.Data;
using WorkBunny.Data.Entities.Identity;
using WorkBunny.Models.Account;
using WorkBunny.Models.Emails;
using WorkBunny.Services.Contracts;

namespace WorkBunny.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _emailService;
    private const string ErrorMsg = "Something went wrong. Please try again";
    private const string DefaultErrorMsg = "Something went wrong! Please try again later or contact us.";

    public AccountController(
        UserManager<ApplicationUser> userManager, 
        ApplicationDbContext db, 
        IEmailService emailService
        )
    {
        _userManager = userManager;
        _db = db;
        _emailService = emailService;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterModel model)
    {
        // Get User
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return BadRequest("Something went wrong");
        }
        
        // Check if the username is already taken
        var exists = await _db.Users.FirstOrDefaultAsync(x => x.CustomUserName == model.Username);
        if (exists != null)
        {
            await _userManager.DeleteAsync(user);
            await _db.SaveChangesAsync();
            return BadRequest("Username is taken!");
        }

        // Add the custom username and country
        user.CustomUserName = model.Username;
        user.Country = model.Country;
        await _userManager.UpdateAsync(user);

        return Ok();
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        
        // If the user does not exist
        if (user == null)
        {
            return Ok(new { Name = "error", Message = "Username or password is incorrect!" });
        }

        // If user exists check if problem is email is not confirmed or password is wrong
        var message = await _userManager.CheckPasswordAsync(user, model.Password)
            ? user.EmailConfirmed 
                ? "Username or password is incorrect!"
                :  "Verify your email to login"
            : "Username or password is incorrect!";
        var name =  await _userManager.CheckPasswordAsync(user, model.Password)
            ? user.EmailConfirmed ? "error" : "info"
            : "error";

        return Ok(new { Name = name, Message = message });
    }

    [HttpPost("sendEmailVerificationLink")]
    public async Task<IActionResult> SendEmailVerificationLink(string email, string name)
    {
        // Check if user exists
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return Ok();

        // If user exists but has already been confirmed
        if (user.EmailConfirmed && name.Equals("register")) return BadRequest("User Email is already confirmed");
        
        try
        { 
            await _emailService.SendEmailVerificationLink(email, name);
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            
            return BadRequest(DefaultErrorMsg);
        }
    }
    
    [HttpPost("verifyEmail")]
    public async Task<IActionResult> VerifyEmail(ValidateEmailModel model)
    {
        try
        {
            var message = await _emailService.ValidateCode(model, false);
            
            // Assign role to user
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) throw new NotFoundException("Something went wrong! Please try again.");
            
            if (message.Equals("success"))
            { 
                var roles = await _userManager.GetRolesAsync(user); 
                
                // Role assigning rule
                var role = model.Email.Equals("emksakashili@gmail.com") ? "Admin" : "Basic";
                
               // Add the role if the user doesn't already have it
               if (!roles.Contains(role)) await _userManager.AddToRoleAsync(user, role);
               
               await  _db.SaveChangesAsync();
            }

            if (!message.Equals("success"))
            {
                return BadRequest(message);
            }

            return Ok("Email successfully verified.");
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(ErrorMsg);
            }

            var message = await _emailService.ValidateCode(new ValidateEmailModel
            {
                Code = model.Code,
                CodeId = model.CodeId,
                Email = model.Email
            }, true);

            if (!message.Equals("success")) return BadRequest(new { Errors = message });
                
            // Reset Password
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
            
            if (result.Succeeded)
            {
                var verificationCode = await _db.VerificationCodes
                                           .FirstOrDefaultAsync(x => x.CodeId == model.CodeId)
                                       ?? throw new KeyNotFoundException(DefaultErrorMsg);
                _db.VerificationCodes.Remove(verificationCode);
                await _db.SaveChangesAsync();
                return Ok("Password has been successfully reset.");
            }
            else
            {
                // If resetting the password failed, return error messages
                var errors = result.Errors.Select(error => error.Description).ToArray();
                return BadRequest(new { Errors = errors });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return BadRequest(DefaultErrorMsg);
        }
    }
}
