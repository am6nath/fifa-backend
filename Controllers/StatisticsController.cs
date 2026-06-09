using fifa_backend.DTOs.Common;
using fifa_backend.DTOs.Statistics;
using fifa_backend.Services.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fifa_backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;

    public StatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    [HttpGet("results/{sessionId}")]
    public async Task<ActionResult<ApiResponse<List<VotingResultResponse>>>> GetResults(int sessionId)
    {
        var response = await _statisticsService.GetSessionResultsAsync(sessionId, ignorePublishRequirement: false);
        return Ok(ApiResponse<List<VotingResultResponse>>.SuccessResponse(response, "Session results retrieved successfully."));
    }

    [HttpGet("live/{sessionId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<VotingResultResponse>>>> GetLiveResults(int sessionId)
    {
        var response = await _statisticsService.GetSessionResultsAsync(sessionId, ignorePublishRequirement: true);
        return Ok(ApiResponse<List<VotingResultResponse>>.SuccessResponse(response, "Live session results retrieved successfully."));
    }

    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<DashboardStatsResponse>>> GetDashboardStats()
    {
        var response = await _statisticsService.GetDashboardStatsAsync();
        return Ok(ApiResponse<DashboardStatsResponse>.SuccessResponse(response, "Dashboard statistics retrieved successfully."));
    }
}
