using ResumeAI.Auth.Entities;

namespace ResumeAI.Auth.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> FindByTokenAsync(string token);
    Task<IList<RefreshToken>> FindActiveByUserIdAsync(int userId);
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
    Task RevokeAsync(string token);
    Task RevokeAllForUserAsync(int userId);
    Task DeleteExpiredAsync();
}
