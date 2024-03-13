using WorkBunny.Data.Entities.Emails;
using WorkBunny.Models.Emails;

namespace WorkBunny.Services.Contracts;

public interface IEmailService
{
    Task<int> SendEmailVerificationCode(string to);
    Task<string> ValidateCode(ValidateEmailModel model, bool request);
    Task ClearValidationCodes(IEnumerable<VerificationCode> list);
}