using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IAutomatedBillingService
{
    Task ProcessRecurringBillingAsync();
    Task ProcessSubscriptionRenewalAsync();
    Task ProcessFailedPaymentRetryAsync();
    Task ProcessPlanChangeAsync(Guid subscriptionId, Guid newPlanId);
    Task ProcessManualBillingAsync(Guid subscriptionId);
    Task<PaymentResultDto> ProcessPaymentAsync(Guid subscriptionId, decimal amount);
    Task<bool> ValidateBillingCycleAsync(Guid subscriptionId);
    Task<DateTime> CalculateNextBillingDateAsync(Guid subscriptionId);
    Task<decimal> CalculateProratedAmountAsync(Guid subscriptionId, DateTime effectiveDate);
}
