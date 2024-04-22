using Microsoft.AspNetCore.Identity;

namespace WorkBunny.Data.Entities.Identity;

public class ApplicationUser : IdentityUser
{
    public string Country { get; set; } = string.Empty;
    public string CustomUserName { get; set; } = string.Empty;
}