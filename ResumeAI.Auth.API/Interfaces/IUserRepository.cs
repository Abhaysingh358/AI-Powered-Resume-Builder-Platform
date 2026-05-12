using ResumeAI.Auth.Entities;
using ResumeAI.Auth.Enums;

namespace ResumeAI.Auth.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByUserIdAsync(int userId);
    Task<User?> FindByProviderIdAsync(AuthProvider provider, string providerId);
    Task<bool> ExistsByEmailAsync(string email);
    Task<IList<User>> FindAllByRoleAsync(Role role);
    Task<IList<User>> FindBySubscriptionPlanAsync(SubscriptionPlan plan);
    Task<IList<User>> FindByIsActiveAsync(bool isActive);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteByUserIdAsync(int userId);
    Task<int> CountAsync();
}
