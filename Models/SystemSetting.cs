namespace fifa_backend.Models;

public class VotingSession : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public DateTime VotingStartAt { get; set; }

    public DateTime VotingEndAt { get; set; }

    public bool IsVotingClosedManually { get; set; }

    public bool ResultsPublished { get; set; }

    public DateTime? ResultsPublishedAt { get; set; }
}