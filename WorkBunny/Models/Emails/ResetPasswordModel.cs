namespace WorkBunny.Models.Emails;

public class ResetPasswordModel : ValidateEmailModel
{
    public string NewPassword { get; set; } = string.Empty;
}