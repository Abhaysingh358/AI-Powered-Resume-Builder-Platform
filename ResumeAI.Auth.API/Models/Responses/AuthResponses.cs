using ResumeAI.Auth.Enums;

namespace ResumeAI.Auth.Models.Responses;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserResponse User
);

public record UserResponse(
    int UserId,
    string FullName,
    string Email,
    string? Phone,
    string Role,
    string Provider,
    bool IsActive,
    string SubscriptionPlan,
    DateTime CreatedAt
);

public record ApiResponse<T>(
    bool Success,
    string Message,
    T? Data = default
)
{
    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new(true, message, data);

    public static ApiResponse<T> Fail(string message) =>
        new(false, message, default);
}
