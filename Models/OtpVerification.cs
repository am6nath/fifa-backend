namespace fifa_backend.Models;

public class OtpVerification : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    public string OtpCode { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; } = false;
    public int AttemptCount { get; set; }

    public DateTime? VerifiedAt { get; set; }
}