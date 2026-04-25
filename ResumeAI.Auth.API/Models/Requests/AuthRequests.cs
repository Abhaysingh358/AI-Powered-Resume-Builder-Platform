using ResumeAI.Auth.Enums;

namespace ResumeAI.Auth.Models.Requests;

// --------------------------- Auth -----------------------------------------

public record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    string? Phone = null
);

public record LoginRequest(
    string Email,
    string Password
);

public record RefreshTokenRequest(
    string RefreshToken
);

// ------------------------------------------ Profile --------------------------------------------------------------------------

public record UpdateProfileRequest(
    string FullName,
    string? Phone
);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);

public record UpdateSubscriptionRequest(
    SubscriptionPlan Plan
);
