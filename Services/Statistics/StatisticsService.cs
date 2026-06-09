using fifa_backend.DTOs.Statistics;
using fifa_backend.Exceptions;
using fifa_backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace fifa_backend.Services.Statistics;

public class StatisticsService : IStatisticsService
{
    private readonly IUnitOfWork _unitOfWork;

    public StatisticsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<VotingResultResponse>> GetSessionResultsAsync(int sessionId, bool ignorePublishRequirement = false)
    {
        var session = await _unitOfWork.VotingSessions.GetByIdAsync(sessionId);
        if (session == null)
        {
            throw new NotFoundException($"Voting session with ID {sessionId} not found.");
        }

        if (!session.ResultsPublished && !ignorePublishRequirement)
        {
            throw new ForbiddenException("Results for this voting session have not been published yet.");
        }

        // Get total vote count for the session
        var totalVotes = await _unitOfWork.Votes.Query()
            .Where(v => v.VotingSessionId == sessionId)
            .CountAsync();

        // Get vote counts per team
        var teamVotes = await _unitOfWork.Votes.Query()
            .Where(v => v.VotingSessionId == sessionId)
            .GroupBy(v => v.TeamId)
            .Select(g => new { TeamId = g.Key, VoteCount = g.Count() })
            .ToListAsync();

        var teamVotesDict = teamVotes.ToDictionary(x => x.TeamId, x => x.VoteCount);

        // Fetch all teams to include even those with 0 votes
        var teams = await _unitOfWork.Teams.Query().AsNoTracking().ToListAsync();

        var resultsList = new List<VotingResultResponse>();
        foreach (var team in teams)
        {
            int count = teamVotesDict.TryGetValue(team.Id, out var val) ? val : 0;

            // Only show active teams or inactive teams that have actual votes in this session
            if (team.IsActive || count > 0)
            {
                resultsList.Add(new VotingResultResponse
                {
                    TeamId = team.Id,
                    TeamName = team.Name,
                    CountryCode = team.CountryCode,
                    FlagUrl = team.FlagUrl,
                    GroupName = team.GroupName,
                    VoteCount = count,
                    Percentage = totalVotes > 0 ? Math.Round((double)count / totalVotes * 100, 2) : 0
                });
            }
        }

        // Sort descending by vote count, then by team name alphabetically
        var sortedResults = resultsList
            .OrderByDescending(r => r.VoteCount)
            .ThenBy(r => r.TeamName)
            .ToList();

        // Assign ranks
        for (int i = 0; i < sortedResults.Count; i++)
        {
            sortedResults[i].Rank = i + 1;
        }

        return sortedResults;
    }

    public async Task<DashboardStatsResponse> GetDashboardStatsAsync()
    {
        var now = DateTime.UtcNow;
        var totalUsers = await _unitOfWork.Users.Query().CountAsync();
        var totalVotes = await _unitOfWork.Votes.Query().CountAsync();
        var totalTeams = await _unitOfWork.Teams.Query().CountAsync();
        var totalSessions = await _unitOfWork.VotingSessions.Query().CountAsync();
        
        var activeSessions = await _unitOfWork.VotingSessions.Query()
            .CountAsync(s => s.VotingStartAt <= now && s.VotingEndAt >= now && !s.IsVotingClosedManually);

        return new DashboardStatsResponse
        {
            TotalUsers = totalUsers,
            TotalVotes = totalVotes,
            TotalTeams = totalTeams,
            TotalSessions = totalSessions,
            ActiveSessions = activeSessions
        };
    }
}
