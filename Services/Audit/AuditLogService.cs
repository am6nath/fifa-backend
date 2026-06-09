using fifa_backend.DTOs.Audit;
using fifa_backend.DTOs.Common;
using fifa_backend.Models;
using fifa_backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace fifa_backend.Services.Audit;

public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditLogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task LogAsync(int? userId, string action, string entityName, string description, string? ipAddress = null)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            Description = description,
            IpAddress = ipAddress
        };

        await _unitOfWork.AuditLogs.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PagedResponse<AuditLogResponse>> GetLogsAsync(AuditLogFilter filter)
    {
        var query = _unitOfWork.AuditLogs.Query()
            .Include(x => x.User)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(filter.Action))
        {
            query = query.Where(x => x.Action == filter.Action);
        }

        if (!string.IsNullOrEmpty(filter.EntityName))
        {
            query = query.Where(x => x.EntityName == filter.EntityName);
        }

        if (filter.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == filter.UserId.Value);
        }

        if (!string.IsNullOrEmpty(filter.Username))
        {
            query = query.Where(x => x.User != null && x.User.UserName.Contains(filter.Username));
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= filter.EndDate.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new AuditLogResponse
            {
                Id = x.Id,
                UserId = x.UserId,
                UserName = x.User != null ? x.User.UserName : null,
                Action = x.Action,
                EntityName = x.EntityName,
                Description = x.Description,
                IpAddress = x.IpAddress,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return new PagedResponse<AuditLogResponse>(items, totalCount, filter.PageNumber, filter.PageSize);
    }
}
