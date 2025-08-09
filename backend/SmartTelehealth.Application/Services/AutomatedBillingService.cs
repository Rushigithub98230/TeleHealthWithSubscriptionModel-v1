using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Services;

public class AutomatedBillingService : IAutomatedBillingService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IBillingService _billingService;
    private readonly IStripeService _stripeService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AutomatedBillingService> _logger;

    public AutomatedBillingService(
        ISubscriptionRepository subscriptionRepository,
        IBillingService billingService,
        IStripeService stripeService,
        IAuditService auditService,
        ILogger<AutomatedBillingService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _billingService = billingService;
        _stripeService = stripeService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task ProcessRecurringBillingAsync()
    {
        try
        {
            _logger.LogInformation("Starting recurring billing process");
            
            // Get all active subscriptions that are due for billing
            var dueSubscriptions = await _subscriptionRepository.GetSubscriptionsDueForBillingAsync(DateTime.UtcNow);
            
            foreach (var subscription in dueSubscriptions)
            {
                try
                {
                    await ProcessSubscriptionBillingAsync(subscription);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing billing for subscription {SubscriptionId}", subscription.Id);
                }
            }
            
            _logger.LogInformation("Completed recurring billing process");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in recurring billing process");
            throw;
        }
    }

    public async Task ProcessSubscriptionRenewalAsync()
    {
        try
        {
            _logger.LogInformation("Starting subscription renewal process");
            
            // Get subscriptions that need renewal
            var renewals = await _subscriptionRepository.GetAllSubscriptionsAsync();
            renewals = renewals.Where(s => s.Status == Subscription.SubscriptionStatuses.Active && 
                                          s.EndDate.HasValue && 
                                          s.EndDate.Value <= DateTime.UtcNow.AddDays(7));
            
            foreach (var subscription in renewals)
            {
                try
                {
                    await ProcessSubscriptionRenewalAsync(subscription);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing renewal for subscription {SubscriptionId}", subscription.Id);
                }
            }
            
            _logger.LogInformation("Completed subscription renewal process");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in subscription renewal process");
            throw;
        }
    }

    public async Task ProcessFailedPaymentRetryAsync()
    {
        try
        {
            _logger.LogInformation("Starting failed payment retry process");
            
            // Get subscriptions with failed payments that can be retried
            var failedPayments = await _subscriptionRepository.GetAllSubscriptionsAsync();
            failedPayments = failedPayments.Where(s => s.Status == Subscription.SubscriptionStatuses.PaymentFailed);
            
            foreach (var subscription in failedPayments)
            {
                try
                {
                    await RetryFailedPaymentAsync(subscription);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrying payment for subscription {SubscriptionId}", subscription.Id);
                }
            }
            
            _logger.LogInformation("Completed failed payment retry process");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in failed payment retry process");
            throw;
        }
    }

    public async Task ProcessPlanChangeAsync(Guid subscriptionId, Guid newPlanId)
    {
        try
        {
            _logger.LogInformation("Processing plan change for subscription {SubscriptionId} to plan {NewPlanId}", subscriptionId, newPlanId);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", subscriptionId);
                return;
            }
            
            // Calculate prorated amounts
            var proratedAmount = await CalculateProratedAmountAsync(subscriptionId, DateTime.UtcNow);
            
            // Process the plan change
            await _subscriptionRepository.UpdatePlanAsync(subscription.SubscriptionPlan);
            
            // Create billing record for the change
            var billingRecord = new CreateBillingRecordDto
            {
                UserId = subscription.UserId.ToString(),
                SubscriptionId = subscriptionId.ToString(),
                Amount = proratedAmount,
                Description = $"Plan change to {subscription.SubscriptionPlan.Name}",
                Type = BillingRecord.BillingType.Subscription.ToString()
            };
            
            await _billingService.CreateBillingRecordAsync(billingRecord);
            
            _logger.LogInformation("Successfully processed plan change for subscription {SubscriptionId}", subscriptionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing plan change for subscription {SubscriptionId}", subscriptionId);
            throw;
        }
    }

    public async Task ProcessManualBillingAsync(Guid subscriptionId)
    {
        try
        {
            _logger.LogInformation("Processing manual billing for subscription {SubscriptionId}", subscriptionId);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", subscriptionId);
                return;
            }
            
            await ProcessSubscriptionBillingAsync(subscription);
            
            _logger.LogInformation("Successfully processed manual billing for subscription {SubscriptionId}", subscriptionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing manual billing for subscription {SubscriptionId}", subscriptionId);
            throw;
        }
    }

    public async Task<PaymentResultDto> ProcessPaymentAsync(Guid subscriptionId, decimal amount)
    {
        try
        {
            _logger.LogInformation("Processing payment for subscription {SubscriptionId} with amount {Amount}", subscriptionId, amount);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                return new PaymentResultDto
                {
                    Status = "failed",
                    ErrorMessage = "Subscription not found"
                };
            }
            
            // Process payment through Stripe
            var paymentResult = await _stripeService.ProcessPaymentAsync(subscription.StripeCustomerId, amount, "usd");
            
            if (paymentResult.Success)
            {
                // Create billing record
                var billingRecord = new CreateBillingRecordDto
                {
                    UserId = subscription.UserId.ToString(),
                    SubscriptionId = subscriptionId.ToString(),
                    Amount = amount,
                    Description = $"Subscription payment for {subscription.SubscriptionPlan?.Name ?? "Unknown Plan"}",
                    Type = BillingRecord.BillingType.Subscription.ToString(),
                    StripePaymentIntentId = paymentResult.PaymentIntentId
                };
                
                await _billingService.CreateBillingRecordAsync(billingRecord);
                
                _logger.LogInformation("Successfully processed payment for subscription {SubscriptionId}", subscriptionId);
            }
            else
            {
                _logger.LogWarning("Payment failed for subscription {SubscriptionId}: {Error}", subscriptionId, paymentResult.ErrorMessage);
            }
            
            return paymentResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for subscription {SubscriptionId}", subscriptionId);
            return new PaymentResultDto
            {
                Status = "failed",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> ValidateBillingCycleAsync(Guid subscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null) return false;
            
            // Check if billing cycle is valid
            var now = DateTime.UtcNow;
            var nextBilling = subscription.NextBillingDate;
            
            return nextBilling <= now;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating billing cycle for subscription {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    public async Task<DateTime> CalculateNextBillingDateAsync(Guid subscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                throw new ArgumentException("Subscription not found");
            }
            
            var currentDate = subscription.NextBillingDate;
            var billingCycle = subscription.BillingCycle;
            
            // Calculate next billing date based on billing cycle
            return billingCycle.DurationInDays switch
            {
                1 => currentDate.AddDays(1), // Daily
                7 => currentDate.AddDays(7), // Weekly
                30 => currentDate.AddMonths(1), // Monthly
                90 => currentDate.AddMonths(3), // Quarterly
                365 => currentDate.AddYears(1), // Yearly
                _ => currentDate.AddDays(billingCycle.DurationInDays)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating next billing date for subscription {SubscriptionId}", subscriptionId);
            throw;
        }
    }

    public async Task<decimal> CalculateProratedAmountAsync(Guid subscriptionId, DateTime effectiveDate)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                throw new ArgumentException("Subscription not found");
            }
            
            var plan = subscription.SubscriptionPlan;
            var billingCycle = subscription.BillingCycle;
            
            // Calculate prorated amount based on remaining days in current billing cycle
            var currentDate = DateTime.UtcNow;
            var nextBillingDate = subscription.NextBillingDate;
            var remainingDays = (nextBillingDate - currentDate).Days;
            var totalDays = billingCycle.DurationInDays;
            
            var proratedRatio = (decimal)remainingDays / totalDays;
            var proratedAmount = plan.Price * proratedRatio;
            
            return Math.Max(0, proratedAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating prorated amount for subscription {SubscriptionId}", subscriptionId);
            throw;
        }
    }

    private async Task ProcessSubscriptionBillingAsync(Subscription subscription)
    {
        try
        {
            // Validate billing cycle
            if (!await ValidateBillingCycleAsync(subscription.Id))
            {
                _logger.LogInformation("Subscription {SubscriptionId} is not due for billing", subscription.Id);
                return;
            }
            
            // Process payment
            var paymentResult = await ProcessPaymentAsync(subscription.Id, subscription.SubscriptionPlan?.Price ?? 0);
            
            if (paymentResult.Success)
            {
                // Update subscription status
                subscription.Status = Subscription.SubscriptionStatuses.Active;
                subscription.LastBillingDate = DateTime.UtcNow;
                subscription.NextBillingDate = await CalculateNextBillingDateAsync(subscription.Id);
                subscription.FailedPaymentAttempts = 0;
                subscription.LastPaymentError = null;
                await _subscriptionRepository.UpdateAsync(subscription);
                
                _logger.LogInformation("Successfully processed billing for subscription {SubscriptionId}", subscription.Id);
            }
            else
            {
                // Handle failed payment
                subscription.Status = Subscription.SubscriptionStatuses.PaymentFailed;
                subscription.FailedPaymentAttempts++;
                subscription.LastPaymentError = paymentResult.ErrorMessage;
                subscription.LastPaymentFailedDate = DateTime.UtcNow;
                await _subscriptionRepository.UpdateAsync(subscription);
                
                _logger.LogWarning("Payment failed for subscription {SubscriptionId}: {Error}", subscription.Id, paymentResult.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing billing for subscription {SubscriptionId}", subscription.Id);
            throw;
        }
    }

    private async Task ProcessSubscriptionRenewalAsync(Subscription subscription)
    {
        try
        {
            // Check if subscription should be renewed
            if (subscription.Status != Subscription.SubscriptionStatuses.Active && subscription.Status != Subscription.SubscriptionStatuses.Expired)
            {
                return;
            }
            
            // Process renewal payment
            var paymentResult = await ProcessPaymentAsync(subscription.Id, subscription.SubscriptionPlan?.Price ?? 0);
            
            if (paymentResult.Success)
            {
                // Update subscription for renewal
                subscription.Status = Subscription.SubscriptionStatuses.Active;
                subscription.StartDate = DateTime.UtcNow;
                subscription.EndDate = subscription.EndDate?.AddDays(subscription.BillingCycle?.DurationInDays ?? 30);
                subscription.LastBillingDate = DateTime.UtcNow;
                subscription.NextBillingDate = await CalculateNextBillingDateAsync(subscription.Id);
                subscription.FailedPaymentAttempts = 0;
                subscription.LastPaymentError = null;
                await _subscriptionRepository.UpdateAsync(subscription);
                
                _logger.LogInformation("Successfully renewed subscription {SubscriptionId}", subscription.Id);
            }
            else
            {
                subscription.Status = Subscription.SubscriptionStatuses.PaymentFailed;
                subscription.FailedPaymentAttempts++;
                subscription.LastPaymentError = paymentResult.ErrorMessage;
                subscription.LastPaymentFailedDate = DateTime.UtcNow;
                await _subscriptionRepository.UpdateAsync(subscription);
                
                _logger.LogWarning("Renewal payment failed for subscription {SubscriptionId}: {Error}", subscription.Id, paymentResult.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing renewal for subscription {SubscriptionId}", subscription.Id);
            throw;
        }
    }

    private async Task RetryFailedPaymentAsync(Subscription subscription)
    {
        try
        {
            if (subscription.Status != Subscription.SubscriptionStatuses.PaymentFailed)
            {
                return;
            }
            
            // Retry payment
            var paymentResult = await ProcessPaymentAsync(subscription.Id, subscription.SubscriptionPlan?.Price ?? 0);
            
            if (paymentResult.Success)
            {
                subscription.Status = Subscription.SubscriptionStatuses.Active;
                subscription.LastBillingDate = DateTime.UtcNow;
                subscription.NextBillingDate = await CalculateNextBillingDateAsync(subscription.Id);
                subscription.FailedPaymentAttempts = 0;
                subscription.LastPaymentError = null;
                await _subscriptionRepository.UpdateAsync(subscription);
                
                _logger.LogInformation("Successfully retried payment for subscription {SubscriptionId}", subscription.Id);
            }
            else
            {
                // Increment retry count or suspend subscription after multiple failures
                subscription.FailedPaymentAttempts++;
                subscription.LastPaymentError = paymentResult.ErrorMessage;
                subscription.LastPaymentFailedDate = DateTime.UtcNow;
                
                // Suspend subscription after 3 failed attempts
                if (subscription.FailedPaymentAttempts >= 3)
                {
                    subscription.Status = Subscription.SubscriptionStatuses.Suspended;
                    subscription.SuspendedDate = DateTime.UtcNow;
                }
                
                await _subscriptionRepository.UpdateAsync(subscription);
                
                _logger.LogWarning("Payment retry failed for subscription {SubscriptionId}: {Error}", subscription.Id, paymentResult.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying payment for subscription {SubscriptionId}", subscription.Id);
            throw;
        }
    }
} 