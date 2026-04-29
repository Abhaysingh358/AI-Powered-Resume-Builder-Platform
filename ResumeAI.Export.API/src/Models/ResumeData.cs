namespace ResumeAI.Export.Models;

// Resume data passed by the caller when requesting an export.
// Export Service does not fetch resume data from Resume Service.
// The caller (frontend or Resume Service) serialises the resume and sends it here.
public class ResumeData
{
    public int    ResumeId      { get; set; }
    public string FullName      { get; set; } = string.Empty;
    public string? Email        { get; set; }
    public string? Phone        { get; set; }
    public string? Location     { get; set; }
    public string? TargetJobTitle { get; set; }
    public string? Summary      { get; set; }
    public IList<SectionData> Sections { get; set; } = [];
}

public class SectionData
{
    public string  SectionType  { get; set; } = string.Empty;
    public string  Title        { get; set; } = string.Empty;
    public string? Content      { get; set; }
    public int     DisplayOrder { get; set; }
    public bool    IsVisible    { get; set; } = true;
}
