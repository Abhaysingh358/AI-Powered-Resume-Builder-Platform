namespace ResumeAI.Section.Enums;

/// <summary>
/// String-backed enum for section types.
/// Stored as string in the database via EF Core HasConversion.
/// </summary>
public enum SectionType
{
    SUMMARY,
    EXPERIENCE,
    EDUCATION,
    SKILLS,
    CERTIFICATIONS,
    PROJECTS,
    LANGUAGES,
    VOLUNTEER,
    CUSTOM
}
