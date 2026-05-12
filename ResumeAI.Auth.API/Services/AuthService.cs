using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using ResumeAI.Auth.Entities;
using ResumeAI.Auth.Enums;
using ResumeAI.Auth.Interfaces;
using ResumeAI.Auth.Models.Requests;
using ResumeAI.Auth.Models.Responses;

namespace ResumeAI.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenRepository _tokenRepo;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    // ASP.NET Core Identity password hasher — no full Identity pipeline needed
    private readonly PasswordHasher<User> _hasher = new();

    private const int RefreshTokenExpiryDays = 30;

    public AuthService(
        IUserRepository userRepo,
        IRefreshTokenRepository tokenRepo,
        IJwtService jwtService,
        IConfiguration config,
        ILogger<AuthService> logger)
    {
        _userRepo   = userRepo;
        _tokenRepo  = tokenRepo;
        _jwtService = jwtService;
        _config     = config;
        _logger     = logger;
    }

    // ---------------- Register -------------------------------

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepo.ExistsByEmailAsync(request.Email))
            throw new InvalidOperationException($"Email '{request.Email}' is already registered.");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email    = request.Email.Trim().ToLower(),
            Phone    = request.Phone?.Trim(),
            Provider = AuthProvider.LOCAL,
            Role     = Role.USER,
        };

        // Hash the password before persisting
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        user = await _userRepo.CreateAsync(user);
        _logger.LogInformation("New user registered: {Email} (UserId={UserId})", user.Email, user.UserId);

        return await BuildAuthResponseAsync(user);
    }

    // ------------------------ Login -------------------------------------

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepo.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated.");

        if (user.Provider != AuthProvider.LOCAL || user.PasswordHash is null)
            throw new UnauthorizedAccessException("Please sign in with your OAuth provider.");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid email or password.");

        // Rehash if the algorithm changed
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _hasher.HashPassword(user, request.Password);
            await _userRepo.UpdateAsync(user);
        }

        _logger.LogInformation("User logged in: {Email}", user.Email);
        return await BuildAuthResponseAsync(user);
    }

    // -------------------------------- Logout --------------------------

    public async Task LogoutAsync(string refreshToken)
    {
        await _tokenRepo.RevokeAsync(refreshToken);
        _logger.LogInformation("Refresh token revoked.");
    }

    // ------------------------------- Refresh Token ------------------------------------

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _tokenRepo.FindByTokenAsync(refreshToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (storedToken.IsRevoked)
            throw new UnauthorizedAccessException("Refresh token has been revoked.");

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token has expired.");

        if (storedToken.User is null)
            throw new UnauthorizedAccessException("User not found.");

        // Rotate: revoke old, issue new
        await _tokenRepo.RevokeAsync(refreshToken);
        return await BuildAuthResponseAsync(storedToken.User);
    }

    // -------------------------------- Validate Access Token -------------------------------

    public async Task<int?> ValidateTokenAsync(string accessToken)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
        if (principal is null) return null;

        var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!int.TryParse(sub, out var userId)) return null;

        var user = await _userRepo.FindByUserIdAsync(userId);
        return (user is { IsActive: true }) ? userId : null;
    }

    // -------------------------------------- Get User ---------------------------------

    public async Task<User> GetUserByIdAsync(int userId) =>
        await _userRepo.FindByUserIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

    public async Task<User> GetUserByEmailAsync(string email) =>
        await _userRepo.FindByEmailAsync(email)
            ?? throw new KeyNotFoundException($"User with email '{email}' not found.");

    // -------------------------------- Update Profile ------------------------------------------

    public async Task<User> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await GetTrackedUserAsync(userId);
        user.FullName = request.FullName.Trim();
        user.Phone    = request.Phone?.Trim();
        return await _userRepo.UpdateAsync(user);
    }

    // --------------------------- Change Password ---------------------------------------

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await GetTrackedUserAsync(userId);

        if (user.Provider != AuthProvider.LOCAL || user.PasswordHash is null)
            throw new InvalidOperationException("Password change is only supported for local accounts.");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Current password is incorrect.");

        user.PasswordHash = _hasher.HashPassword(user, request.NewPassword);
        await _userRepo.UpdateAsync(user);

        // Revoke all existing refresh tokens after password change
        await _tokenRepo.RevokeAllForUserAsync(userId);
        _logger.LogInformation("Password changed for UserId={UserId}", userId);
    }

    // ------------------------------- Update Subscription ----------------------------------------

    public async Task UpdateSubscriptionAsync(int userId, SubscriptionPlan plan)
    {
        var user = await GetTrackedUserAsync(userId);
        user.SubscriptionPlan = plan;
        await _userRepo.UpdateAsync(user);
        _logger.LogInformation("UserId={UserId} subscription changed to {Plan}", userId, plan);
    }

    // -------------------------------- Deactivate Account ---------------------------------------------

    public async Task DeactivateAccountAsync(int userId)
    {
        var user = await GetTrackedUserAsync(userId);
        user.IsActive = false;
        await _userRepo.UpdateAsync(user);
        await _tokenRepo.RevokeAllForUserAsync(userId);
        _logger.LogInformation("UserId={UserId} account deactivated.", userId);
    }

    // ----------------------------- Google OAuth --------------------------------------

    public async Task<AuthResponse> HandleGoogleCallbackAsync(string googleIdToken)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _config["Google:ClientId"] }
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(googleIdToken, settings);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Google token validation failed.");
            throw new UnauthorizedAccessException("Invalid Google token.");
        }

        // Find existing user by Google subject ID
        var user = await _userRepo.FindByProviderIdAsync(AuthProvider.GOOGLE, payload.Subject);

        if (user is null)
        {
            // First-time Google sign-in → auto-register
            if (await _userRepo.ExistsByEmailAsync(payload.Email))
                throw new InvalidOperationException(
                    $"Email '{payload.Email}' is already registered with a different provider.");

            user = new User
            {
                FullName   = payload.Name,
                Email      = payload.Email.ToLower(),
                Provider   = AuthProvider.GOOGLE,
                ProviderId = payload.Subject,
                Role       = Role.USER,
            };
            user = await _userRepo.CreateAsync(user);
            _logger.LogInformation("New Google user registered: {Email}", user.Email);
        }

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated.");

        return await BuildAuthResponseAsync(user);
    }

    // ------------------------------- Private Helpers ------------------------------------

    /// <summary>Build JWT + refresh token and persist the refresh token.</summary>
    private async Task<AuthResponse> BuildAuthResponseAsync(User user)
    {
        var accessToken  = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        await _tokenRepo.CreateAsync(new RefreshToken
        {
            UserId    = user.UserId,
            Token     = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays),
        });

        return new AuthResponse(
            AccessToken:  accessToken,
            RefreshToken: refreshToken,
            ExpiresAt:    _jwtService.GetAccessTokenExpiry(),
            User:         MapToUserResponse(user)
        );
    }

    /// <summary>Load a tracked (non-AsNoTracking) user entity for mutation.</summary>
    private async Task<User> GetTrackedUserAsync(int userId)
    {
        // We need a tracked entity for update — bypass the AsNoTracking repo query
        var user = await _userRepo.FindByUserIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");
        return user;
    }

    private static UserResponse MapToUserResponse(User u) => new(
        UserId:           u.UserId,
        FullName:         u.FullName,
        Email:            u.Email,
        Phone:            u.Phone,
        Role:             u.Role.ToString(),
        Provider:         u.Provider.ToString(),
        IsActive:         u.IsActive,
        SubscriptionPlan: u.SubscriptionPlan.ToString(),
        CreatedAt:        u.CreatedAt
    );
}
