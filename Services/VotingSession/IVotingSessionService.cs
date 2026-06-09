using fifa_backend.DTOs.VotingSession;

namespace fifa_backend.Services.VotingSession;

public interface IVotingSessionService
{
    Task<VotingSessionResponse> CreateAsync(CreateVotingSessionRequest request);
    Task<VotingSessionResponse> UpdateAsync(int id, UpdateVotingSessionRequest request);
    Task CloseSessionAsync(int id);
    Task PublishResultsAsync(int id, string publishedBy);
    Task<VotingSessionResponse> GetByIdAsync(int id);
    Task<List<VotingSessionResponse>> GetAllAsync();
    Task<List<VotingSessionResponse>> GetActiveSessionsAsync();
}
