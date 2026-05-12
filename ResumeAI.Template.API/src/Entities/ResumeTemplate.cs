using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ResumeAI.Template.Enums;

namespace ResumeAI.Template.Entities;

[Table("resume_templates")]
public class ResumeTemplate
{
    [Key]
    [Column("template_id")]
    public int TemplateId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    [MaxLength(500)]
    [Column("thumbnail_url")]
    public string? ThumbnailUrl { get; set; }

    // Full HTML layout string — stored as TEXT column
    [Column("html_layout", TypeName = "text")]
    public string? HtmlLayout { get; set; }

    // Full CSS styles string — stored as TEXT column
    [Column("css_styles", TypeName = "text")]
    public string? CssStyles { get; set; }

    [Required]
    [Column("category")]
    public TemplateCategory Category { get; set; } = TemplateCategory.PROFESSIONAL;

    // True = requires Premium plan to use
    [Column("is_premium")]
    public bool IsPremium { get; set; } = false;

    // False = soft-deleted by admin
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    // Incremented atomically via ExecuteUpdateAsync when user selects this template
    [Column("usage_count")]
    public int UsageCount { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
