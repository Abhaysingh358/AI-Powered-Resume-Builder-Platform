using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeAI.Auth.Enums;
using ResumeAI.Auth.Interfaces;
using ResumeAI.Auth.Models.Requests;
using ResumeAI.Auth.Models.Responses;

namespace ResumeAI.Auth.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger      = logger;
    }

    //  POST /api/auth/register 

    /// <summary>Register a new user with email and password.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return CreatedAtAction(nameof(GetProfile), new { id = result.User.UserId },
                ApiResponse<AuthResponse>.Ok(result, "Registration successful."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    //  POST /api/auth/login 

    /// <summary>Authenticate with email + password.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
    }

    //   POST /api/auth/logout  

    /// <summary>Revoke the supplied refresh token (logout).</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
{
    await _authService.LogoutAsync(request.RefreshToken);
    return Ok(ApiResponse<object>.Ok(new { }, "Logged out successfully."));
}

    //   POST /api/auth/refresh  

    /// <summary>Issue a new access token using a valid refresh token.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Token refreshed."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
    }

    //   GET /api/auth/profile  

    /// <summary>Get the authenticated user's profile.</summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();
        var user   = await _authService.GetUserByIdAsync(userId);

        return Ok(ApiResponse<UserResponse>.Ok(new UserResponse(
            user.UserId, user.FullName, user.Email, user.Phone,
            user.Role.ToString(), user.Provider.ToString(),
            user.IsActive, user.SubscriptionPlan.ToString(), user.CreatedAt
        )));
    }

    // Hidden overload used by CreatedAtAction
    [HttpGet("profile/{id:int}", Name = "GetProfileById")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetProfile(int id)
    {
        try
        {
            var user = await _authService.GetUserByIdAsync(id);
            return Ok(ApiResponse<UserResponse>.Ok(new UserResponse(
                user.UserId, user.FullName, user.Email, user.Phone,
                user.Role.ToString(), user.Provider.ToString(),
                user.IsActive, user.SubscriptionPlan.ToString(), user.CreatedAt
            )));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    //   PUT /api/auth/profile  

    /// <summary>Update name and phone number.</summary>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();
        var user   = await _authService.UpdateProfileAsync(userId, request);

        return Ok(ApiResponse<UserResponse>.Ok(new UserResponse(
            user.UserId, user.FullName, user.Email, user.Phone,
            user.Role.ToString(), user.Provider.ToString(),
            user.IsActive, user.SubscriptionPlan.ToString(), user.CreatedAt
        ), "Profile updated."));
    }

    //   PUT /api/auth/password  

    /// <summary>Change password (LOCAL accounts only).</summary>
    [HttpPut("password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _authService.ChangePasswordAsync(userId, request);
            return Ok(ApiResponse<object>.Ok(new { }, "Password changed successfully."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    //   PUT /api/auth/subscription  

    /// <summary>Upgrade or downgrade subscription plan.</summary>
    [HttpPut("subscription")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSubscription([FromBody] UpdateSubscriptionRequest request)
    {
        var userId = GetCurrentUserId();
        await _authService.UpdateSubscriptionAsync(userId, request.Plan);
        return Ok(ApiResponse<object>.Ok(new { }, $"Subscription updated to {request.Plan}."));
    }

    //   DELETE /api/auth/deactivate 

    /// <summary>Soft-deactivate the authenticated account.</summary>
    [HttpDelete("deactivate")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateAccount()
    {
        var userId = GetCurrentUserId();
        await _authService.DeactivateAccountAsync(userId);
        return Ok(ApiResponse<object>.Ok(new { }, "Account deactivated."));
    }

    //   POST /api/auth/google  

    /// <summary>Sign in or register via Google OAuth ID token.</summary>
    [HttpPost("google")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        try
        {
            var result = await _authService.HandleGoogleCallbackAsync(request.IdToken);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Google login successful."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    //   Helpers  

    private int GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub")
               ?? throw new UnauthorizedAccessException("User identity not found.");
        return int.Parse(sub);
    }
}

/// <summary>Payload for Google OAuth login.</summary>
public record GoogleLoginRequest(string IdToken);
