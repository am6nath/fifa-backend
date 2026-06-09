using fifa_backend.DTOs.Common;
using fifa_backend.DTOs.User;

namespace fifa_backend.Services.Users;

public interface IUserService
{
    Task<PagedResponse<UserResponse>> GetUsersAsync(UserFilter filter);
    Task<UserResponse> GetByIdAsync(int id);
    Task<UserResponse> GetProfileAsync(int userId);
    Task<UserResponse> ToggleActiveStatusAsync(int id);
}
