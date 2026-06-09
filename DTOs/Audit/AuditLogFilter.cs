using System.ComponentModel.DataAnnotations;

namespace fifa_backend.DTOs.Audit;

public class AuditLogFilter
{
    public string? Action { get; set; }
    public string? EntityName { get; set; }
    public int? UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1.")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100.")]
    public int PageSize { get; set; } = 20;
}
