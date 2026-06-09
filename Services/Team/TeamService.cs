using fifa_backend.DTOs.Team;
using fifa_backend.Exceptions;
using fifa_backend.Models;
using fifa_backend.Repositories;
using fifa_backend.Services.Audit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace fifa_backend.Services.Team;

public class TeamService : ITeamService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TeamService(
        IUnitOfWork unitOfWork,
        IAuditLogService auditLogService,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TeamResponse> CreateAsync(CreateTeamRequest request)
    {
        if (await _unitOfWork.Teams.ExistsAsync(t => t.Name == request.Name))
        {
            throw new ConflictException($"A team with name '{request.Name}' already exists.");
        }

        var team = new Models.Team
        {
            Name = request.Name,
            CountryCode = request.CountryCode,
            FlagUrl = request.FlagUrl ?? string.Empty,
            GroupName = request.GroupName,
            CoachName = request.CoachName ?? string.Empty,
            Description = request.Description ?? string.Empty,
            IsActive = true,
            Region = request.Region
        };

        await _unitOfWork.Teams.AddAsync(team);
        await _unitOfWork.SaveChangesAsync();

        await _auditLogService.LogAsync(
            GetCurrentUserId(),
            "CREATE",
            "Team",
            $"Created team: {team.Name} (ID: {team.Id})",
            GetCurrentIpAddress()
        );

        return MapToResponse(team);
    }

    public async Task<TeamResponse> UpdateAsync(int id, UpdateTeamRequest request)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(id);
        if (team == null)
        {
            throw new NotFoundException($"Team with ID {id} not found.");
        }

        if (await _unitOfWork.Teams.ExistsAsync(t => t.Name == request.Name && t.Id != id))
        {
            throw new ConflictException($"Another team with name '{request.Name}' already exists.");
        }

        team.Name = request.Name;
        team.CountryCode = request.CountryCode;
        team.FlagUrl = request.FlagUrl ?? string.Empty;
        team.GroupName = request.GroupName;
        team.CoachName = request.CoachName ?? string.Empty;
        team.Description = request.Description ?? string.Empty;
        team.Region = request.Region;
        team.IsActive = request.IsActive;

        _unitOfWork.Teams.Update(team);
        await _unitOfWork.SaveChangesAsync();

        await _auditLogService.LogAsync(
            GetCurrentUserId(),
            "UPDATE",
            "Team",
            $"Updated team: {team.Name} (ID: {team.Id})",
            GetCurrentIpAddress()
        );

        return MapToResponse(team);
    }

    public async Task SoftDeleteAsync(int id)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(id);
        if (team == null)
        {
            throw new NotFoundException($"Team with ID {id} not found.");
        }

        team.IsActive = false;
        await _unitOfWork.Teams.SoftDeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        await _auditLogService.LogAsync(
            GetCurrentUserId(),
            "DELETE",
            "Team",
            $"Soft-deleted team: {team.Name} (ID: {team.Id})",
            GetCurrentIpAddress()
        );
    }

    public async Task<TeamResponse> GetByIdAsync(int id)
    {
        var team = await _unitOfWork.Teams.Query()
            .Include(t => t.Votes)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null)
        {
            throw new NotFoundException($"Team with ID {id} not found.");
        }

        return MapToResponse(team);
    }

    public async Task<List<TeamResponse>> GetAllAsync(bool includeInactive = false)
    {
        var query = _unitOfWork.Teams.Query()
            .Include(t => t.Votes)
            .AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        var teams = await query.ToListAsync();
        return teams.Select(MapToResponse).ToList();
    }

    private static TeamResponse MapToResponse(Models.Team team)
    {
        return new TeamResponse
        {
            Id = team.Id,
            Name = team.Name,
            CountryCode = team.CountryCode,
            FlagUrl = team.FlagUrl,
            GroupName = team.GroupName,
            CoachName = team.CoachName,
            Description = team.Description,
            IsActive = team.IsActive,
            Region = team.Region,
            TotalVotes = team.Votes?.Count ?? 0,
            CreatedAt = team.CreatedAt,
            UpdatedAt = team.UpdatedAt
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
