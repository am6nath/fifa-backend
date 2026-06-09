using fifa_backend.Models;

namespace fifa_backend.Services.Auth;

public interface IJwtService
{
    string GenerateToken(User user);
}