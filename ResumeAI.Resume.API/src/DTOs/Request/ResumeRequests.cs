using System.ComponentModel.DataAnnotations;

namespace ResumeAI.Resume.DTOs.Request;

// ── Create Resume 
public record CreateResumeRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(200)]
    public string? TargetJobTitle { get; init; }

    public int? TemplateId { get; init; }

    [MaxLength(10)]
    public string Language { get; init; } = "en";
}

// ── Update Resume 
public record UpdateResumeRequest
{
    [MaxLength(200)]
    public string? Title { get; init; }

    [MaxLength(200)]
    public string? TargetJobTitle { get; init; }

    public int? TemplateId { get; init; }

    [MaxLength(10)]
    public string? Language { get; init; }

    public string? Status { get; init; }  // DRAFT | COMPLETE
}

// ── Update ATS Score (called internally by AI Service)
public record UpdateAtsScoreRequest
{
    [Required]
    [Range(0, 100)]
    public int AtsScore { get; init; }
}
