using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
    }

    public async Task<User?> GetByUserNameAsync(string userName)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == userName && !u.IsDeleted);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && 
                                     u.RefreshTokenExpiry > DateTime.UtcNow && 
                                     !u.IsDeleted);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive && !u.IsDeleted)
            .OrderBy(u => u.CreatedDate)
            .ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        user.CreatedDate = DateTime.UtcNow;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        user.IsDeleted = true;
        user.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Users
            .AnyAsync(u => u.Id == id && !u.IsDeleted);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email && !u.IsDeleted);
    }

    public async Task<int> GetActiveUserCountAsync()
    {
        return await _context.Users
            .CountAsync(u => u.IsActive && !u.IsDeleted);
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
    {
        return await _context.Users
            .Where(u => (u.FirstName.ToLower().Contains(searchTerm.ToLower()) ||
                        u.LastName.ToLower().Contains(searchTerm.ToLower()) ||
                        u.Email.ToLower().Contains(searchTerm.ToLower())) &&
                       !u.IsDeleted)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetUsersBySubscriptionStatusAsync(string status)
    {
        return await _context.Users
            .Include(u => u.Subscriptions.Where(s => s.Status == status && !s.IsDeleted))
            .Where(u => u.Subscriptions.Any(s => s.Status == status && !s.IsDeleted) && !u.IsDeleted)
            .ToListAsync();
    }

    public async Task<object> GetUserAnalyticsAsync()
    {
        var totalUsers = await _context.Users.CountAsync(u => !u.IsDeleted);
        var activeUsers = await _context.Users.CountAsync(u => u.IsActive && !u.IsDeleted);
        var newUsersThisMonth = await _context.Users.CountAsync(u => 
            u.CreatedAt >= DateTime.UtcNow.AddDays(-30) && !u.IsDeleted);

        return new
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            NewUsersThisMonth = newUsersThisMonth,
            ActiveUserPercentage = totalUsers > 0 ? (double)activeUsers / totalUsers * 100 : 0
        };
    }

    public async Task<IEnumerable<User>> GetByUserTypeAsync(string userType)
    {
        return await _context.Users
            .Where(u => u.UserType == userType && !u.IsDeleted)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();
    }

    public async Task<User?> GetByLicenseNumberAsync(string licenseNumber)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.LicenseNumber == licenseNumber && !u.IsDeleted);
    }

    public Task<IEnumerable<User>> GetByRoleAsync(string role) => throw new NotImplementedException();
} 