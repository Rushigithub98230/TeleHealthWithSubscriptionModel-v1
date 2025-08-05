using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class SubscriptionStatusHistoryRepository : ISubscriptionStatusHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public SubscriptionStatusHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionStatusHistory> GetByIdAsync(Guid id)
    {
        return await _context.SubscriptionStatusHistories
            .Include(ssh => ssh.Subscription)
            .FirstOrDefaultAsync(ssh => ssh.Id == id);
    }

    public async Task<IEnumerable<SubscriptionStatusHistory>> GetBySubscriptionIdAsync(Guid subscriptionId)
    {
        return await _context.SubscriptionStatusHistories
            .Include(ssh => ssh.Subscription)
            .Where(ssh => ssh.SubscriptionId == subscriptionId)
            .OrderByDescending(ssh => ssh.CreatedAt)
            .ToListAsync();
    }

    public async Task<SubscriptionStatusHistory> CreateAsync(SubscriptionStatusHistory statusHistory)
    {
        _context.SubscriptionStatusHistories.Add(statusHistory);
        await _context.SaveChangesAsync();
        return statusHistory;
    }

    public async Task<SubscriptionStatusHistory> UpdateAsync(SubscriptionStatusHistory statusHistory)
    {
        _context.SubscriptionStatusHistories.Update(statusHistory);
        await _context.SaveChangesAsync();
        return statusHistory;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var statusHistory = await GetByIdAsync(id);
        if (statusHistory == null)
            return false;

        _context.SubscriptionStatusHistories.Remove(statusHistory);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<SubscriptionStatusHistory>> GetAllAsync()
    {
        return await _context.SubscriptionStatusHistories
            .Include(ssh => ssh.Subscription)
            .OrderByDescending(ssh => ssh.CreatedAt)
            .ToListAsync();
    }
} 