using Microsoft.EntityFrameworkCore;
using ResumeAI.AI.Entities;

namespace ResumeAI.AI.Data;

public class AiDbContext : DbContext
{
    public AiDbContext(DbContextOptions<AiDbContext> options) : base(options) { }

    public DbSet<AiRequest> AiRequests => Set<AiRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AiRequest>(entity =>
        {
            // Fast lookup by user
            entity.HasIndex(a => a.UserId);

            // Fast lookup by resume
            entity.HasIndex(a => a.ResumeId);

            // Filter by status
            entity.HasIndex(a => a.Status);

            // Filter by request type
            entity.HasIndex(a => a.RequestType);

            // Composite index for quota count queries (userId + createdAt)
            entity.HasIndex(a => new { a.UserId, a.CreatedAt });

            entity.Property(a => a.RequestType)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.Property(a => a.Model)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.Property(a => a.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.Property(a => a.CreatedAt)
                  .HasDefaultValueSql("NOW()");
        });
    }
}
