using fifa_backend.DTOs.Common;
using fifa_backend.DTOs.User;
using fifa_backend.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace fifa_backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PagedResponse<UserResponse>>>> GetUsers([FromQuery] UserFilter filter)
    {
        var response = await _userService.GetUsersAsync(filter);
        return Ok(ApiResponse<PagedResponse<UserResponse>>.SuccessResponse(response, "Users list retrieved successfully."));
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetProfile()
    {
        var userId = GetCurrentUserId();
        var response = await _userService.GetProfileAsync(userId);
        return Ok(ApiResponse<UserResponse>.SuccessResponse(response, "User profile retrieved successfully."));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetById(int id)
    {
        var response = await _userService.GetByIdAsync(id);
        return Ok(ApiResponse<UserResponse>.SuccessResponse(response, "User details retrieved successfully."));
    }

    [HttpPut("{id}/toggle-active")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> ToggleActive(int id)
    {
        var response = await _userService.ToggleActiveStatusAsync(id);
        return Ok(ApiResponse<UserResponse>.SuccessResponse(response, "User status toggled successfully."));
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
