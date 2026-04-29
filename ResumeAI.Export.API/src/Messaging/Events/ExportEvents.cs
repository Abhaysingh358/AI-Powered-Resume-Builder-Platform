namespace ResumeAI.Export.Messaging.Events;

// Message published to RabbitMQ when an export is requested
public record ExportRequestedEvent
{
    public string JobId { get; init; } = string.Empty;

    public int UserId { get; init; }

    public int ResumeId { get; init; }

    public string Format { get; init; } = string.Empty;

    public string ResumeJson { get; init; } = string.Empty;

    public int? TemplateId { get; init; }

    public string? Customizations { get; init; }
}

// Message published when export is completed
public record ExportCompletedEvent
{
    public string JobId { get; init; } = string.Empty;

    public int UserId { get; init; }

    public string FileUrl { get; init; } = string.Empty;

    public long FileSizeKb { get; init; }

    public bool Success { get; init; }

    public string? Error { get; init; }
}