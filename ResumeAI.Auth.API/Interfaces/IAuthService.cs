using ResumeAI.Auth.Entities;
using ResumeAI.Auth.Enums;
using ResumeAI.Auth.Models.Requests;
using ResumeAI.Auth.Models.Responses;

namespace ResumeAI.Auth.Interfaces;

public interface IAuthService
{
    /// <summary>Register a new user with email + password.</summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    /// <summary>Authenticate with email + password. Returns JWT + refresh token.</summary>
    Task<AuthResponse> LoginAsync(LoginRequest request);

    /// <summary>Invalidate the given refresh token (logout).</summary>
    Task LogoutAsync(string refreshToken);

    /// <summary>Issue a new access token using a valid refresh token.</summary>
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);

    /// <summary>Validate an access token and return the userId, or null if invalid.</summary>
    Task<int?> ValidateTokenAsync(string accessToken);

    /// <summary>Return the user record by ID.</summary>
    Task<User> GetUserByIdAsync(int userId);

    /// <summary>Return the user record by email.</summary>
    Task<User> GetUserByEmailAsync(string email);

    /// <summary>Update name + phone on a user's profile.</summary>
    Task<User> UpdateProfileAsync(int userId, UpdateProfileRequest request);

    /// <summary>Change the password after verifying the current one.</summary>
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request);

    /// <summary>Upgrade or downgrade the subscription tier.</summary>
    Task UpdateSubscriptionAsync(int userId, SubscriptionPlan plan);

    /// <summary>Soft-deactivate an account (IsActive = false).</summary>
    Task DeactivateAccountAsync(int userId);

    /// <summary>Handle Google OAuth callback — register or return existing user.</summary>
    Task<AuthResponse> HandleGoogleCallbackAsync(string googleIdToken);
}
