using System.ComponentModel.DataAnnotations;

namespace fifa_backend.DTOs.Team;

public class UpdateTeamRequest
{
    [Required(ErrorMessage = "Team name is required.")]
    [StringLength(100, ErrorMessage = "Team name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Country code is required.")]
    [StringLength(3, MinimumLength = 2, ErrorMessage = "Country code must be between 2 and 3 characters.")]
    public string CountryCode { get; set; } = string.Empty;

    [Url(ErrorMessage = "Flag URL must be a valid URL.")]
    public string? FlagUrl { get; set; }

    [Required(ErrorMessage = "Group name is required.")]
    [StringLength(10, ErrorMessage = "Group name cannot exceed 10 characters.")]
    public string GroupName { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Coach name cannot exceed 100 characters.")]
    public string? CoachName { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Region is required.")]
    [StringLength(50, ErrorMessage = "Region cannot exceed 50 characters.")]
    public string Region { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
