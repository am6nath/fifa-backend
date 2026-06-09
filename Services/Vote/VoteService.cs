using fifa_backend.DTOs.Vote;
using fifa_backend.Exceptions;
using fifa_backend.Models;
using fifa_backend.Repositories;
using fifa_backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace fifa_backend.Services.Vote;

public class VoteService : IVoteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;

    public VoteService(
        IUnitOfWork unitOfWork,
        IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }

    public async Task<VoteResponse> CastVoteAsync(int userId, CastVoteRequest request, string? ipAddress, string? userAgent)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Layer 2: Lock the user row in database to block concurrent vote requests for same user
            await _unitOfWork.LockUserAsync(userId);

            // Verify the user exists, is active, and is verified
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || !user.IsActive || user.IsDeleted || !user.EmailVerified)
            {
                throw new BadRequestException("User account is inactive, deactivated, or not verified.");
            }

            // Fetch active session
            var now = DateTime.UtcNow;
            var activeSession = await _unitOfWork.VotingSessions.Query()
                .Include(s => s.Teams)
                .FirstOrDefaultAsync(s => s.VotingStartAt <= now && s.VotingEndAt >= now && !s.IsVotingClosedManually);

            if (activeSession == null)
            {
                throw new BadRequestException("There is no active voting session at this time.");
            }

            // Layer 1: App-level double-check under the lock
            var hasVoted = await _unitOfWork.Votes.ExistsAsync(v => v.UserId == userId && v.VotingSessionId == activeSession.Id);
            if (hasVoted)
            {
                throw new ConflictException("You have already cast a vote in this voting session.");
            }

            // Verify team exists and is active
            var team = await _unitOfWork.Teams.GetByIdAsync(request.TeamId);
            if (team == null || !team.IsActive)
            {
                throw new NotFoundException("The selected team does not exist or is inactive.");
            }

            // Verify team eligibility in the session
            if (activeSession.Teams != null && activeSession.Teams.Any())
            {
                var isEligible = activeSession.Teams.Any(t => t.Id == request.TeamId);
                if (!isEligible)
                {
                    throw new BadRequestException("The selected team is not eligible in this voting session.");
                }
            }

            if (!string.IsNullOrEmpty(activeSession.RegionFilter))
            {
                if (!string.Equals(team.Region, activeSession.RegionFilter, StringComparison.OrdinalIgnoreCase))
                {
                    throw new BadRequestException($"The selected team is not in the '{activeSession.RegionFilter}' region.");
                }
            }

            // Create vote
            var vote = new Models.Vote
            {
                UserId = userId,
                TeamId = request.TeamId,
                VotingSessionId = activeSession.Id,
                VotedAt = DateTime.UtcNow,
                VotedByIp = ipAddress,
                UserAgent = userAgent
            };

            await _unitOfWork.Votes.AddAsync(vote);
            await _unitOfWork.SaveChangesAsync();

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync();

            // Fire-and-forget Audit logging
            await _auditLogService.LogAsync(
                userId,
                "VOTE",
                "Vote",
                $"Cast vote for team '{team.Name}' (ID: {team.Id}) in session '{activeSession.Title}' (ID: {activeSession.Id})",
                ipAddress
            );

            return new VoteResponse
            {
                Id = vote.Id,
                TeamId = vote.TeamId,
                TeamName = team.Name,
                TeamCountryCode = team.CountryCode,
                TeamFlagUrl = team.FlagUrl,
                VotingSessionId = vote.VotingSessionId,
                SessionTitle = activeSession.Title,
                VotedAt = vote.VotedAt,
                ResultsPublished = activeSession.ResultsPublished
            };
        }
        catch (Exception)
        {
            // Rollback on any exception
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<VoteResponse?> GetUserVoteAsync(int userId, int sessionId)
    {
        var vote = await _unitOfWork.Votes.Query()
            .Include(v => v.Team)
            .Include(v => v.VotingSession)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.UserId == userId && v.VotingSessionId == sessionId);

        if (vote == null)
        {
            return null;
        }

        return new VoteResponse
        {
            Id = vote.Id,
            TeamId = vote.TeamId,
            TeamName = vote.Team.Name,
            TeamCountryCode = vote.Team.CountryCode,
            TeamFlagUrl = vote.Team.FlagUrl,
            VotingSessionId = vote.VotingSessionId,
            SessionTitle = vote.VotingSession.Title,
            VotedAt = vote.VotedAt,
            ResultsPublished = vote.VotingSession.ResultsPublished
        };
    }

    public async Task<bool> HasUserVotedAsync(int userId, int sessionId)
    {
        return await _unitOfWork.Votes.ExistsAsync(v => v.UserId == userId && v.VotingSessionId == sessionId);
    }

    public async Task<List<VoteResponse>> GetUserVoteHistoryAsync(int userId)
    {
        var votes = await _unitOfWork.Votes.Query()
            .Include(v => v.Team)
            .Include(v => v.VotingSession)
            .AsNoTracking()
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.VotedAt)
            .ToListAsync();

        var responseList = new List<VoteResponse>();
        foreach (var vote in votes)
        {
            responseList.Add(new VoteResponse
            {
                Id = vote.Id,
                TeamId = vote.TeamId,
                TeamName = vote.Team.Name,
                TeamCountryCode = vote.Team.CountryCode,
                TeamFlagUrl = vote.Team.FlagUrl,
                VotingSessionId = vote.VotingSessionId,
                SessionTitle = vote.VotingSession.Title,
                VotedAt = vote.VotedAt,
                ResultsPublished = vote.VotingSession.ResultsPublished
            });
        }
        return responseList;
    }
}
