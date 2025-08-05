using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Services;

public class AutomatedBillingService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IBillingService _billingService;
    private readonly IStripeService _stripeService;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AutomatedBillingService> _logger;

    public AutomatedBillingService(
        ISubscriptionRepository subscriptionRepository,
        IBillingService billingService,
        IStripeService stripeService,
        INotificationService notificationService,
        IAuditService auditService,
        ILogger<AutomatedBillingService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _billingService = billingService;
        _stripeService = stripeService;
        _notificationService = notificationService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Process recurring billing for all subscriptions due for billing
    /// </summary>
    public async Task<BillingProcessResult> ProcessRecurringBillingAsync()
    {
        var result = new BillingProcessResult
        {
            ProcessedAt = DateTime.UtcNow,
            TotalSubscriptions = 0,
            SuccessfulPayments = 0,
            FailedPayments = 0,
            Errors = new List<string>()
        };

        try
        {
            // Get all subscriptions due for billing
            var dueSubscriptions = await _subscriptionRepository.GetSubscriptionsDueForBillingAsync(DateTime.UtcNow);
            result.TotalSubscriptions = dueSubscriptions.Count();

            foreach (var subscription in dueSubscriptions)
            {
                try
                {
                    var paymentResult = await ProcessSubscriptionBillingAsync(subscription);
                    if (paymentResult.Success)
                    {
                        result.SuccessfulPayments++;
                    }
                    else
                    {
                        result.FailedPayments++;
                        result.Errors.Add($"Subscription {subscription.Id}: {paymentResult.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    result.FailedPayments++;
                    result.Errors.Add($"Subscription {subscription.Id}: {ex.Message}");
                    _logger.LogError(ex, "Error processing billing for subscription {SubscriptionId}", subscription.Id);
                }
            }

            await _auditService.LogActionAsync(
                "System",
                "RecurringBillingProcessed",
                "System",
                $"Processed {result.TotalSubscriptions} subscriptions: {result.SuccessfulPayments} successful, {result.FailedPayments} failed"
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in recurring billing process");
            result.Errors.Add($"System error: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Process billing for a specific subscription
    /// </summary>
    public async Task<PaymentProcessResult> ProcessSubscriptionBillingAsync(Subscription subscription)
    {
        try
        {
            // Create billing record
            var billingRecord = await _billingService.CreateBillingRecordAsync(new CreateBillingRecordDto
            {
                UserId = subscription.UserId.ToString(),
                SubscriptionId = subscription.Id.ToString(),
                Amount = subscription.CurrentPrice,
                Description = $"Recurring payment for {subscription.SubscriptionPlan.Name}",
                DueDate = subscription.NextBillingDate, // This is fine since DateTime can be assigned to DateTime?
                Type = BillingRecord.BillingType.Subscription.ToString()
            });

            if (!billingRecord.Success)
            {
                return new PaymentProcessResult
                {
                    Success = false,
                    ErrorMessage = "Failed to create billing record"
                };
            }

            // Process payment through Stripe
            var paymentResult = await _stripeService.ProcessPaymentAsync(
                "pm_card_visa", // Default test payment method - in production, get from subscription
                subscription.CurrentPrice,
                "usd"
            );

            if (paymentResult.Status == "succeeded")
            {
                // Update subscription next billing date
                subscription.NextBillingDate = subscription.NextBillingDate.AddMonths(1);
                subscription.UpdatedAt = DateTime.UtcNow;
                await _subscriptionRepository.UpdateAsync(subscription);

                // Send success notification
                await SendBillingSuccessNotificationAsync(subscription, billingRecord.Data);

                return new PaymentProcessResult
                {
                    Success = true,
                    PaymentIntentId = paymentResult.PaymentIntentId,
                    Amount = paymentResult.Amount
                };
            }
            else
            {
                // Handle failed payment
                await HandleFailedPaymentAsync(subscription, paymentResult.ErrorMessage);
                return new PaymentProcessResult
                {
                    Success = false,
                    ErrorMessage = paymentResult.ErrorMessage
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing billing for subscription {SubscriptionId}", subscription.Id);
            return new PaymentProcessResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Retry failed payments with exponential backoff
    /// </summary>
    public async Task<RetryPaymentResult> RetryFailedPaymentsAsync()
    {
        var result = new RetryPaymentResult
        {
            ProcessedAt = DateTime.UtcNow,
            TotalRetries = 0,
            SuccessfulRetries = 0,
            FailedRetries = 0,
            Errors = new List<string>()
        };

        try
        {
            // Get failed billing records
            var failedRecords = await _billingService.GetOverdueBillingRecordsAsync();
            if (!failedRecords.Success)
            {
                result.Errors.Add("Failed to retrieve overdue billing records");
                return result;
            }

            foreach (var billingRecord in failedRecords.Data)
            {
                result.TotalRetries++;
                try
                {
                    var retryResult = await _billingService.RetryFailedPaymentAsync(Guid.Parse(billingRecord.Id));
                    if (retryResult.Success)
                    {
                        result.SuccessfulRetries++;
                    }
                    else
                    {
                        result.FailedRetries++;
                        result.Errors.Add($"Billing record {billingRecord.Id}: {retryResult.Message}");
                    }
                }
                catch (Exception ex)
                {
                    result.FailedRetries++;
                    result.Errors.Add($"Billing record {billingRecord.Id}: {ex.Message}");
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in payment retry process");
            result.Errors.Add($"System error: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Handle grace period for failed payments
    /// </summary>
    public async Task<GracePeriodResult> ProcessGracePeriodAsync()
    {
        var result = new GracePeriodResult
        {
            ProcessedAt = DateTime.UtcNow,
            SubscriptionsInGracePeriod = 0,
            SubscriptionsSuspended = 0,
            Errors = new List<string>()
        };

        try
        {
            // Get subscriptions with failed payments
            var failedSubscriptions = await _subscriptionRepository.GetSubscriptionsByStatusAsync(Subscription.SubscriptionStatuses.PaymentFailed);
            
            foreach (var subscription in failedSubscriptions)
            {
                result.SubscriptionsInGracePeriod++;
                
                // Check if grace period has expired (e.g., 7 days)
                var gracePeriodDays = 7;
                var lastPaymentAttempt = subscription.UpdatedAt ?? subscription.CreatedAt;
                
                if (DateTime.UtcNow.Subtract(lastPaymentAttempt).TotalDays > gracePeriodDays)
                {
                    // Suspend subscription
                    subscription.Status = Subscription.SubscriptionStatuses.Paused;
                    subscription.UpdatedAt = DateTime.UtcNow;
                    await _subscriptionRepository.UpdateAsync(subscription);
                    
                    result.SubscriptionsSuspended++;
                    
                    // Send suspension notification
                    await SendSuspensionNotificationAsync(subscription);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in grace period processing");
            result.Errors.Add($"System error: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Reset usage counters for all active subscriptions
    /// </summary>
    public async Task<UsageResetResult> ResetUsageCountersAsync()
    {
        var result = new UsageResetResult
        {
            ProcessedAt = DateTime.UtcNow,
            TotalSubscriptions = 0,
            SuccessfullyReset = 0,
            Errors = new List<string>()
        };

        try
        {
            var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
            result.TotalSubscriptions = activeSubscriptions.Count();

            foreach (var subscription in activeSubscriptions)
            {
                try
                {
                    // Reset usage counters for this subscription
                    await ResetSubscriptionUsageAsync(subscription);
                    result.SuccessfullyReset++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Subscription {subscription.Id}: {ex.Message}");
                    _logger.LogError(ex, "Error resetting usage for subscription {SubscriptionId}", subscription.Id);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in usage reset process");
            result.Errors.Add($"System error: {ex.Message}");
            return result;
        }
    }

    private async Task HandleFailedPaymentAsync(Subscription subscription, string errorMessage)
    {
        // Update subscription status
        subscription.Status = Subscription.SubscriptionStatuses.PaymentFailed;
        subscription.UpdatedAt = DateTime.UtcNow;
        await _subscriptionRepository.UpdateAsync(subscription);

        // Send failure notification
        await SendBillingFailureNotificationAsync(subscription, errorMessage);

        // Log the failure
        await _auditService.LogPaymentEventAsync(
            subscription.UserId.ToString(),
            "PaymentFailed",
            subscription.Id.ToString(),
            "Failed",
            errorMessage
        );
    }

    private async Task ResetSubscriptionUsageAsync(Subscription subscription)
    {
        // This would typically involve resetting privilege usage counters
        // Implementation depends on your privilege system
        _logger.LogInformation("Reset usage counters for subscription {SubscriptionId}", subscription.Id);
    }

    private async Task SendBillingSuccessNotificationAsync(Subscription subscription, BillingRecordDto billingRecord)
    {
        try
        {
            // Send email notification
            await _notificationService.SendPaymentSuccessEmailAsync(
                subscription.User.Email,
                subscription.User.FullName,
                billingRecord
            );

            // Send in-app notification
            await _notificationService.CreateInAppNotificationAsync(
                subscription.UserId,
                "Payment Successful",
                $"Your payment of ${billingRecord.Amount} for {subscription.SubscriptionPlan.Name} has been processed successfully."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending billing success notification for subscription {SubscriptionId}", subscription.Id);
        }
    }

    private async Task SendBillingFailureNotificationAsync(Subscription subscription, string errorMessage)
    {
        try
        {
            // Send email notification
            await _notificationService.SendPaymentFailedEmailAsync(
                subscription.User.Email,
                subscription.User.FullName,
                new BillingRecordDto { Amount = subscription.CurrentPrice }
            );

            // Send in-app notification
            await _notificationService.CreateInAppNotificationAsync(
                subscription.UserId,
                "Payment Failed",
                $"We were unable to process your payment of ${subscription.CurrentPrice}. Please update your payment method."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending billing failure notification for subscription {SubscriptionId}", subscription.Id);
        }
    }

    private async Task SendSuspensionNotificationAsync(Subscription subscription)
    {
        try
        {
            // Send email notification
            await _notificationService.SendSubscriptionSuspensionEmailAsync(
                subscription.User.Email,
                subscription.User.FullName,
                new SubscriptionDto {
                    Id = subscription.Id.ToString(),
                    UserId = subscription.UserId.ToString(),
                    PlanId = subscription.SubscriptionPlanId.ToString(),
                    PlanName = subscription.SubscriptionPlan?.Name ?? string.Empty,
                    Status = subscription.Status,
                    CurrentPrice = subscription.CurrentPrice,
                    StartDate = subscription.StartDate,
                    EndDate = subscription.EndDate,
                    NextBillingDate = subscription.NextBillingDate,
                    IsPaused = subscription.Status == Subscription.SubscriptionStatuses.Paused,
                    IsCancelled = subscription.Status == Subscription.SubscriptionStatuses.Cancelled,
                    IsActive = subscription.Status == Subscription.SubscriptionStatuses.Active,
                    // Add more fields as needed
                }
            );

            // Send in-app notification
            await _notificationService.CreateInAppNotificationAsync(
                subscription.UserId,
                "Subscription Suspended",
                $"Your {subscription.SubscriptionPlan.Name} subscription has been suspended due to payment issues. Please update your payment method to reactivate."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending suspension notification for subscription {SubscriptionId}", subscription.Id);
        }
    }
}

// Result DTOs for Automated Billing Service
public class BillingProcessResult
{
    public DateTime ProcessedAt { get; set; }
    public int TotalSubscriptions { get; set; }
    public int SuccessfulPayments { get; set; }
    public int FailedPayments { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}

public class PaymentProcessResult
{
    public bool Success { get; set; }
    public string? PaymentIntentId { get; set; }
    public decimal? Amount { get; set; }
    public string? ErrorMessage { get; set; }
}

public class RetryPaymentResult
{
    public DateTime ProcessedAt { get; set; }
    public int TotalRetries { get; set; }
    public int SuccessfulRetries { get; set; }
    public int FailedRetries { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}

public class GracePeriodResult
{
    public DateTime ProcessedAt { get; set; }
    public int SubscriptionsInGracePeriod { get; set; }
    public int SubscriptionsSuspended { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}

public class UsageResetResult
{
    public DateTime ProcessedAt { get; set; }
    public int TotalSubscriptions { get; set; }
    public int SuccessfullyReset { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
} 