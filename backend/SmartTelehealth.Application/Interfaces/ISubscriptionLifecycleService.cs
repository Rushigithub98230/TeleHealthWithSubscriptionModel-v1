using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.Interfaces;

public interface ISubscriptionLifecycleService
{
    Task<bool> ActivateSubscriptionAsync(Guid subscriptionId, string? reason = null);
    Task<bool> PauseSubscriptionAsync(Guid subscriptionId, string? reason = null);
    Task<bool> ResumeSubscriptionAsync(Guid subscriptionId, string? reason = null);
    Task<bool> CancelSubscriptionAsync(Guid subscriptionId, string? reason = null);
    Task<bool> SuspendSubscriptionAsync(Guid subscriptionId, string? reason = null);
    Task<bool> RenewSubscriptionAsync(Guid subscriptionId, string? reason = null);
    Task<bool> ExpireSubscriptionAsync(Guid subscriptionId, string? reason = null);
    Task<bool> MarkPaymentFailedAsync(Guid subscriptionId, string? reason = null);
    Task<bool> MarkPaymentSucceededAsync(Guid subscriptionId, string? reason = null);
    Task<bool> UpdateSubscriptionStatusAsync(Guid subscriptionId, string newStatus, string? reason = null);
    Task<IEnumerable<SubscriptionStatusHistory>> GetStatusHistoryAsync(Guid subscriptionId);
    Task<bool> ValidateStatusTransitionAsync(string currentStatus, string newStatus);
    Task<string> GetNextValidStatusAsync(string currentStatus);
    
    // Process methods for automation
    Task<bool> ProcessSubscriptionExpirationAsync(Guid subscriptionId);
    Task<bool> ProcessSubscriptionSuspensionAsync(Guid subscriptionId, string reason);
}
