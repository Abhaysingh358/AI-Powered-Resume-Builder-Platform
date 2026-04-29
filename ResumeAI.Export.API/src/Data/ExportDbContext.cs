using Microsoft.EntityFrameworkCore;
using ResumeAI.Export.Entities;

namespace ResumeAI.Export.Data;

public class ExportDbContext : DbContext
{
    public ExportDbContext(DbContextOptions<ExportDbContext> options) : base(options) { }

    public DbSet<ExportJob> ExportJobs => Set<ExportJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ExportJob>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ResumeId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => new { e.UserId, e.RequestedAt });

            entity.Property(e => e.Format)
                  .HasConversion<string>()
                  .HasMaxLength(10);

            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.Property(e => e.RequestedAt)
                  .HasDefaultValueSql("NOW()");
        });
    }
}
