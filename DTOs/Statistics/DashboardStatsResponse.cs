namespace fifa_backend.DTOs.Statistics;

public class DashboardStatsResponse
{
    public int TotalUsers { get; set; }
    public int TotalVotes { get; set; }
    public int TotalTeams { get; set; }
    public int TotalSessions { get; set; }
    public int ActiveSessions { get; set; }
}
