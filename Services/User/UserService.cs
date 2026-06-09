using fifa_backend.DTOs.Common;
using fifa_backend.DTOs.User;
using fifa_backend.Exceptions;
using fifa_backend.Models.Enums;
using fifa_backend.Repositories;
using fifa_backend.Services.Audit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace fifa_backend.Services.Users;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(
        IUnitOfWork unitOfWork,
        IAuditLogService auditLogService,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResponse<UserResponse>> GetUsersAsync(UserFilter filter)
    {
        var query = _unitOfWork.Users.Query()
            .Include(u => u.Votes)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(filter.Search))
        {
            query = query.Where(u => u.UserName.Contains(filter.Search) || u.Email.Contains(filter.Search));
        }

        if (!string.IsNullOrEmpty(filter.Role) && Enum.TryParse<UserRole>(filter.Role, true, out var roleEnum))
        {
            query = query.Where(u => u.Role == roleEnum);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == filter.IsActive.Value);
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var mapped = users.Select(MapToResponse).ToList();

        return new PagedResponse<UserResponse>(mapped, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<UserResponse> GetByIdAsync(int id)
    {
        var user = await _unitOfWork.Users.Query()
            .Include(u => u.Votes)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new NotFoundException($"User with ID {id} not found.");
        }

        return MapToResponse(user);
    }

    public async Task<UserResponse> GetProfileAsync(int userId)
    {
        var user = await _unitOfWork.Users.Query()
            .Include(u => u.Votes)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new NotFoundException("Profile not found.");
        }

        return MapToResponse(user);
    }

    public async Task<UserResponse> ToggleActiveStatusAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {id} not found.");
        }

        user.IsActive = !user.IsActive;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        await _auditLogService.LogAsync(
            GetCurrentUserId(),
            "TOGGLE_USER_ACTIVE",
            "User",
            $"Toggled user active state to {user.IsActive} for user: {user.Email} (ID: {user.Id})",
            GetCurrentIpAddress()
        );

        // Fetch user with votes count to match response shape
        var updatedUser = await _unitOfWork.Users.Query()
            .Include(u => u.Votes)
            .FirstAsync(u => u.Id == id);

        return MapToResponse(updatedUser);
    }

    private static UserResponse MapToResponse(Models.User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            EmailVerified = user.EmailVerified,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            VoteCount = user.Votes?.Count ?? 0
        };
    }

    private int? GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
    }

    private string? GetCurrentIpAddress()
    {
        return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
    }
}
