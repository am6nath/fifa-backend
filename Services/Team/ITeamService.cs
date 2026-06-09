using fifa_backend.DTOs.Team;

namespace fifa_backend.Services.Team;

public interface ITeamService
{
    Task<TeamResponse> CreateAsync(CreateTeamRequest request);
    Task<TeamResponse> UpdateAsync(int id, UpdateTeamRequest request);
    Task SoftDeleteAsync(int id);
    Task<TeamResponse> GetByIdAsync(int id);
    Task<List<TeamResponse>> GetAllAsync(bool includeInactive = false);
}
