using fifa_backend.Models;

namespace fifa_backend.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<Team> Teams { get; }
    IRepository<Vote> Votes { get; }
    IRepository<VotingSession> VotingSessions { get; }
    IRepository<User> Users { get; }
    IRepository<AuditLog> AuditLogs { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task LockUserAsync(int userId);
}
