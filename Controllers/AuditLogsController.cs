using fifa_backend.DTOs.Audit;
using fifa_backend.DTOs.Common;
using fifa_backend.Services.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fifa_backend.Controllers;

[ApiController]
[Route("api/v1/audit-logs")]
[Authorize(Roles = "Admin")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<AuditLogResponse>>>> GetLogs([FromQuery] AuditLogFilter filter)
    {
        var response = await _auditLogService.GetLogsAsync(filter);
        return Ok(ApiResponse<PagedResponse<AuditLogResponse>>.SuccessResponse(response, "Audit logs retrieved successfully."));
    }
}
