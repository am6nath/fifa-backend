using fifa_backend.DTOs.Auth;

namespace fifa_backend.Services.Auth;

public interface IAuthService
{
    Task<bool> SendOtpAsync(SendOtpRequest request);

    Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request);

    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<bool> RegisterAsync(RegisterRequest request);
}