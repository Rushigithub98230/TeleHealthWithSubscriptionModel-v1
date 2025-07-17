using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly ApplicationDbContext _context;

    public SubscriptionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Subscription?> GetByIdAsync(Guid id)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<IEnumerable<Subscription>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Subscriptions
            .Where(s => s.UserId == userId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetActiveSubscriptionsAsync()
    {
        return await _context.Subscriptions
            .Where(s => s.Status == Subscription.SubscriptionStatus.Active && !s.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetPausedSubscriptionsAsync()
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
                .ThenInclude(sp => sp.Category)
            .Include(s => s.Provider)
            .Where(s => s.Status == Subscription.SubscriptionStatus.Paused && !s.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByStatusAsync(Subscription.SubscriptionStatus status)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
                .ThenInclude(sp => sp.Category)
            .Include(s => s.Provider)
            .Where(s => s.Status == status && !s.IsDeleted)
            .ToListAsync();
    }

    public async Task<Subscription> CreateAsync(Subscription subscription)
    {
        subscription.CreatedAt = DateTime.UtcNow;
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();
        return subscription;
    }

    public async Task<Subscription> UpdateAsync(Subscription subscription)
    {
        subscription.UpdatedAt = DateTime.UtcNow;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();
        return subscription;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var subscription = await _context.Subscriptions.FindAsync(id);
        if (subscription == null) return false;

        subscription.IsDeleted = true;
        subscription.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Subscriptions
            .AnyAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<int> GetActiveSubscriptionCountAsync(Guid userId)
    {
        return await _context.Subscriptions
            .CountAsync(s => s.UserId == userId && 
                           s.Status == Subscription.SubscriptionStatus.Active && 
                           !s.IsDeleted);
    }

    public async Task<Subscription?> GetActiveSubscriptionByUserIdAsync(Guid userId)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.Provider)
            .Where(s => s.UserId == userId && 
                       s.Status == Subscription.SubscriptionStatus.Active && 
                       !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId && !s.IsDeleted);
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsDueForBillingAsync(DateTime billingDate)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.Status == Subscription.SubscriptionStatus.Active && 
                       s.NextBillingDate <= billingDate && 
                       !s.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsDueForDeliveryAsync(DateTime deliveryDate)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.Status == Subscription.SubscriptionStatus.Active && 
                       !s.IsDeleted)
            .ToListAsync();
    }

    // Subscription Plan Management Methods
    public async Task<SubscriptionPlan?> GetSubscriptionPlanByIdAsync(Guid id)
    {
        return await _context.SubscriptionPlans
            .FirstOrDefaultAsync(sp => sp.Id == id && !sp.IsDeleted);
    }

    public async Task<IEnumerable<SubscriptionPlan>> GetAllSubscriptionPlansAsync()
    {
        return await _context.SubscriptionPlans
            .Where(sp => !sp.IsDeleted)
            .OrderBy(sp => sp.DisplayOrder)
            .ThenBy(sp => sp.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<SubscriptionPlan>> GetActiveSubscriptionPlansAsync()
    {
        return await _context.SubscriptionPlans
            .Where(sp => sp.IsActive && !sp.IsDeleted)
            .OrderBy(sp => sp.DisplayOrder)
            .ThenBy(sp => sp.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<SubscriptionPlan>> GetSubscriptionPlansByCategoryAsync(Guid categoryId)
    {
        return await _context.SubscriptionPlans
            .Include(sp => sp.Category)
            .Where(sp => sp.CategoryId == categoryId && sp.IsActive && !sp.IsDeleted)
            .OrderBy(sp => sp.DisplayOrder)
            .ThenBy(sp => sp.Name)
            .ToListAsync();
    }

    public async Task<SubscriptionPlan> CreateSubscriptionPlanAsync(SubscriptionPlan subscriptionPlan)
    {
        subscriptionPlan.CreatedAt = DateTime.UtcNow;
        _context.SubscriptionPlans.Add(subscriptionPlan);
        await _context.SaveChangesAsync();
        return subscriptionPlan;
    }

    public async Task<SubscriptionPlan> UpdateSubscriptionPlanAsync(SubscriptionPlan subscriptionPlan)
    {
        subscriptionPlan.UpdatedAt = DateTime.UtcNow;
        _context.SubscriptionPlans.Update(subscriptionPlan);
        await _context.SaveChangesAsync();
        return subscriptionPlan;
    }

    public async Task<bool> DeleteSubscriptionPlanAsync(Guid id)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(id);
        if (plan == null) return false;

        plan.IsDeleted = true;
        plan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Subscription>> GetActiveSubscriptionsByPlanAsync(Guid planId)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.SubscriptionPlanId == planId && 
                       s.Status == Subscription.SubscriptionStatus.Active && 
                       !s.IsDeleted)
            .ToListAsync();
    }

    public Task<Subscription?> GetByPlanIdAsync(int planId) => throw new NotImplementedException();

    // --- MISSING INTERFACE METHODS (STUBS) ---
    public Task<IEnumerable<Subscription>> GetAllSubscriptionsAsync()
        => Task.FromResult<IEnumerable<Subscription>>(new List<Subscription>());

    public Task<IEnumerable<Subscription>> GetSubscriptionsByDateRangeAsync(DateTime start, DateTime end)
        => Task.FromResult<IEnumerable<Subscription>>(new List<Subscription>());

    public Task<SubscriptionPlan> UpdatePlanAsync(SubscriptionPlan plan)
        => throw new NotImplementedException();

    public Task<IEnumerable<Subscription>> GetSubscriptionsByPlanIdAsync(Guid planId)
        => Task.FromResult<IEnumerable<Subscription>>(new List<Subscription>());
} 