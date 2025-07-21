using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface ISubscriptionRepository
{
    // Subscription Plan Management
    Task<SubscriptionPlan?> GetSubscriptionPlanByIdAsync(Guid id);
    Task<IEnumerable<SubscriptionPlan>> GetAllSubscriptionPlansAsync();
    Task<IEnumerable<SubscriptionPlan>> GetActiveSubscriptionPlansAsync();
    Task<IEnumerable<SubscriptionPlan>> GetSubscriptionPlansByCategoryAsync(Guid categoryId);
    Task<SubscriptionPlan> CreateSubscriptionPlanAsync(SubscriptionPlan subscriptionPlan);
    Task<SubscriptionPlan> UpdateSubscriptionPlanAsync(SubscriptionPlan subscriptionPlan);
    Task<bool> DeleteSubscriptionPlanAsync(Guid id);
    
    // User Subscription Management
    Task<Subscription?> GetByIdAsync(Guid id);
    Task<IEnumerable<Subscription>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Subscription>> GetActiveSubscriptionsAsync();
    Task<IEnumerable<Subscription>> GetPausedSubscriptionsAsync();
    Task<IEnumerable<Subscription>> GetSubscriptionsByStatusAsync(string status);
    Task<IEnumerable<Subscription>> GetActiveSubscriptionsByPlanAsync(Guid planId);
    Task<Subscription?> GetByPlanIdAsync(Guid planId);
    Task<Subscription> CreateAsync(Subscription subscription);
    Task<Subscription> UpdateAsync(Subscription subscription);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetActiveSubscriptionCountAsync(Guid userId);
    Task<Subscription?> GetActiveSubscriptionByUserIdAsync(Guid userId);
    Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId);
    Task<IEnumerable<Subscription>> GetSubscriptionsDueForBillingAsync(DateTime billingDate);
    Task<IEnumerable<Subscription>> GetSubscriptionsDueForDeliveryAsync(DateTime deliveryDate);
    Task<IEnumerable<Subscription>> GetAllSubscriptionsAsync();
    Task<IEnumerable<Subscription>> GetSubscriptionsByDateRangeAsync(DateTime start, DateTime end);
    Task<SubscriptionPlan> UpdatePlanAsync(SubscriptionPlan plan);
    Task<IEnumerable<Subscription>> GetSubscriptionsByPlanIdAsync(Guid planId);
    Task AddStatusHistoryAsync(SubscriptionStatusHistory history);
    Task AddPaymentRefundAsync(PaymentRefund refund);
} 