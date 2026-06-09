namespace fifa_backend.DTOs.VotingSession;

public class VotingSessionResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime VotingStartAt { get; set; }
    public DateTime VotingEndAt { get; set; }
    public bool IsVotingClosedManually { get; set; }
    public bool ResultsPublished { get; set; }
    public DateTime? ResultsPublishedAt { get; set; }
    public string? PublishedBy { get; set; }
    public string? Notes { get; set; }
    public List<int> TeamIds { get; set; } = new();
    public string? RegionFilter { get; set; }
    public int TotalVotes { get; set; }
    public int WinnersCount { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
