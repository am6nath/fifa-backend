using fifa_backend.DTOs.Vote;

namespace fifa_backend.Services.Vote;

public interface IVoteService
{
    Task<VoteResponse> CastVoteAsync(int userId, CastVoteRequest request, string? ipAddress, string? userAgent);
    Task<VoteResponse?> GetUserVoteAsync(int userId, int sessionId);
    Task<bool> HasUserVotedAsync(int userId, int sessionId);
    Task<List<VoteResponse>> GetUserVoteHistoryAsync(int userId);
}
