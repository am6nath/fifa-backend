using fifa_backend.Data;
using fifa_backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace fifa_backend.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    private IRepository<Team>? _teams;
    private IRepository<Vote>? _votes;
    private IRepository<VotingSession>? _votingSessions;
    private IRepository<User>? _users;
    private IRepository<AuditLog>? _auditLogs;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<Team> Teams => _teams ??= new Repository<Team>(_context);
    public IRepository<Vote> Votes => _votes ??= new Repository<Vote>(_context);
    public IRepository<VotingSession> VotingSessions => _votingSessions ??= new Repository<VotingSession>(_context);
    public IRepository<User> Users => _users ??= new Repository<User>(_context);
    public IRepository<AuditLog> AuditLogs => _auditLogs ??= new Repository<AuditLog>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task LockUserAsync(int userId)
    {
        await _context.Database.ExecuteSqlRawAsync("SELECT Id FROM Users WHERE Id = {0} FOR UPDATE", userId);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
