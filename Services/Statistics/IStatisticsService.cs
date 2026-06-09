using fifa_backend.DTOs.Statistics;

namespace fifa_backend.Services.Statistics;

public interface IStatisticsService
{
    Task<List<VotingResultResponse>> GetSessionResultsAsync(int sessionId, bool ignorePublishRequirement = false);
    Task<DashboardStatsResponse> GetDashboardStatsAsync();
}
