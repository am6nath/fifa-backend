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
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        builder.Entity<User>()
            .HasIndex(x => x.UserName)
            .IsUnique();

        // One vote per user PER session
        builder.Entity<Vote>()
            .HasIndex(x => new { x.UserId, x.VotingSessionId })
            .IsUnique();

        builder.Entity<Vote>()
            .HasOne(x => x.User)
            .WithMany(x => x.Votes)
            .HasForeignKey(x => x.UserId);

        builder.Entity<Vote>()
            .HasOne(x => x.Team)
            .WithMany(x => x.Votes)
            .HasForeignKey(x => x.TeamId);

        builder.Entity<Vote>()
            .HasOne(x => x.VotingSession)
            .WithMany(x => x.Votes)
            .HasForeignKey(x => x.VotingSessionId);

        builder.Entity<VotingSession>()
            .HasMany(x => x.Teams)
            .WithMany(x => x.VotingSessions)
            .UsingEntity(j => j.ToTable("VotingSessionTeams"));

        builder.Entity<AuditLog>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Global query filters for soft delete
        builder.Entity<User>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Team>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Vote>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<VotingSession>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<AuditLog>().HasQueryFilter(x => !x.IsDeleted);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (BaseEntity)entityEntry.Entity;
            entity.UpdatedAt = DateTime.UtcNow;

            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
        }
    }
}