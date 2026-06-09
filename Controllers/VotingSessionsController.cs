using fifa_backend.DTOs.Common;
using fifa_backend.DTOs.VotingSession;
using fifa_backend.Services.VotingSession;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fifa_backend.Controllers;

[ApiController]
[Route("api/v1/voting-sessions")]
public class VotingSessionsController : ControllerBase
{
    private readonly IVotingSessionService _sessionService;

    public VotingSessionsController(IVotingSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<VotingSessionResponse>>> Create(CreateVotingSessionRequest request)
    {
        var response = await _sessionService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, ApiResponse<VotingSessionResponse>.SuccessResponse(response, "Voting session created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<VotingSessionResponse>>> Update(int id, UpdateVotingSessionRequest request)
    {
        var response = await _sessionService.UpdateAsync(id, request);
        return Ok(ApiResponse<VotingSessionResponse>.SuccessResponse(response, "Voting session updated successfully."));
    }

    [HttpPost("{id}/close")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> Close(int id)
    {
        await _sessionService.CloseSessionAsync(id);
        return Ok(ApiResponse.Ok("Voting session closed successfully."));
    }

    [HttpPost("{id}/publish")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> Publish(int id)
    {
        var publishedBy = User.Identity?.Name ?? "Admin";
        await _sessionService.PublishResultsAsync(id, publishedBy);
        return Ok(ApiResponse.Ok("Voting results published successfully."));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<VotingSessionResponse>>> GetById(int id)
    {
        var response = await _sessionService.GetByIdAsync(id);
        return Ok(ApiResponse<VotingSessionResponse>.SuccessResponse(response, "Voting session retrieved successfully."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<VotingSessionResponse>>>> GetAll()
    {
        var response = await _sessionService.GetAllAsync();
        return Ok(ApiResponse<List<VotingSessionResponse>>.SuccessResponse(response, "Voting sessions retrieved successfully."));
    }

    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<List<VotingSessionResponse>>>> GetActive()
    {
        var response = await _sessionService.GetActiveSessionsAsync();
        return Ok(ApiResponse<List<VotingSessionResponse>>.SuccessResponse(response, "Active voting sessions retrieved successfully."));
    }
}
