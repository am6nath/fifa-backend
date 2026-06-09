using System.ComponentModel.DataAnnotations;

namespace fifa_backend.DTOs.Vote;

public class CastVoteRequest
{
    [Required(ErrorMessage = "Team ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "A valid Team ID is required.")]
    public int TeamId { get; set; }
}
