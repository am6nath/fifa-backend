namespace fifa_backend.Models;

public class Vote : BaseEntity
{
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public int TeamId { get; set; }

    public Team Team { get; set; } = null!;

    public DateTime VotedAt { get; set; } = DateTime.UtcNow;

    public string VotedByIp { get; set; } = string.Empty;
}