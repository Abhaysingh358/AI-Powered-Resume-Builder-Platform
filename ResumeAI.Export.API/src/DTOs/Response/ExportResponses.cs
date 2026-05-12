namespace ResumeAI.Export.DTOs.Response;

public record ExportJobResponse
{
    public string JobId { get; init; } = string.Empty;

    public int ResumeId { get; init; }

    public int UserId { get; init; }

    public string Format { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string? FileUrl { get; init; }

    public long FileSizeKb { get; init; }

    public DateTime RequestedAt { get; init; }

    public DateTime? CompletedAt { get; init; }

    public DateTime ExpiresAt { get; init; }

    public int? TemplateId { get; init; }

    public string? ErrorMessage { get; init; }
}

public record ExportStatsResponse
{
    public int TotalExports { get; init; }

    public int PdfExports { get; init; }

    public int DocxExports { get; init; }

    public int JsonExports { get; init; }

    public int ExportsToday { get; init; }

    public int RemainingToday { get; init; }
}

public record ApiResponse<T>
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }

    public IEnumerable<string>? Errors { get; init; }

    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new()
        {
            Success = true,
            Message = message,
            Data = data
        };

    public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors
        };
}