using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IBillingRepository
{
    Task<BillingRecord> GetByIdAsync(Guid id);
    Task<IEnumerable<BillingRecord>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<BillingRecord>> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<BillingRecord> CreateAsync(BillingRecord billingRecord);
    Task<BillingRecord> UpdateAsync(BillingRecord billingRecord);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<BillingRecord>> GetPendingPaymentsAsync();
    Task<IEnumerable<BillingRecord>> GetOverduePaymentsAsync();
    Task<IEnumerable<BillingRecord>> GetFailedPaymentsAsync();
    Task<BillingAdjustment> CreateAdjustmentAsync(BillingAdjustment adjustment);
    Task<IEnumerable<BillingAdjustment>> GetAdjustmentsByBillingRecordIdAsync(Guid billingRecordId);
    Task<IEnumerable<BillingRecord>> GetAllAsync();
    Task<IEnumerable<BillingRecord>> GetByBillingCycleIdAsync(Guid billingCycleId);
} 