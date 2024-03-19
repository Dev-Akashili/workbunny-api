using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WorkBunny.Constants;
using WorkBunny.Data.Entities.Identity;
using WorkBunny.Models.Account;
using WorkBunny.Models.User;

namespace WorkBunny.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<UserModel>> GetUser()
    {
        var user = await _userManager.GetUserAsync(User);
        
        if (user == null)
        {
            return Unauthorized();
        }
        
        return Ok(new UserModel
        {
            Email = user.Email,
            UserName = user.UserName
        });
    }
}