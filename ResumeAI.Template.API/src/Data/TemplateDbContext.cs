using Microsoft.EntityFrameworkCore;
using ResumeAI.Template.Entities;
using ResumeAI.Template.Enums;

namespace ResumeAI.Template.Data;

public class TemplateDbContext : DbContext
{
    public TemplateDbContext(DbContextOptions<TemplateDbContext> options) : base(options) { }

    public DbSet<ResumeTemplate> ResumeTemplates => Set<ResumeTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ResumeTemplate>(entity =>
        {
            entity.HasIndex(t => t.Category);
            entity.HasIndex(t => t.IsPremium);
            entity.HasIndex(t => t.IsActive);
            entity.HasIndex(t => t.UsageCount);

            entity.Property(t => t.Category)
                  .HasConversion<string>()
                  .HasMaxLength(30);

            entity.Property(t => t.CreatedAt)
                  .HasDefaultValueSql("NOW()");
        });

        // Seed 5 default templates — 3 free, 2 premium
        modelBuilder.Entity<ResumeTemplate>().HasData(

            new ResumeTemplate
            {
                TemplateId   = 1,
                Name         = "Classic Professional",
                Description  = "A clean, traditional layout ideal for corporate and finance roles. ATS-friendly structure.",
                ThumbnailUrl = "/thumbnails/classic-professional.png",
                Category     = TemplateCategory.PROFESSIONAL,
                IsPremium    = false,
                IsActive     = true,
                UsageCount   = 0,
                CreatedAt    = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                HtmlLayout   = """
                    <div class="resume">
                      <header class="resume-header">
                        <h1>{{FullName}}</h1>
                        <p>{{Email}} | {{Phone}} | {{Location}}</p>
                      </header>
                      <section class="summary"><h2>Professional Summary</h2><p>{{Summary}}</p></section>
                      <section class="experience"><h2>Work Experience</h2>{{Experience}}</section>
                      <section class="education"><h2>Education</h2>{{Education}}</section>
                      <section class="skills"><h2>Skills</h2>{{Skills}}</section>
                    </div>
                    """,
                CssStyles    = """
                    .resume { font-family: 'Times New Roman', serif; max-width: 800px; margin: 0 auto; padding: 40px; color: #222; }
                    .resume-header { text-align: center; border-bottom: 2px solid #222; padding-bottom: 16px; margin-bottom: 24px; }
                    .resume-header h1 { font-size: 28px; margin: 0; }
                    section { margin-bottom: 24px; }
                    h2 { font-size: 16px; text-transform: uppercase; border-bottom: 1px solid #999; padding-bottom: 4px; }
                    """
            },

            new ResumeTemplate
            {
                TemplateId   = 2,
                Name         = "Modern Minimal",
                Description  = "A clean two-column layout with a side accent bar. Great for tech and startup roles.",
                ThumbnailUrl = "/thumbnails/modern-minimal.png",
                Category     = TemplateCategory.MODERN,
                IsPremium    = false,
                IsActive     = true,
                UsageCount   = 0,
                CreatedAt    = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                HtmlLayout   = """
                    <div class="resume two-col">
                      <aside class="sidebar">
                        <h1>{{FullName}}</h1>
                        <p>{{Email}}</p><p>{{Phone}}</p>
                        <h3>Skills</h3>{{Skills}}
                      </aside>
                      <main class="content">
                        <section><h2>Summary</h2><p>{{Summary}}</p></section>
                        <section><h2>Experience</h2>{{Experience}}</section>
                        <section><h2>Education</h2>{{Education}}</section>
                      </main>
                    </div>
                    """,
                CssStyles    = """
                    .resume { font-family: 'Segoe UI', sans-serif; display: flex; max-width: 820px; margin: 0 auto; }
                    .sidebar { width: 280px; background: #2d3748; color: #fff; padding: 32px 24px; }
                    .sidebar h1 { font-size: 22px; margin-bottom: 8px; }
                    .content { flex: 1; padding: 32px; }
                    h2 { font-size: 15px; color: #2d3748; text-transform: uppercase; border-bottom: 2px solid #2d3748; }
                    """
            },

            new ResumeTemplate
            {
                TemplateId   = 3,
                Name         = "ATS Optimised",
                Description  = "Single-column plain text layout designed to score 90+ on ATS systems. No tables or graphics.",
                ThumbnailUrl = "/thumbnails/ats-optimised.png",
                Category     = TemplateCategory.ATS_OPTIMISED,
                IsPremium    = false,
                IsActive     = true,
                UsageCount   = 0,
                CreatedAt    = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                HtmlLayout   = """
                    <div class="resume ats">
                      <h1>{{FullName}}</h1>
                      <p>{{Email}} | {{Phone}}</p>
                      <h2>Summary</h2><p>{{Summary}}</p>
                      <h2>Experience</h2>{{Experience}}
                      <h2>Education</h2>{{Education}}
                      <h2>Skills</h2>{{Skills}}
                      <h2>Certifications</h2>{{Certifications}}
                    </div>
                    """,
                CssStyles    = """
                    .resume.ats { font-family: Arial, sans-serif; max-width: 750px; margin: 0 auto; padding: 32px; color: #000; }
                    h1 { font-size: 24px; margin-bottom: 4px; }
                    h2 { font-size: 14px; text-transform: uppercase; margin-top: 20px; margin-bottom: 6px; }
                    p, li { font-size: 13px; line-height: 1.6; }
                    """
            },

            new ResumeTemplate
            {
                TemplateId   = 4,
                Name         = "Creative Portfolio",
                Description  = "Bold typography with colour accents. Perfect for designers, marketers, and creatives. Premium only.",
                ThumbnailUrl = "/thumbnails/creative-portfolio.png",
                Category     = TemplateCategory.CREATIVE,
                IsPremium    = true,
                IsActive     = true,
                UsageCount   = 0,
                CreatedAt    = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                HtmlLayout   = """
                    <div class="resume creative">
                      <header><h1>{{FullName}}</h1><span class="tagline">{{TargetJobTitle}}</span></header>
                      <div class="grid">
                        <div class="left">
                          <section><h2>About</h2><p>{{Summary}}</p></section>
                          <section><h2>Skills</h2>{{Skills}}</section>
                          <section><h2>Languages</h2>{{Languages}}</section>
                        </div>
                        <div class="right">
                          <section><h2>Experience</h2>{{Experience}}</section>
                          <section><h2>Projects</h2>{{Projects}}</section>
                        </div>
                      </div>
                    </div>
                    """,
                CssStyles    = """
                    .resume.creative { font-family: 'Montserrat', sans-serif; max-width: 860px; margin: 0 auto; }
                    header { background: #e63946; color: #fff; padding: 40px; }
                    header h1 { font-size: 36px; margin: 0; }
                    .tagline { font-size: 16px; opacity: 0.85; }
                    .grid { display: grid; grid-template-columns: 1fr 2fr; gap: 0; }
                    .left { background: #f8f9fa; padding: 32px; }
                    .right { padding: 32px; }
                    h2 { color: #e63946; font-size: 13px; text-transform: uppercase; letter-spacing: 1px; }
                    """
            },

            new ResumeTemplate
            {
                TemplateId   = 5,
                Name         = "Executive Minimalist",
                Description  = "Ultra-clean layout with generous white space. Ideal for senior professionals and executives. Premium only.",
                ThumbnailUrl = "/thumbnails/executive-minimalist.png",
                Category     = TemplateCategory.MINIMALIST,
                IsPremium    = true,
                IsActive     = true,
                UsageCount   = 0,
                CreatedAt    = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                HtmlLayout   = """
                    <div class="resume executive">
                      <header>
                        <h1>{{FullName}}</h1>
                        <p class="contact">{{Email}} &bull; {{Phone}} &bull; {{Location}}</p>
                      </header>
                      <section><h2>Summary</h2><p>{{Summary}}</p></section>
                      <section><h2>Experience</h2>{{Experience}}</section>
                      <section><h2>Education</h2>{{Education}}</section>
                      <section><h2>Core Competencies</h2>{{Skills}}</section>
                    </div>
                    """,
                CssStyles    = """
                    .resume.executive { font-family: 'Garamond', Georgia, serif; max-width: 760px; margin: 60px auto; padding: 0 40px; color: #1a1a1a; }
                    header { border-bottom: 3px solid #1a1a1a; padding-bottom: 20px; margin-bottom: 32px; }
                    h1 { font-size: 32px; font-weight: normal; letter-spacing: 2px; text-transform: uppercase; }
                    .contact { font-size: 12px; color: #666; letter-spacing: 1px; }
                    h2 { font-size: 11px; text-transform: uppercase; letter-spacing: 3px; color: #666; margin-top: 28px; }
                    """
            }
        );
    }
}
