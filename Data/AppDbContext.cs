using fifa_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace fifa_backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Team> Teams => Set<Team>();

    public DbSet<Vote> Votes => Set<Vote>();

    public DbSet<VotingSession> VotingSessions => Set<VotingSession>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        builder.Entity<User>()
            .HasIndex(x => x.UserName)
            .IsUnique();

        builder.Entity<Vote>()
            .HasIndex(x => x.UserId)
            .IsUnique();

        builder.Entity<Vote>()
            .HasOne(x => x.User)
            .WithMany(x => x.Votes)
            .HasForeignKey(x => x.UserId);

        builder.Entity<Vote>()
            .HasOne(x => x.Team)
            .WithMany(x => x.Votes)
            .HasForeignKey(x => x.TeamId);
    }
}