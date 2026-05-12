using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeAI.AI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_requests",
                columns: table => new
                {
                    request_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    resume_id = table.Column<int>(type: "integer", nullable: false),
                    request_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    input_prompt = table.Column<string>(type: "text", nullable: true),
                    ai_response = table.Column<string>(type: "text", nullable: true),
                    model = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tokens_used = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_requests", x => x.request_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_requests_request_type",
                table: "ai_requests",
                column: "request_type");

            migrationBuilder.CreateIndex(
                name: "IX_ai_requests_resume_id",
                table: "ai_requests",
                column: "resume_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_requests_status",
                table: "ai_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_ai_requests_user_id",
                table: "ai_requests",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_requests_user_id_created_at",
                table: "ai_requests",
                columns: new[] { "user_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_requests");
        }
    }
}
