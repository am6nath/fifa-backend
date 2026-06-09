namespace fifa_backend.DTOs.Vote;

public class VoteResponse
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string TeamCountryCode { get; set; } = string.Empty;
    public string TeamFlagUrl { get; set; } = string.Empty;
    public int VotingSessionId { get; set; }
    public string SessionTitle { get; set; } = string.Empty;
    public DateTime VotedAt { get; set; }
    public bool ResultsPublished { get; set; }
}
