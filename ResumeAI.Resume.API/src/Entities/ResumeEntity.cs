using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ResumeAI.Resume.Enums;

namespace ResumeAI.Resume.Entities;

[Table("resumes")]
public class ResumeEntity
{
    [Key]
    [Column("resume_id")]
    public int ResumeId { get; set; }

    /// <summary>Owner — references users table in Auth Service (same DB).</summary>
    [Required]
    [Column("user_id")]
    public int UserId { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(200)]
    [Column("target_job_title")]
    public string? TargetJobTitle { get; set; }

    /// <summary>References resume_templates table (Template Service — same DB).</summary>
    [Column("template_id")]
    public int? TemplateId { get; set; }

    /// <summary>0-100 ATS compatibility score updated asynchronously by AI Service.</summary>
    [Column("ats_score")]
    public int AtsScore { get; set; } = 0;

    [Required]
    [Column("status")]
    public ResumeStatus Status { get; set; } = ResumeStatus.DRAFT;

    [MaxLength(10)]
    [Column("language")]
    public string Language { get; set; } = "en";

    /// <summary>True when user shares resume to public gallery.</summary>
    [Column("is_public")]
    public bool IsPublic { get; set; } = false;

    /// <summary>Incremented each time public resume is viewed.</summary>
    [Column("view_count")]
    public int ViewCount { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
