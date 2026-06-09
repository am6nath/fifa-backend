using fifa_backend.DTOs.Common;
using fifa_backend.DTOs.Team;
using fifa_backend.Services.Team;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fifa_backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<TeamResponse>>> Create(CreateTeamRequest request)
    {
        var response = await _teamService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, ApiResponse<TeamResponse>.SuccessResponse(response, "Team created successfully."));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<TeamResponse>>> Update(int id, UpdateTeamRequest request)
    {
        var response = await _teamService.UpdateAsync(id, request);
        return Ok(ApiResponse<TeamResponse>.SuccessResponse(response, "Team updated successfully."));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        await _teamService.SoftDeleteAsync(id);
        return Ok(ApiResponse.Ok("Team deleted successfully."));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TeamResponse>>> GetById(int id)
    {
        var response = await _teamService.GetByIdAsync(id);
        return Ok(ApiResponse<TeamResponse>.SuccessResponse(response, "Team retrieved successfully."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TeamResponse>>>> GetAll([FromQuery] bool includeInactive = false)
    {
        var isAdmin = User.IsInRole("Admin");
        var response = await _teamService.GetAllAsync(includeInactive && isAdmin);
        return Ok(ApiResponse<List<TeamResponse>>.SuccessResponse(response, "Teams retrieved successfully."));
    }
}
