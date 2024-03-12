namespace WorkBunny.Models.Emails;

public class ValidateEmailModel
{
    public int CodeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTimeOffset CurrentTime { get; set; } = DateTime.UtcNow;
}