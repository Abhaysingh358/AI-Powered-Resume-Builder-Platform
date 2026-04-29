using System.ComponentModel.DataAnnotations;
using ResumeAI.Export.Models;

namespace ResumeAI.Export.DTOs.Request;

public record ExportPdfRequest
{
    [Required]
    public int ResumeId { get; init; }

    [Required]
    public ResumeData ResumeData { get; init; } = null!;

    public int? TemplateId { get; init; }

    public string? Customizations { get; init; }
}

public record ExportDocxRequest
{
    [Required]
    public int ResumeId { get; init; }

    [Required]
    public ResumeData ResumeData { get; init; } = null!;

    public int? TemplateId { get; init; }
}

public record ExportJsonRequest
{
    [Required]
    public int ResumeId { get; init; }

    [Required]
    public ResumeData ResumeData { get; init; } = null!;
}