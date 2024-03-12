using WorkBunny.Models.Emails;

namespace WorkBunny.Services.Contracts;

public interface IEmailService
{
    Task<int> SendEmailVerificationCode(string to);
    Task<string> ValidateCode(ValidateEmailModel model);
}