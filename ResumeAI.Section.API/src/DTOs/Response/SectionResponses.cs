namespace ResumeAI.Section.DTOs.Response;

//   Section Response 
public record SectionResponse
{
    public int     SectionId    { get; init; }
    public int     ResumeId     { get; init; }
    public int     UserId       { get; init; }
    public string  SectionType  { get; init; } = string.Empty;
    public string  Title        { get; init; } = string.Empty;
    public string? Content      { get; init; }
    public int     DisplayOrder { get; init; }
    public bool    IsVisible    { get; init; }
    public bool    AiGenerated  { get; init; }
    public DateTime CreatedAt   { get; init; }
    public DateTime UpdatedAt   { get; init; }
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
