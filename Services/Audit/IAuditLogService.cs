using fifa_backend.DTOs.Audit;
using fifa_backend.DTOs.Common;

namespace fifa_backend.Services.Audit;

public interface IAuditLogService
{
    Task LogAsync(int? userId, string action, string entityName, string description, string? ipAddress = null);
    Task<PagedResponse<AuditLogResponse>> GetLogsAsync(AuditLogFilter filter);
}
