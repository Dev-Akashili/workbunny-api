using Microsoft.AspNetCore.Identity;

namespace WorkBunny.Data.Entities.Identity;

public class ApplicationUser : IdentityUser
{
    public string CustomUserName { get; set; } = string.Empty;
}