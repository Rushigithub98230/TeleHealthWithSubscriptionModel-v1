using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUserNameAsync(string userName);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsByEmailAsync(string email);
    Task<int> GetActiveUserCountAsync();
    Task<IEnumerable<User>> SearchUsersAsync(string searchTerm);
    Task<IEnumerable<User>> GetUsersBySubscriptionStatusAsync(Subscription.SubscriptionStatus status);
    Task<object> GetUserAnalyticsAsync();
    Task<IEnumerable<User>> GetByUserTypeAsync(string userType);
    Task<User?> GetByLicenseNumberAsync(string licenseNumber);
    Task<IEnumerable<User>> GetByRoleAsync(string role);
} 