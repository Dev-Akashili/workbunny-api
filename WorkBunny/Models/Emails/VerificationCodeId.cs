namespace WorkBunny.Models.Emails;

public class VerificationCodeId
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}