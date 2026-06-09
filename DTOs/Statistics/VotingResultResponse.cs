namespace fifa_backend.DTOs.Statistics;

public class VotingResultResponse
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string FlagUrl { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public int VoteCount { get; set; }
    public double Percentage { get; set; }
    public int Rank { get; set; }
}
