using fifa_backend.DTOs.VotingSession;
using fifa_backend.Exceptions;
using fifa_backend.Models;
using fifa_backend.Repositories;
using fifa_backend.Services.Audit;
using fifa_backend.Services.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace fifa_backend.Services.VotingSession;

public class VotingSessionService : IVotingSessionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;

    public VotingSessionService(
        IUnitOfWork unitOfWork,
        IAuditLogService auditLogService,
        IHttpContextAccessor httpContextAccessor,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
    }

    public async Task<VotingSessionResponse> CreateAsync(CreateVotingSessionRequest request)
    {
        if (request.VotingStartAt >= request.VotingEndAt)
        {
            throw new BadRequestException("Start time must be before end time.");
        }

        // Overlap checks are disabled to allow concurrent active/upcoming polls.

        var session = new Models.VotingSession
        {
            Title = request.Title,
            VotingStartAt = request.VotingStartAt,
            VotingEndAt = request.VotingEndAt,
            Notes = request.Notes,
            RegionFilter = request.RegionFilter,
            WinnersCount = request.WinnersCount,
            IsVotingClosedManually = false,
            ResultsPublished = false
        };

        if (request.TeamIds != null && request.TeamIds.Any())
        {
            var teams = await _unitOfWork.Teams.Query()
                .Where(t => request.TeamIds.Contains(t.Id))
                .ToListAsync();

            if (teams.Count != request.TeamIds.Count)
            {
                throw new BadRequestException("One or more specified Team IDs do not exist.");
            }

            foreach (var team in teams)
            {
                session.Teams.Add(team);
            }
        }

        await _unitOfWork.VotingSessions.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        await _auditLogService.LogAsync(
            GetCurrentUserId(),
            "CREATE",
            "VotingSession",
            $"Created voting session: {session.Title} (ID: {session.Id})",
            GetCurrentIpAddress()
        );

        return MapToResponse(session);
    }

    public async Task<VotingSessionResponse> UpdateAsync(int id, UpdateVotingSessionRequest request)
    {
        if (request.VotingStartAt >= request.VotingEndAt)
        {
            throw new BadRequestException("Start time must be before end time.");
        }

        var session = await _unitOfWork.VotingSessions.Query()
            .Include(s => s.Teams)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session == null)
        {
            throw new NotFoundException($"Voting session with ID {id} not found.");
        }

        // Overlap checks are disabled to allow concurrent active/upcoming polls.

        session.Title = request.Title;
        session.VotingStartAt = request.VotingStartAt;
        session.VotingEndAt = request.VotingEndAt;
        session.Notes = request.Notes;
        session.RegionFilter = request.RegionFilter;
        session.WinnersCount = request.WinnersCount;

        session.Teams.Clear();
        if (request.TeamIds != null && request.TeamIds.Any())
        {
            var teams = await _unitOfWork.Teams.Query()
                .Where(t => request.TeamIds.Contains(t.Id))
                .ToListAsync();

            if (teams.Count != request.TeamIds.Count)
            {
                throw new BadRequestException("One or more specified Team IDs do not exist.");
            }

            foreach (var team in teams)
            {
                session.Teams.Add(team);
            }
        }

        _unitOfWork.VotingSessions.Update(session);
        await _unitOfWork.SaveChangesAsync();

        await _auditLogService.LogAsync(
            GetCurrentUserId(),
            "UPDATE",
            "VotingSession",
            $"Updated voting session: {session.Title} (ID: {session.Id})",
            GetCurrentIpAddress()
        );

        return MapToResponse(session);
    }

    public async Task CloseSessionAsync(int id)
    {
        var session = await _unitOfWork.VotingSessions.GetByIdAsync(id);
        if (session == null)
        {
            throw new NotFoundException($"Voting session with ID {id} not found.");
        }

        session.IsVotingClosedManually = true;
        _unitOfWork.VotingSessions.Update(session);
        await _unitOfWork.SaveChangesAsync();

        await _auditLogService.LogAsync(
            GetCurrentUserId(),
            "CLOSE",
            "VotingSession",
            $"Closed voting session manually: {session.Title} (ID: {session.Id})",
            GetCurrentIpAddress()
        );
    }

    public async Task PublishResultsAsync(int id, string publishedBy)
    {
        var session = await _unitOfWork.VotingSessions.GetByIdAsync(id);
        if (session == null)
        {
            throw new NotFoundException($"Voting session with ID {id} not found.");
        }

        if (!session.IsVotingClosedManually && DateTime.UtcNow < session.VotingEndAt)
        {
            throw new BadRequestException("Cannot publish results while voting is still active.");
        }

        session.ResultsPublished = true;
        session.ResultsPublishedAt = DateTime.UtcNow;
        session.PublishedBy = publishedBy;

        _unitOfWork.VotingSessions.Update(session);
        await _unitOfWork.SaveChangesAsync();

        await _auditLogService.LogAsync(
            GetCurrentUserId(),
            "PUBLISH",
            "VotingSession",
            $"Published results for session: {session.Title} (ID: {session.Id}) by {publishedBy}",
            GetCurrentIpAddress()
        );

        // Fetch all votes cast in this session (with voter profile details and team data)
        var votes = await _unitOfWork.Votes.Query()
            .Where(v => v.VotingSessionId == id)
            .Include(v => v.User)
            .Include(v => v.Team)
            .ToListAsync();

        if (votes.Any())
        {
            // Determine the session results and winners
            var totalVotesCount = votes.Count;
            var teamVotes = votes
                .GroupBy(v => v.TeamId)
                .Select(g => new { TeamId = g.Key, VoteCount = g.Count() })
                .OrderByDescending(g => g.VoteCount)
                .ToList();

            var winnersCount = session.WinnersCount > 0 ? session.WinnersCount : 1;
            var winnersList = new List<string>();

            for (int i = 0; i < Math.Min(winnersCount, teamVotes.Count); i++)
            {
                var tv = teamVotes[i];
                var teamName = votes.First(v => v.TeamId == tv.TeamId).Team.Name;
                var percentage = totalVotesCount > 0 ? Math.Round((double)tv.VoteCount / totalVotesCount * 100, 2) : 0;
                winnersList.Add($"{teamName} - {tv.VoteCount} votes ({percentage}%)");
            }

            if (!winnersList.Any())
            {
                winnersList.Add("No votes cast.");
            }

            // Dispatch notification emails to all verified voters who participated in parallel
            var emailTasks = votes
                .Where(v => v.User != null && !string.IsNullOrEmpty(v.User.Email) && v.User.EmailVerified && v.User.IsActive)
                .Select(async vote =>
                {
                    try
                    {
                        await _emailService.SendResultsEmailAsync(
                            vote.User.Email,
                            vote.User.UserName,
                            session.Title,
                            vote.Team.Name,
                            winnersList
                        );
                    }
                    catch (Exception)
                    {
                        // Log warnings/exceptions inside tasks to prevent blocking
                    }
                });

            await Task.WhenAll(emailTasks);
        }
    }

    public async Task<VotingSessionResponse> GetByIdAsync(int id)
    {
        var session = await _unitOfWork.VotingSessions.Query()
            .Include(s => s.Teams)
            .Include(s => s.Votes)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session == null)
        {
            throw new NotFoundException($"Voting session with ID {id} not found.");
        }

        return MapToResponse(session);
    }

    public async Task<List<VotingSessionResponse>> GetAllAsync()
    {
        var sessions = await _unitOfWork.VotingSessions.Query()
            .Include(s => s.Teams)
            .Include(s => s.Votes)
            .AsNoTracking()
            .OrderByDescending(s => s.VotingStartAt)
            .ToListAsync();

        return sessions.Select(MapToResponse).ToList();
    }

    public async Task<List<VotingSessionResponse>> GetActiveSessionsAsync()
    {
        var now = DateTime.UtcNow;
        var sessions = await _unitOfWork.VotingSessions.Query()
            .Include(s => s.Teams)
            .Include(s => s.Votes)
            .AsNoTracking()
            .Where(s => s.VotingStartAt <= now && s.VotingEndAt >= now && !s.IsVotingClosedManually)
            .ToListAsync();

        return sessions.Select(MapToResponse).ToList();
    }

    private static VotingSessionResponse MapToResponse(Models.VotingSession session)
    {
        var now = DateTime.UtcNow;
        var isActive = now >= session.VotingStartAt && now <= session.VotingEndAt && !session.IsVotingClosedManually;

        string status;
        if (session.IsVotingClosedManually || now > session.VotingEndAt)
        {
            status = "Closed";
        }
        else if (now < session.VotingStartAt)
        {
            status = "Upcoming";
        }
        else
        {
            status = "Active";
        }

        return new VotingSessionResponse
        {
            Id = session.Id,
            Title = session.Title,
            VotingStartAt = session.VotingStartAt,
            VotingEndAt = session.VotingEndAt,
            IsVotingClosedManually = session.IsVotingClosedManually,
            ResultsPublished = session.ResultsPublished,
            ResultsPublishedAt = session.ResultsPublishedAt,
            PublishedBy = session.PublishedBy,
            Notes = session.Notes,
            TeamIds = session.Teams?.Select(t => t.Id).ToList() ?? new List<int>(),
            RegionFilter = session.RegionFilter,
            TotalVotes = session.Votes?.Count ?? 0,
            WinnersCount = session.WinnersCount,
            IsActive = isActive,
            Status = status,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt
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
