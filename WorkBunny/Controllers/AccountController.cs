using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

    [HttpPost("login")]
    public async Task<ActionResult<AuthMessage>> Login(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        
        // If the user does not exist
        if (user == null)
        {
            return Ok(new AuthMessage
            {
                Email = email,
                Message = "A User with this email does not exist"
            });
        }

        // If user exists check if problem is email is not confirmed or password is wrong
        var message = user.EmailConfirmed ? "Password incorrect!" : "Verify email to login";

        return Ok(new AuthMessage
        {
            Email = email,
            Message = message
        });
    }

    [HttpPost("sendEmailVerificationCode")]
    public async Task<IActionResult> SendEmailVerificationCode(string email)
    {
        // Check if user exists
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return BadRequest("User does not exist");
        
        // If user exists but has already been confirmed
        if (user.EmailConfirmed) return BadRequest("User Email is already confirmed");
        
        try
        {
            var codeId = await _emailService.SendEmailVerificationCode(email);
            return Ok(codeId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return BadRequest("Something went wrong! Please try again later or contact us.");
        }
    }
    
    [HttpPost("verifyEmail")]
    public async Task<IActionResult> VerifyEmail(ValidateEmailModel model)
    {
        try
        {
            var message = await _emailService.ValidateCode(model);
            return Ok(new { Message = message });
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost("forgotPassword")]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        try
        {
            // Check for user
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return BadRequest("User with email does not exist");

            var code = await _emailService.SendEmailVerificationCode(email);
            return Ok(code);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return BadRequest("Something went wrong! Please try again later or contact us.");
        }
    }

    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email) ??
                       throw new KeyNotFoundException("Something went wrong. Please try again");

            var message = await _emailService.ValidateCode(new ValidateEmailModel
            {
                Code = model.Code,
                CodeId = model.CodeId,
                Email = model.Email
            });

            if (!message.Equals("success")) return BadRequest(message);
                
            // Reset Password
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
            
            if (result.Succeeded)
            {
                return Ok(new { Message = "Password has been successfully reset." });
            }
            else
            {
                // If resetting the password failed, return error messages
                return BadRequest(string.Join(", ", result.Errors.Select(error => error.Description)));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return BadRequest("Something went wrong! Please try again later or contact us.");
        }
    }
}