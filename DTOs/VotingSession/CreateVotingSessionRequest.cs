using System.ComponentModel.DataAnnotations;

namespace fifa_backend.DTOs.VotingSession;

public class CreateVotingSessionRequest
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start time is required.")]
    public DateTime VotingStartAt { get; set; }

    [Required(ErrorMessage = "End time is required.")]
    public DateTime VotingEndAt { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string? Notes { get; set; }

    public List<int>? TeamIds { get; set; }

    [StringLength(50, ErrorMessage = "Region filter cannot exceed 50 characters.")]
    public string? RegionFilter { get; set; }

    [Range(1, 32, ErrorMessage = "Winners count must be between 1 and 32.")]
    public int WinnersCount { get; set; } = 1;
}
