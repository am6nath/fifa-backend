namespace fifa_backend.Models;

public class VotingSession : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public DateTime VotingStartAt { get; set; }

    public DateTime VotingEndAt { get; set; }

    public bool IsVotingClosedManually { get; set; }

    public bool ResultsPublished { get; set; }

    public DateTime? ResultsPublishedAt { get; set; }

    public string? PublishedBy { get; set; }

    public string? Notes { get; set; }

    public string? RegionFilter { get; set; }
    
    public int WinnersCount { get; set; } = 1;

    public ICollection<Vote> Votes { get; set; } = new List<Vote>();

    public ICollection<Team> Teams { get; set; } = new List<Team>();
}