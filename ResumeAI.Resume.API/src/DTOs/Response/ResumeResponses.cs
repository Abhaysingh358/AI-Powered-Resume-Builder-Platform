namespace ResumeAI.Resume.DTOs.Response;

//   Resume Response 
public record ResumeResponse
{
    public int     ResumeId       { get; init; }
    public int     UserId         { get; init; }
    public string  Title          { get; init; } = string.Empty;
    public string? TargetJobTitle { get; init; }
    public int?    TemplateId     { get; init; }
    public int     AtsScore       { get; init; }
    public string  Status         { get; init; } = string.Empty;
    public string  Language       { get; init; } = string.Empty;
    public bool    IsPublic       { get; init; }
    public int     ViewCount      { get; init; }
    public DateTime CreatedAt     { get; init; }
    public DateTime UpdatedAt     { get; init; }
}

//   Generic API Envelope 
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

//   Paged Response 
public record PagedResponse<T>
{
    public IEnumerable<T> Items      { get; init; } = [];
    public int            Page       { get; init; }
    public int            PageSize   { get; init; }
    public int            TotalCount { get; init; }
    public int            TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
