using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ResumeAI.Export.Enums;

namespace ResumeAI.Export.Entities;

[Table("export_jobs")]
public class ExportJob
{
    [Key]
    [Column("job_id")]
    [MaxLength(36)]
    public string JobId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("resume_id")]
    public int ResumeId { get; set; }

    [Required]
    [Column("user_id")]
    public int UserId { get; set; }

    [Required]
    [Column("format")]
    public ExportFormat Format { get; set; }

    [Required]
    [Column("status")]
    public ExportStatus Status { get; set; } = ExportStatus.QUEUED;

    // Relative download URL — e.g. /api/exports/download/{jobId}
    [MaxLength(500)]
    [Column("file_url")]
    public string? FileUrl { get; set; }

    [Column("file_size_kb")]
    public long FileSizeKb { get; set; } = 0;

    [Column("requested_at")]
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    // Export records expire after 7 days
    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

    [Column("template_id")]
    public int? TemplateId { get; set; }

    // JSON string for custom font/colour options (Premium)
    [Column("customizations", TypeName = "text")]
    public string? Customizations { get; set; }

    // Error message if status = FAILED
    [Column("error_message", TypeName = "text")]
    public string? ErrorMessage { get; set; }
}
