namespace ResumeAI.AI.DTOs.Response;

public record AiRequestResponse
{
    public string    RequestId    { get; init; } = string.Empty;
    public int       UserId       { get; init; }
    public int       ResumeId     { get; init; }
    public string    RequestType  { get; init; } = string.Empty;
    public string?   AiResponse   { get; init; }
    public string    Model        { get; init; } = string.Empty;
    public int       TokensUsed   { get; init; }
    public string    Status       { get; init; } = string.Empty;
    public DateTime  CreatedAt    { get; init; }
    public DateTime? CompletedAt  { get; init; }
}

public record AtsReportResponse
{
    public int              Score           { get; init; }
    public IList<string>    MissingKeywords { get; init; } = [];
    public string           Recommendation  { get; init; } = string.Empty;
}

public record QuotaResponse
{
    public int  AiCallsUsed       { get; init; }
    public int  AiCallsLimit      { get; init; }
    public int  AtsChecksUsed     { get; init; }
    public int  AtsChecksLimit    { get; init; }
    public int  AiCallsRemaining  { get; init; }
    public int  AtsChecksRemaining { get; init; }
    public bool IsPremium         { get; init; }
    public string ResetDate       { get; init; } = string.Empty;
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
