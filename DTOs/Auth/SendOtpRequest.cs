using System.ComponentModel.DataAnnotations;

namespace fifa_backend.DTOs.Auth;

public class SendOtpRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string Email { get; set; } = string.Empty;
}