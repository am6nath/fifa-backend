using fifa_backend.DTOs.Auth;
using fifa_backend.DTOs.Common;
using fifa_backend.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace fifa_backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("send-otp")]
    public async Task<ActionResult<ApiResponse>> SendOtp(SendOtpRequest request)
    {
        await _authService.SendOtpAsync(request);
        return Ok(ApiResponse.Ok("OTP sent successfully."));
    }

    [HttpPost("verify-otp")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> VerifyOtp(VerifyOtpRequest request)
    {
        var authResponse = await _authService.VerifyOtpAsync(request);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(authResponse, "OTP verified successfully."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request)
    {
        var authResponse = await _authService.LoginAsync(request);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(authResponse, "Logged in successfully."));
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse>> Register(RegisterRequest request)
    {
        await _authService.RegisterAsync(request);
        return Ok(ApiResponse.Ok("Registration successful. Please verify the OTP sent to your email."));
    }
}