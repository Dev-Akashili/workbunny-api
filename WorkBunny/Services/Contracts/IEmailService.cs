using WorkBunny.Data.Entities.Emails;
using WorkBunny.Models.Emails;

namespace WorkBunny.Services.Contracts;

public interface IEmailService
{
    Task SendEmailVerificationLink(string to);
    Task<string> ValidateCode(ValidateEmailModel model, bool request);
}