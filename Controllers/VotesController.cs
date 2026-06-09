using fifa_backend.DTOs.Common;
using fifa_backend.DTOs.Vote;
using fifa_backend.Services.Vote;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace fifa_backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class VotesController : ControllerBase
{
    private readonly IVoteService _voteService;

    public VotesController(IVoteService voteService)
    {
        _voteService = voteService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<VoteResponse>>> CastVote(CastVoteRequest request)
    {
        var userId = GetCurrentUserId();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers["User-Agent"].ToString();

        var response = await _voteService.CastVoteAsync(userId, request, ipAddress, userAgent);
        return Ok(ApiResponse<VoteResponse>.SuccessResponse(response, "Vote cast successfully."));
    }

    [HttpGet("my-vote")]
    public async Task<ActionResult<ApiResponse<VoteResponse>>> GetMyVote([FromQuery] int sessionId)
    {
        var userId = GetCurrentUserId();
        var response = await _voteService.GetUserVoteAsync(userId, sessionId);
        
        if (response == null)
        {
            return Ok(ApiResponse<VoteResponse>.SuccessResponse(null, "No vote cast in this session yet."));
        }
        
        return Ok(ApiResponse<VoteResponse>.SuccessResponse(response, "Vote retrieved successfully."));
    }

    [HttpGet("has-voted")]
    public async Task<ActionResult<ApiResponse<bool>>> HasVoted([FromQuery] int sessionId)
    {
        var userId = GetCurrentUserId();
        var hasVoted = await _voteService.HasUserVotedAsync(userId, sessionId);
        return Ok(ApiResponse<bool>.SuccessResponse(hasVoted, "Voted status retrieved successfully."));
    }

    [HttpGet("my-history")]
    public async Task<ActionResult<ApiResponse<List<VoteResponse>>>> GetMyHistory()
    {
        var userId = GetCurrentUserId();
        var history = await _voteService.GetUserVoteHistoryAsync(userId);
        return Ok(ApiResponse<List<VoteResponse>>.SuccessResponse(history, "Voting history retrieved successfully."));
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out var userId))
        {
            throw new Exceptions.BadRequestException("User session is invalid. Re-authenticate.");
        }
        return userId;
    }
}
