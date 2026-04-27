namespace ResumeAI.Template.DTOs.Response;

public record TemplateResponse
{
    public int     TemplateId   { get; init; }
    public string  Name         { get; init; } = string.Empty;
    public string? Description  { get; init; }
    public string? ThumbnailUrl { get; init; }
    public string? HtmlLayout   { get; init; }
    public string? CssStyles    { get; init; }
    public string  Category     { get; init; } = string.Empty;
    public bool    IsPremium    { get; init; }
    public bool    IsActive     { get; init; }
    public int     UsageCount   { get; init; }
    public DateTime CreatedAt   { get; init; }
}

// Lightweight response for listing — excludes HtmlLayout and CssStyles for performance
public record TemplateListResponse
{
    public int     TemplateId   { get; init; }
    public string  Name         { get; init; } = string.Empty;
    public string? Description  { get; init; }
    public string? ThumbnailUrl { get; init; }
    public string  Category     { get; init; } = string.Empty;
    public bool    IsPremium    { get; init; }
    public bool    IsActive     { get; init; }
    public int     UsageCount   { get; init; }
}

public record ApiResponse<T>
{
    public bool    Success { get; init; }
    public string  Message { get; init; } = string.Empty;
    public T?      Data    { get; init; }
    public IEnumerable<string>? Errors { get; init; }

    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}
