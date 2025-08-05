using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface ISubscriptionStatusHistoryRepository
{
    Task<SubscriptionStatusHistory> GetByIdAsync(Guid id);
    Task<IEnumerable<SubscriptionStatusHistory>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<SubscriptionStatusHistory> CreateAsync(SubscriptionStatusHistory statusHistory);
    Task<SubscriptionStatusHistory> UpdateAsync(SubscriptionStatusHistory statusHistory);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<SubscriptionStatusHistory>> GetAllAsync();
} 