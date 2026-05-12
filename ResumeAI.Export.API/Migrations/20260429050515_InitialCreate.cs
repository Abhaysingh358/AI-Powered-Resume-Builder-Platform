using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeAI.Export.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "export_jobs",
                columns: table => new
                {
                    job_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    resume_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    format = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    file_size_kb = table.Column<long>(type: "bigint", nullable: false),
                    requested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    template_id = table.Column<int>(type: "integer", nullable: true),
                    customizations = table.Column<string>(type: "text", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_export_jobs", x => x.job_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_export_jobs_expires_at",
                table: "export_jobs",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_export_jobs_resume_id",
                table: "export_jobs",
                column: "resume_id");

            migrationBuilder.CreateIndex(
                name: "IX_export_jobs_status",
                table: "export_jobs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_export_jobs_user_id",
                table: "export_jobs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_export_jobs_user_id_requested_at",
                table: "export_jobs",
                columns: new[] { "user_id", "requested_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "export_jobs");
        }
    }
}
