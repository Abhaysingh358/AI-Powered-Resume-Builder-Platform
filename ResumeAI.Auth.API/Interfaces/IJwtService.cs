using System.Security.Claims;
using ResumeAI.Auth.Entities;

namespace ResumeAI.Auth.Interfaces;

public interface IJwtService
{
    /// <summary>Generate a signed JWT access token for the user.</summary>
    string GenerateAccessToken(User user);

    /// <summary>Generate a cryptographically random opaque refresh token.</summary>
    string GenerateRefreshToken();

    /// <summary>Extract the ClaimsPrincipal from a JWT (expired tokens accepted).</summary>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

    /// <summary>Return the expiry DateTime of the access token.</summary>
    DateTime GetAccessTokenExpiry();
}
