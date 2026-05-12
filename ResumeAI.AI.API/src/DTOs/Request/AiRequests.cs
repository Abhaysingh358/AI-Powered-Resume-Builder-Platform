using System.ComponentModel.DataAnnotations;

namespace ResumeAI.AI.DTOs.Request;

public record GenerateSummaryRequest
{
    [Required] public int    ResumeId        { get; init; }
    [Required] public string JobTitle        { get; init; } = string.Empty;
    [Required] public string KeySkills       { get; init; } = string.Empty;
               public int    YearsExperience { get; init; } = 0;
}

public record GenerateBulletsRequest
{
    [Required] public int    ResumeId        { get; init; }
    [Required] public string JobTitle        { get; init; } = string.Empty;
    [Required] public string Responsibilities { get; init; } = string.Empty;
               public string? CompanyName    { get; init; }
}

public record GenerateCoverLetterRequest
{
    [Required] public int    ResumeId        { get; init; }
    [Required] public string JobDescription  { get; init; } = string.Empty;
    [Required] public string ApplicantName   { get; init; } = string.Empty;
               public string? CompanyName    { get; init; }
}

public record ImproveSectionRequest
{
    [Required] public int    ResumeId     { get; init; }
    [Required] public string SectionType  { get; init; } = string.Empty;
    [Required] public string CurrentContent { get; init; } = string.Empty;
}

public record CheckAtsRequest
{
    [Required] public int    ResumeId       { get; init; }
    [Required] public string ResumeText     { get; init; } = string.Empty;
    [Required] public string JobDescription { get; init; } = string.Empty;
}

public record SuggestSkillsRequest
{
    [Required] public int    ResumeId  { get; init; }
    [Required] public string JobTitle  { get; init; } = string.Empty;
}

public record TailorResumeRequest
{
    [Required] public int    ResumeId       { get; init; }
    [Required] public string ResumeJson     { get; init; } = string.Empty;
    [Required] public string JobDescription { get; init; } = string.Empty;
}

public record TranslateResumeRequest
{
    [Required] public int    ResumeId        { get; init; }
    [Required] public string ResumeContent   { get; init; } = string.Empty;
    [Required] public string TargetLanguage  { get; init; } = string.Empty;
}
