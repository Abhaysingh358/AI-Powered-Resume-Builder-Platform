using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ResumeAI.Section.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "resume_sections",
                columns: table => new
                {
                    section_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    resume_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    section_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false),
                    ai_generated = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resume_sections", x => x.section_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_resume_sections_ai_generated",
                table: "resume_sections",
                column: "ai_generated");

            migrationBuilder.CreateIndex(
                name: "IX_resume_sections_resume_id",
                table: "resume_sections",
                column: "resume_id");

            migrationBuilder.CreateIndex(
                name: "IX_resume_sections_resume_id_display_order",
                table: "resume_sections",
                columns: new[] { "resume_id", "display_order" });

            migrationBuilder.CreateIndex(
                name: "IX_resume_sections_resume_id_section_type",
                table: "resume_sections",
                columns: new[] { "resume_id", "section_type" });

            migrationBuilder.CreateIndex(
                name: "IX_resume_sections_user_id",
                table: "resume_sections",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "resume_sections");
        }
    }
}
