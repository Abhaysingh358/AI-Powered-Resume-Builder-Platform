using Microsoft.EntityFrameworkCore;
using ResumeAI.Auth.Data;
using ResumeAI.Auth.Entities;
using ResumeAI.Auth.Enums;
using ResumeAI.Auth.Interfaces;

namespace ResumeAI.Auth.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;

    public UserRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<User?> FindByEmailAsync(string email) =>
        await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<User?> FindByUserIdAsync(int userId) =>
        await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);

    public async Task<User?> FindByProviderIdAsync(AuthProvider provider, string providerId) =>
        await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Provider == provider && u.ProviderId == providerId);

    public async Task<bool> ExistsByEmailAsync(string email) =>
        await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<IList<User>> FindAllByRoleAsync(Role role) =>
        await _context.Users
            .AsNoTracking()
            .Where(u => u.Role == role)
            .ToListAsync();

    public async Task<IList<User>> FindBySubscriptionPlanAsync(SubscriptionPlan plan) =>
        await _context.Users
            .AsNoTracking()
            .Where(u => u.SubscriptionPlan == plan)
            .ToListAsync();

    public async Task<IList<User>> FindByIsActiveAsync(bool isActive) =>
        await _context.Users
            .AsNoTracking()
            .Where(u => u.IsActive == isActive)
            .ToListAsync();

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteByUserIdAsync(int userId)
    {
        await _context.Users
            .Where(u => u.UserId == userId)
            .ExecuteDeleteAsync();
    }

    public async Task<int> CountAsync() =>
        await _context.Users.CountAsync();
}
