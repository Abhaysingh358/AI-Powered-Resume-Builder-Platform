using Microsoft.EntityFrameworkCore;
using ResumeAI.Resume.Entities;
using ResumeAI.Resume.Enums;

namespace ResumeAI.Resume.Data;

public class ResumeDbContext : DbContext
{
    public ResumeDbContext(DbContextOptions<ResumeDbContext> options) : base(options) { }

    public DbSet<ResumeEntity> Resumes => Set<ResumeEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ResumeEntity>(entity =>
        {
            // Index for fast user resume lookups
            entity.HasIndex(r => r.UserId);

            // Index for public gallery queries
            entity.HasIndex(r => r.IsPublic);

            // Index for template-based queries
            entity.HasIndex(r => r.TemplateId);

            // Index for status filtering
            entity.HasIndex(r => r.Status);

            // Composite index for user + status filtering
            entity.HasIndex(r => new { r.UserId, r.Status });

            // Store enum as string
            entity.Property(r => r.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.Property(r => r.CreatedAt)
                  .HasDefaultValueSql("NOW()");

            entity.Property(r => r.UpdatedAt)
                  .HasDefaultValueSql("NOW()");
        });
    }
}
