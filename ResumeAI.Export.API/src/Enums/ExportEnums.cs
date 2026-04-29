namespace ResumeAI.Export.Enums;

public enum ExportFormat
{
    PDF,
    DOCX,
    JSON
}

public enum ExportStatus
{
    QUEUED,
    PROCESSING,
    COMPLETED,
    FAILED
}
