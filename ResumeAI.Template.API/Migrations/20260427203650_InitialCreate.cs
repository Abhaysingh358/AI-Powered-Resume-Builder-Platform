using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ResumeAI.Template.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "resume_templates",
                columns: table => new
                {
                    template_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    thumbnail_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    html_layout = table.Column<string>(type: "text", nullable: true),
                    css_styles = table.Column<string>(type: "text", nullable: true),
                    category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    is_premium = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    usage_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resume_templates", x => x.template_id);
                });

            migrationBuilder.InsertData(
                table: "resume_templates",
                columns: new[] { "template_id", "category", "created_at", "css_styles", "description", "html_layout", "is_active", "is_premium", "name", "thumbnail_url", "usage_count" },
                values: new object[,]
                {
                    { 1, "PROFESSIONAL", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), ".resume { font-family: 'Times New Roman', serif; max-width: 800px; margin: 0 auto; padding: 40px; color: #222; }\n.resume-header { text-align: center; border-bottom: 2px solid #222; padding-bottom: 16px; margin-bottom: 24px; }\n.resume-header h1 { font-size: 28px; margin: 0; }\nsection { margin-bottom: 24px; }\nh2 { font-size: 16px; text-transform: uppercase; border-bottom: 1px solid #999; padding-bottom: 4px; }", "A clean, traditional layout ideal for corporate and finance roles. ATS-friendly structure.", "<div class=\"resume\">\n  <header class=\"resume-header\">\n    <h1>{{FullName}}</h1>\n    <p>{{Email}} | {{Phone}} | {{Location}}</p>\n  </header>\n  <section class=\"summary\"><h2>Professional Summary</h2><p>{{Summary}}</p></section>\n  <section class=\"experience\"><h2>Work Experience</h2>{{Experience}}</section>\n  <section class=\"education\"><h2>Education</h2>{{Education}}</section>\n  <section class=\"skills\"><h2>Skills</h2>{{Skills}}</section>\n</div>", true, false, "Classic Professional", "/thumbnails/classic-professional.png", 0 },
                    { 2, "MODERN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), ".resume { font-family: 'Segoe UI', sans-serif; display: flex; max-width: 820px; margin: 0 auto; }\n.sidebar { width: 280px; background: #2d3748; color: #fff; padding: 32px 24px; }\n.sidebar h1 { font-size: 22px; margin-bottom: 8px; }\n.content { flex: 1; padding: 32px; }\nh2 { font-size: 15px; color: #2d3748; text-transform: uppercase; border-bottom: 2px solid #2d3748; }", "A clean two-column layout with a side accent bar. Great for tech and startup roles.", "<div class=\"resume two-col\">\n  <aside class=\"sidebar\">\n    <h1>{{FullName}}</h1>\n    <p>{{Email}}</p><p>{{Phone}}</p>\n    <h3>Skills</h3>{{Skills}}\n  </aside>\n  <main class=\"content\">\n    <section><h2>Summary</h2><p>{{Summary}}</p></section>\n    <section><h2>Experience</h2>{{Experience}}</section>\n    <section><h2>Education</h2>{{Education}}</section>\n  </main>\n</div>", true, false, "Modern Minimal", "/thumbnails/modern-minimal.png", 0 },
                    { 3, "ATS_OPTIMISED", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), ".resume.ats { font-family: Arial, sans-serif; max-width: 750px; margin: 0 auto; padding: 32px; color: #000; }\nh1 { font-size: 24px; margin-bottom: 4px; }\nh2 { font-size: 14px; text-transform: uppercase; margin-top: 20px; margin-bottom: 6px; }\np, li { font-size: 13px; line-height: 1.6; }", "Single-column plain text layout designed to score 90+ on ATS systems. No tables or graphics.", "<div class=\"resume ats\">\n  <h1>{{FullName}}</h1>\n  <p>{{Email}} | {{Phone}}</p>\n  <h2>Summary</h2><p>{{Summary}}</p>\n  <h2>Experience</h2>{{Experience}}\n  <h2>Education</h2>{{Education}}\n  <h2>Skills</h2>{{Skills}}\n  <h2>Certifications</h2>{{Certifications}}\n</div>", true, false, "ATS Optimised", "/thumbnails/ats-optimised.png", 0 },
                    { 4, "CREATIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), ".resume.creative { font-family: 'Montserrat', sans-serif; max-width: 860px; margin: 0 auto; }\nheader { background: #e63946; color: #fff; padding: 40px; }\nheader h1 { font-size: 36px; margin: 0; }\n.tagline { font-size: 16px; opacity: 0.85; }\n.grid { display: grid; grid-template-columns: 1fr 2fr; gap: 0; }\n.left { background: #f8f9fa; padding: 32px; }\n.right { padding: 32px; }\nh2 { color: #e63946; font-size: 13px; text-transform: uppercase; letter-spacing: 1px; }", "Bold typography with colour accents. Perfect for designers, marketers, and creatives. Premium only.", "<div class=\"resume creative\">\n  <header><h1>{{FullName}}</h1><span class=\"tagline\">{{TargetJobTitle}}</span></header>\n  <div class=\"grid\">\n    <div class=\"left\">\n      <section><h2>About</h2><p>{{Summary}}</p></section>\n      <section><h2>Skills</h2>{{Skills}}</section>\n      <section><h2>Languages</h2>{{Languages}}</section>\n    </div>\n    <div class=\"right\">\n      <section><h2>Experience</h2>{{Experience}}</section>\n      <section><h2>Projects</h2>{{Projects}}</section>\n    </div>\n  </div>\n</div>", true, true, "Creative Portfolio", "/thumbnails/creative-portfolio.png", 0 },
                    { 5, "MINIMALIST", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), ".resume.executive { font-family: 'Garamond', Georgia, serif; max-width: 760px; margin: 60px auto; padding: 0 40px; color: #1a1a1a; }\nheader { border-bottom: 3px solid #1a1a1a; padding-bottom: 20px; margin-bottom: 32px; }\nh1 { font-size: 32px; font-weight: normal; letter-spacing: 2px; text-transform: uppercase; }\n.contact { font-size: 12px; color: #666; letter-spacing: 1px; }\nh2 { font-size: 11px; text-transform: uppercase; letter-spacing: 3px; color: #666; margin-top: 28px; }", "Ultra-clean layout with generous white space. Ideal for senior professionals and executives. Premium only.", "<div class=\"resume executive\">\n  <header>\n    <h1>{{FullName}}</h1>\n    <p class=\"contact\">{{Email}} &bull; {{Phone}} &bull; {{Location}}</p>\n  </header>\n  <section><h2>Summary</h2><p>{{Summary}}</p></section>\n  <section><h2>Experience</h2>{{Experience}}</section>\n  <section><h2>Education</h2>{{Education}}</section>\n  <section><h2>Core Competencies</h2>{{Skills}}</section>\n</div>", true, true, "Executive Minimalist", "/thumbnails/executive-minimalist.png", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_resume_templates_category",
                table: "resume_templates",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "IX_resume_templates_is_active",
                table: "resume_templates",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_resume_templates_is_premium",
                table: "resume_templates",
                column: "is_premium");

            migrationBuilder.CreateIndex(
                name: "IX_resume_templates_usage_count",
                table: "resume_templates",
                column: "usage_count");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "resume_templates");
        }
    }
}
