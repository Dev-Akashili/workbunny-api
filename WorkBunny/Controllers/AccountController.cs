using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using WorkBunny.Constants;
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
                Id = "AC_LN_1",
                Name = AuthMessageName.Error,
                Message = "A User with this email does not exist"
            });
        }

        // If user exists check if problem is email is not confirmed or password is wrong
        var message = user.EmailConfirmed ? "Password incorrect!" : "Verify email to login";
        var name = user.EmailConfirmed ? AuthMessageName.Error : AuthMessageName.Info;

        return Ok(new AuthMessage
        {
            Id = "AC_LN_2",
            Name = name,
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
            
            return BadRequest(new AuthMessage
            {
                Id = "AC_SE",
                Name = AuthMessageName.Error,
                Message = "Something went wrong! Please try again later or contact us."
            });
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

            return Ok(new AuthMessage
            {
                Id = "AC_VL_1",
                Name = message.Equals("success") ? AuthMessageName.Success : AuthMessageName.Error,
                Message = message.Equals("success") ? "Email successfully verified." : "Something went wrong. Please try again."
            });
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new AuthMessage
            {
                Id = "AC_VL_2",
                Name = AuthMessageName.Error,
                Message = e.Message
            });
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
            return BadRequest(new AuthMessage
            {
                Id = "AC_FD",
                Name = AuthMessageName.Error,
                Message = "Something went wrong! Please try again later or contact us."
            });
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
                return BadRequest(new AuthMessage
                {
                    Id = "AC_RD_1",
                    Name = AuthMessageName.Error,
                    Message = "Something went wrong. Please try again"
                });
            }

            var message = await _emailService.ValidateCode(new ValidateEmailModel
            {
                Code = model.Code,
                CodeId = model.CodeId,
                Email = model.Email
            }, true);

            if (!message.Equals("success")) return BadRequest(message);
                
            // Reset Password
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
            
            if (result.Succeeded)
            {
                var list = await _db.VerificationCodes.ToListAsync();
                await _emailService.ClearValidationCodes(list);
                
                return Ok(new AuthMessage
                {
                    Id = "AC_RD_2",
                    Name = AuthMessageName.Success,
                    Message = "Password has been successfully reset."
                });
            }
            else
            {
                // If resetting the password failed, return error messages
                return BadRequest(new AuthMessage
                {
                    Id = "AC_RD_3",
                    Name = AuthMessageName.Error,
                    Message = string.Join(", ", result.Errors.Select(error => error.Description))
                });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return BadRequest(new AuthMessage
            {
                Id = "AC_RD_4",
                Name = AuthMessageName.Error,
                Message = "Something went wrong! Please try again later or contact us."
            });
        }
    }
}