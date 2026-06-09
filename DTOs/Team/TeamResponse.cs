namespace fifa_backend.DTOs.Team;

public class TeamResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string FlagUrl { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string CoachName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Region { get; set; } = string.Empty;
    public int TotalVotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
