namespace fifa_backend.Models;

public class Team : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string FlagUrl { get; set; } = string.Empty;

    public string GroupName { get; set; } = string.Empty;

    public string CoachName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}