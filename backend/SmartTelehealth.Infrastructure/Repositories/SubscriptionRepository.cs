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
            .Include(s => s.StatusHistory)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<IEnumerable<Subscription>> GetByUserIdAsync(int userId)
    {
        return await _context.Subscriptions
            .Where(s => s.UserId == userId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetActiveSubscriptionsAsync()
    {
        return await _context.Subscriptions
            .Where(s => s.Status == Subscription.SubscriptionStatuses.Active && !s.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetPausedSubscriptionsAsync()
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.Status == Subscription.SubscriptionStatuses.Paused && !s.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByStatusAsync(string status)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.Status == status && !s.IsDeleted)
            .ToListAsync();
    }

    public async Task<Subscription> CreateAsync(Subscription subscription)
    {
        subscription.CreatedDate = DateTime.UtcNow;
        // Set default values for new fields if needed
        if (string.IsNullOrEmpty(subscription.Status))
            subscription.Status = Subscription.SubscriptionStatuses.Pending;
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();
        return subscription;
    }

    public async Task<Subscription> UpdateAsync(Subscription subscription)
    {
        subscription.UpdatedDate = DateTime.UtcNow;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();
        return subscription;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var subscription = await _context.Subscriptions.FindAsync(id);
        if (subscription == null) return false;

        subscription.IsDeleted = true;
        subscription.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Subscriptions
            .AnyAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<int> GetActiveSubscriptionCountAsync(int userId)
    {
        return await _context.Subscriptions
            .CountAsync(s => s.UserId == userId && 
                           s.Status == "Active" && 
                           !s.IsDeleted);
    }

    public async Task<Subscription?> GetActiveSubscriptionByUserIdAsync(int userId)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.Provider)
            .Where(s => s.UserId == userId && 
                       s.Status == "Active" && 
                       !s.IsDeleted)
            .OrderByDescending(s => s.CreatedDate)
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
            .Where(s => s.Status == "Active" && 
                       s.NextBillingDate <= billingDate && 
                       !s.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsDueForDeliveryAsync(DateTime deliveryDate)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.Status == "Active" && 
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
        var category = await _context.Categories
            .Include(c => c.SubscriptionPlans)
            .FirstOrDefaultAsync(c => c.Id == categoryId);
        if (category == null) return new List<SubscriptionPlan>();
        return category.SubscriptionPlans
            .Where(sp => sp.IsActive && !sp.IsDeleted)
            .OrderBy(sp => sp.DisplayOrder)
            .ThenBy(sp => sp.Name)
            .ToList();
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
                       s.Status == "Active" && 
                       !s.IsDeleted)
            .ToListAsync();
    }

    public async Task<Subscription?> GetByPlanIdAsync(Guid planId)
    {
        return await _context.Subscriptions
            .Where(s => s.SubscriptionPlanId == planId && !s.IsDeleted)
            .FirstOrDefaultAsync();
    }

    // --- COMPLETED INTERFACE METHODS ---
    public async Task<IEnumerable<Subscription>> GetAllSubscriptionsAsync()
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.BillingCycle)
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.CreatedAt >= start && s.CreatedAt <= end && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<SubscriptionPlan> UpdatePlanAsync(SubscriptionPlan plan)
    {
        plan.UpdatedAt = DateTime.UtcNow;
        _context.SubscriptionPlans.Update(plan);
        await _context.SaveChangesAsync();
        return plan;
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByPlanIdAsync(Guid planId)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.SubscriptionPlanId == planId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    // New: Add status history
    public async Task AddStatusHistoryAsync(SubscriptionStatusHistory history)
    {
        _context.SubscriptionStatusHistories.Add(history);
        await _context.SaveChangesAsync();
    }
    // New: Add payment refund
    public async Task AddPaymentRefundAsync(PaymentRefund refund)
    {
        _context.PaymentRefunds.Add(refund);
        await _context.SaveChangesAsync();
    }

    public async Task<Category?> GetCategoryByNameAsync(string categoryName)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Name == categoryName);
    }

    public async Task<IEnumerable<Subscription>> GetSuspendedSubscriptionsAsync()
    {
        return await _context.Subscriptions
            .Where(s => s.Status == Subscription.SubscriptionStatuses.Suspended && !s.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsWithResetUsageAsync()
    {
        return await _context.Subscriptions
            .Where(s => s.Status == Subscription.SubscriptionStatuses.Active && !s.IsDeleted)
            .ToListAsync();
    }

    public async Task ResetUsageCountersAsync()
    {
        var activeSubscriptions = await _context.Subscriptions
            .Where(s => s.Status == Subscription.SubscriptionStatuses.Active && !s.IsDeleted)
            .ToListAsync();

        foreach (var subscription in activeSubscriptions)
        {
            // Reset usage counters for the subscription
            // This would typically involve updating related usage tracking entities
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsInDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Subscriptions
            .Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate && !s.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsWithFailedPaymentsAsync()
    {
        return await _context.Subscriptions
            .Where(s => s.Status == Subscription.SubscriptionStatuses.PaymentFailed && !s.IsDeleted)
            .ToListAsync();
    }

    // Additional missing methods for comprehensive subscription management
    public async Task<IEnumerable<Subscription>> GetSubscriptionsByBillingCycleAsync(string billingCycle)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.BillingCycle)
            .Where(s => s.BillingCycle.Name == billingCycle && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.CurrentPrice >= minPrice && s.CurrentPrice <= maxPrice && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByAutoRenewAsync(bool autoRenew)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.AutoRenew == autoRenew && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByTrialStatusAsync(bool isTrial)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.IsTrialSubscription == isTrial && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByProviderAsync(int providerId)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.ProviderId == providerId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByCategoryAsync(Guid categoryId)
    {
        var planIds = await _context.Categories
            .Where(c => c.Id == categoryId)
            .SelectMany(c => c.SubscriptionPlans)
            .Where(sp => sp.IsActive && !sp.IsDeleted)
            .Select(sp => sp.Id)
            .ToListAsync();

        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => planIds.Contains(s.SubscriptionPlanId) && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByPaymentMethodAsync(string paymentMethodId)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.PaymentMethodId == paymentMethodId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByStripeCustomerIdAsync(string stripeCustomerId)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.StripeCustomerId == stripeCustomerId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByLastBillingDateAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.LastBillingDate >= startDate && s.LastBillingDate <= endDate && !s.IsDeleted)
            .OrderByDescending(s => s.LastBillingDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByNextBillingDateAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.NextBillingDate >= startDate && s.NextBillingDate <= endDate && !s.IsDeleted)
            .OrderByDescending(s => s.NextBillingDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByFailedPaymentAttemptsAsync(int maxAttempts)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.FailedPaymentAttempts >= maxAttempts && !s.IsDeleted)
            .OrderByDescending(s => s.FailedPaymentAttempts)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByLastPaymentErrorAsync(string errorPattern)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.LastPaymentError != null && s.LastPaymentError.Contains(errorPattern) && !s.IsDeleted)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByUsageThresholdAsync(int usageThreshold)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.TotalUsageCount >= usageThreshold && !s.IsDeleted)
            .OrderByDescending(s => s.TotalUsageCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByPrivilegeAsync(string privilegeName)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .ThenInclude(sp => sp.PlanPrivileges)
            .Where(s => s.SubscriptionPlan.PlanPrivileges.Any(pp => pp.Privilege.Name == privilegeName) && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByExpirationDateAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.EndDate >= startDate && s.EndDate <= endDate && !s.IsDeleted)
            .OrderBy(s => s.EndDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByTrialEndDateAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.TrialEndDate >= startDate && s.TrialEndDate <= endDate && !s.IsDeleted)
            .OrderBy(s => s.TrialEndDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByPauseDateAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.PausedDate >= startDate && s.PausedDate <= endDate && !s.IsDeleted)
            .OrderByDescending(s => s.PausedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByResumeDateAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.ResumedDate >= startDate && s.ResumedDate <= endDate && !s.IsDeleted)
            .OrderByDescending(s => s.ResumedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByCancellationDateAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.CancelledDate >= startDate && s.CancelledDate <= endDate && !s.IsDeleted)
            .OrderByDescending(s => s.CancelledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsBySuspensionDateAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.SuspendedDate >= startDate && s.SuspendedDate <= endDate && !s.IsDeleted)
            .OrderByDescending(s => s.SuspendedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByLastPaymentFailedDateAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.LastPaymentFailedDate >= startDate && s.LastPaymentFailedDate <= endDate && !s.IsDeleted)
            .OrderByDescending(s => s.LastPaymentFailedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetTrialSubscriptionsAsync()
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.BillingCycle)
            .Where(s => s.IsTrialSubscription && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByPlanCategoryAsync(Guid categoryId)
    {
        var planIds = await _context.Categories
            .Where(c => c.Id == categoryId)
            .SelectMany(c => c.SubscriptionPlans)
            .Where(sp => sp.IsActive && !sp.IsDeleted)
            .Select(sp => sp.Id)
            .ToListAsync();

        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.BillingCycle)
            .Where(s => planIds.Contains(s.SubscriptionPlanId) && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByUsageCountAsync(int minUsageCount)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.BillingCycle)
            .Where(s => s.TotalUsageCount >= minUsageCount && !s.IsDeleted)
            .OrderByDescending(s => s.TotalUsageCount)
            .ToListAsync();
    }
} 