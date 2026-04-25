using Microsoft.EntityFrameworkCore;
using ResumeAI.Auth.Data;
using ResumeAI.Auth.Entities;
using ResumeAI.Auth.Interfaces;

namespace ResumeAI.Auth.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthDbContext _context;

    public RefreshTokenRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> FindByTokenAsync(string token) =>
        await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token);

    public async Task<IList<RefreshToken>> FindActiveByUserIdAsync(int userId) =>
        await _context.RefreshTokens
            .AsNoTracking()
            .Where(r => r.UserId == userId && !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

    public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken;
    }

    public async Task RevokeAsync(string token)
    {
        await _context.RefreshTokens
            .Where(r => r.Token == token)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsRevoked, true));
    }

    public async Task RevokeAllForUserAsync(int userId)
    {
        await _context.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsRevoked, true));
    }

    public async Task DeleteExpiredAsync()
    {
        await _context.RefreshTokens
            .Where(r => r.ExpiresAt < DateTime.UtcNow)
            .ExecuteDeleteAsync();
    }
}
