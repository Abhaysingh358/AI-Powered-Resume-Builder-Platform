using Microsoft.EntityFrameworkCore;
using ResumeAI.Section.Entities;

namespace ResumeAI.Section.Data;

public class SectionDbContext : DbContext
{
    public SectionDbContext(DbContextOptions<SectionDbContext> options) : base(options) { }

    public DbSet<ResumeSection> ResumeSections => Set<ResumeSection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ResumeSection>(entity =>
        {
            // Fast lookup of all sections for a resume
            entity.HasIndex(s => s.ResumeId);

            // Fast lookup by owner
            entity.HasIndex(s => s.UserId);

            // Ordered sections per resume
            entity.HasIndex(s => new { s.ResumeId, s.DisplayOrder });

            // Section type filtering per resume
            entity.HasIndex(s => new { s.ResumeId, s.SectionType });

            // AI-generated filter
            entity.HasIndex(s => s.AiGenerated);

            // Store SectionType enum as string
            entity.Property(s => s.SectionType)
                  .HasConversion<string>()
                  .HasMaxLength(30);

            entity.Property(s => s.CreatedAt)
                  .HasDefaultValueSql("NOW()");

            entity.Property(s => s.UpdatedAt)
                  .HasDefaultValueSql("NOW()");
        });
    }
}
