namespace fifa_backend.Models;

public class AuditLog : BaseEntity
{
    public int? UserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? IpAddress { get; set; }
}