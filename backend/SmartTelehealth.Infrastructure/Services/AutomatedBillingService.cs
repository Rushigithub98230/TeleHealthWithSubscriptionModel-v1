using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Infrastructure.Services;

public class AutomatedBillingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AutomatedBillingService> _logger;
    private readonly TimeSpan _billingInterval = TimeSpan.FromHours(1); // Run every hour
    private readonly int _maxRetryAttempts = 3;
    private readonly TimeSpan _retryDelay = TimeSpan.FromHours(6);

    public AutomatedBillingService(
        IServiceProvider serviceProvider,
        ILogger<AutomatedBillingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Automated billing service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBillingCycleAsync();
                await Task.Delay(_billingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Automated billing service stopped");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in automated billing cycle");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait before retry
            }
        }
    }

    private async Task ProcessBillingCycleAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var subscriptionRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();
        var billingService = scope.ServiceProvider.GetRequiredService<IBillingService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        try
        {
            _logger.LogInformation("Starting automated billing cycle at {Time}", DateTime.UtcNow);

            // 1. Process subscriptions due for billing
            await ProcessDueSubscriptionsAsync(subscriptionRepository, billingService, notificationService, auditService, userService);

            // 2. Process failed payment retries
            await ProcessFailedPaymentRetriesAsync(subscriptionRepository, billingService, notificationService, auditService, userService);

            // 3. Reset usage counters for new billing cycles
            await ResetUsageCountersAsync(subscriptionRepository, auditService);

            _logger.LogInformation("Completed automated billing cycle at {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in automated billing cycle");
            throw;
        }
    }

    private async Task ProcessDueSubscriptionsAsync(
        ISubscriptionRepository subscriptionRepository,
        IBillingService billingService,
        INotificationService notificationService,
        IAuditService auditService,
        IUserService userService)
    {
        try
        {
            var dueSubscriptions = await subscriptionRepository.GetSubscriptionsDueForBillingAsync(DateTime.UtcNow);
            _logger.LogInformation("Found {Count} subscriptions due for billing", dueSubscriptions.Count());

            foreach (var subscription in dueSubscriptions)
            {
                try
                {
                    await ProcessSubscriptionBillingAsync(subscription, subscriptionRepository, billingService, notificationService, auditService, userService);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing billing for subscription {SubscriptionId}", subscription.Id);
                    await auditService.LogPaymentEventAsync(
                        subscription.UserId.ToString(),
                        "BillingError",
                        subscription.Id.ToString(),
                        "Error",
                        ex.Message
                    );
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing due subscriptions");
            throw;
        }
    }

    private async Task ProcessSubscriptionBillingAsync(
        Subscription subscription,
        ISubscriptionRepository subscriptionRepository,
        IBillingService billingService,
        INotificationService notificationService,
        IAuditService auditService,
        IUserService userService)
    {
        try
        {
            // Create billing record
            var billingRecord = new CreateBillingRecordDto
            {
                UserId = subscription.UserId.ToString(),
                SubscriptionId = subscription.Id.ToString(),
                Amount = subscription.CurrentPrice,
                Description = $"Subscription billing for {subscription.SubscriptionPlan.Name}",
                DueDate = DateTime.UtcNow
            };

            var billingResult = await billingService.CreateBillingRecordAsync(billingRecord);
            if (!billingResult.Success)
            {
                _logger.LogError("Failed to create billing record for subscription {SubscriptionId}", subscription.Id);
                return;
            }

            // Process payment with retry logic
            var paymentResult = await ProcessPaymentWithRetryAsync(Guid.Parse(billingResult.Data.Id), billingService, auditService);

            if (paymentResult.Success)
            {
                // Update subscription billing date
                subscription.NextBillingDate = subscription.NextBillingDate.AddMonths(1);
                subscription.LastBillingDate = DateTime.UtcNow;
                subscription.FailedPaymentAttempts = 0;
                subscription.LastPaymentError = null;
                await subscriptionRepository.UpdateAsync(subscription);

                // Send success notification
                var userResult = await userService.GetUserByIdAsync(subscription.UserId);
                if (userResult.Success && userResult.Data != null)
                {
                    await notificationService.SendPaymentSuccessEmailAsync(
                        userResult.Data.Email,
                        userResult.Data.FullName,
                        billingResult.Data
                    );
                }

                await auditService.LogPaymentEventAsync(
                    subscription.UserId.ToString(),
                    "PaymentSuccess",
                    subscription.Id.ToString(),
                    "Success"
                );

                _logger.LogInformation("Successfully processed billing for subscription {SubscriptionId}", subscription.Id);
            }
            else
            {
                // Handle failed payment with immediate suspension
                await HandleFailedPaymentAsync(subscription, paymentResult.Message, subscriptionRepository, billingService, notificationService, auditService, userService);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing subscription billing for {SubscriptionId}", subscription.Id);
            throw;
        }
    }

    private async Task<ApiResponse<BillingRecordDto>> ProcessPaymentWithRetryAsync(
        Guid billingRecordId,
        IBillingService billingService,
        IAuditService auditService)
    {
        for (int attempt = 1; attempt <= _maxRetryAttempts; attempt++)
        {
            try
            {
                var paymentResult = await billingService.ProcessPaymentAsync(billingRecordId);
                
                if (paymentResult.Success)
                {
                    return paymentResult;
                }

                if (attempt < _maxRetryAttempts)
                {
                    _logger.LogWarning("Payment attempt {Attempt} failed for billing record {BillingRecordId}, retrying in {Delay} hours", 
                        attempt, billingRecordId, _retryDelay.TotalHours);
                    
                    await Task.Delay(_retryDelay);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in payment attempt {Attempt} for billing record {BillingRecordId}", attempt, billingRecordId);
                
                if (attempt == _maxRetryAttempts)
                {
                    return ApiResponse<BillingRecordDto>.ErrorResponse($"Payment failed after {_maxRetryAttempts} attempts: {ex.Message}");
                }
                
                await Task.Delay(_retryDelay);
            }
        }

        return ApiResponse<BillingRecordDto>.ErrorResponse($"Payment failed after {_maxRetryAttempts} attempts");
    }

    private async Task HandleFailedPaymentAsync(
        Subscription subscription,
        string errorMessage,
        ISubscriptionRepository subscriptionRepository,
        IBillingService billingService,
        INotificationService notificationService,
        IAuditService auditService,
        IUserService userService)
    {
        try
        {
            // Update subscription status to suspended immediately
            subscription.Status = Subscription.SubscriptionStatuses.Suspended;
            subscription.FailedPaymentAttempts += 1;
            subscription.LastPaymentError = errorMessage;
            subscription.LastPaymentFailedDate = DateTime.UtcNow;
            subscription.SuspendedDate = DateTime.UtcNow;
            await subscriptionRepository.UpdateAsync(subscription);

            // Send immediate suspension notification
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await notificationService.SendPaymentFailedEmailAsync(
            //     userResult.Data.Email,
            //     userResult.Data.FullName,
            //     billingRecord
            // );
            _logger.LogInformation("Email notifications disabled - would have sent payment failed notification to user {UserId}", subscription.UserId);

            // Send immediate suspension notification
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await notificationService.SendSubscriptionSuspendedNotificationAsync(
            //     subscription.UserId.ToString(),
            //     subscription.Id.ToString()
            // );
            _logger.LogInformation("Email notifications disabled - would have sent subscription suspended notification to user {UserId}", subscription.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling failed payment for subscription {SubscriptionId}", subscription.Id);
        }
    }

    private async Task ProcessFailedPaymentRetriesAsync(
        ISubscriptionRepository subscriptionRepository,
        IBillingService billingService,
        INotificationService notificationService,
        IAuditService auditService,
        IUserService userService)
    {
        try
        {
            var suspendedSubscriptions = await subscriptionRepository.GetSuspendedSubscriptionsAsync();
            _logger.LogInformation("Found {Count} suspended subscriptions", suspendedSubscriptions.Count());

            foreach (var subscription in suspendedSubscriptions)
            {
                try
                {
                    // Check if enough time has passed since suspension
                    if (subscription.SuspendedDate.HasValue &&
                        DateTime.UtcNow - subscription.SuspendedDate.Value < _retryDelay)
                    {
                        continue; // Skip if not enough time has passed
                    }

                    // Retry payment for suspended subscription
                    var billingRecord = new CreateBillingRecordDto
                    {
                        UserId = subscription.UserId.ToString(),
                        SubscriptionId = subscription.Id.ToString(),
                        Amount = subscription.CurrentPrice,
                        Description = $"Retry payment for {subscription.SubscriptionPlan.Name}",
                        DueDate = DateTime.UtcNow
                    };

                    var billingResult = await billingService.CreateBillingRecordAsync(billingRecord);
                    if (!billingResult.Success)
                    {
                        continue;
                    }

                    var paymentResult = await ProcessPaymentWithRetryAsync(Guid.Parse(billingResult.Data.Id), billingService, auditService);

                    if (paymentResult.Success)
                    {
                        // Reactivate subscription
                        subscription.Status = Subscription.SubscriptionStatuses.Active;
                        subscription.NextBillingDate = subscription.NextBillingDate.AddMonths(1);
                        subscription.LastBillingDate = DateTime.UtcNow;
                        subscription.FailedPaymentAttempts = 0;
                        subscription.LastPaymentError = null;
                        subscription.SuspendedDate = null;
                        await subscriptionRepository.UpdateAsync(subscription);

                        // Send reactivation notification
                        var userResult = await userService.GetUserByIdAsync(subscription.UserId);
                        if (userResult.Success && userResult.Data != null)
                        {
                            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                            // await notificationService.SendSubscriptionReactivatedNotificationAsync(
                            //     subscription.UserId.ToString(),
                            //     subscription.Id.ToString()
                            // );
                            _logger.LogInformation("Email notifications disabled - would have sent subscription reactivated notification to user {UserId}", subscription.UserId);
                        }

                        await auditService.LogPaymentEventAsync(
                            subscription.UserId.ToString(),
                            "PaymentRetrySuccess",
                            subscription.Id.ToString(),
                            "Success"
                        );

                        _logger.LogInformation("Successfully retried payment and reactivated subscription {SubscriptionId}", subscription.Id);
                    }
                    else
                    {
                        // Update failure count but keep suspended
                        subscription.FailedPaymentAttempts += 1;
                        subscription.LastPaymentError = paymentResult.Message;
                        subscription.LastPaymentFailedDate = DateTime.UtcNow;
                        await subscriptionRepository.UpdateAsync(subscription);

                        await auditService.LogPaymentEventAsync(
                            subscription.UserId.ToString(),
                            "PaymentRetryFailed",
                            subscription.Id.ToString(),
                            "Failed",
                            paymentResult.Message
                        );

                        _logger.LogWarning("Payment retry failed for suspended subscription {SubscriptionId}: {Error}", subscription.Id, paymentResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing payment retry for suspended subscription {SubscriptionId}", subscription.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing failed payment retries");
            throw;
        }
    }

    private async Task ResetUsageCountersAsync(ISubscriptionRepository subscriptionRepository, IAuditService auditService)
    {
        try
        {
            var subscriptionsWithResetUsage = await subscriptionRepository.GetSubscriptionsWithResetUsageAsync();
            _logger.LogInformation("Found {Count} subscriptions with usage counters to reset", subscriptionsWithResetUsage.Count());

            foreach (var subscription in subscriptionsWithResetUsage)
            {
                try
                {
                    // Reset usage counters for new billing cycle
                    await subscriptionRepository.ResetUsageCountersAsync();

                    await auditService.LogUserActionAsync(
                        subscription.UserId.ToString(),
                        "UsageReset",
                        "Subscription",
                        subscription.Id.ToString(),
                        "Usage counters reset for new billing cycle"
                    );

                    _logger.LogInformation("Reset usage counters for subscription {SubscriptionId}", subscription.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error resetting usage counters for subscription {SubscriptionId}", subscription.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting usage counters");
            throw;
        }
    }

    public async Task<ApiResponse<bool>> TriggerManualBillingCycleAsync()
    {
        try
        {
            _logger.LogInformation("Manual billing cycle triggered");
            await ProcessBillingCycleAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Manual billing cycle completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in manual billing cycle");
            return ApiResponse<bool>.ErrorResponse("Failed to complete manual billing cycle");
        }
    }

    public async Task<ApiResponse<BillingCycleReportDto>> GetBillingCycleReportAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var subscriptionRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();
            var billingService = scope.ServiceProvider.GetRequiredService<IBillingService>();

            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var report = new BillingCycleReportDto
            {
                StartDate = start,
                EndDate = end,
                TotalSubscriptionsProcessed = 0,
                SuccessfulPayments = 0,
                FailedPayments = 0,
                TotalRevenue = 0,
                SuspendedSubscriptions = 0,
                PaymentRetries = 0,
                UsageResets = 0
            };

            // Get billing statistics
            var billingHistory = await billingService.GetPaymentAnalyticsAsync(start, end);
            report.SuccessfulPayments = (int)billingHistory.Data.SuccessfulTransactions;
            report.FailedPayments = (int)billingHistory.Data.FailedTransactions;
            report.TotalRevenue = billingHistory.Data.TotalPayments;

            // Get subscription statistics
            var subscriptions = await subscriptionRepository.GetSubscriptionsInDateRangeAsync(start, end);
            report.TotalSubscriptionsProcessed = subscriptions.Count();

            // Get suspended subscription statistics
            var suspendedSubscriptions = await subscriptionRepository.GetSuspendedSubscriptionsAsync();
            report.SuspendedSubscriptions = suspendedSubscriptions.Count();

            var failedPayments = await subscriptionRepository.GetSubscriptionsWithFailedPaymentsAsync();
            report.PaymentRetries = failedPayments.Count(s => s.FailedPaymentAttempts > 0);

            return ApiResponse<BillingCycleReportDto>.SuccessResponse(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating billing cycle report");
            return ApiResponse<BillingCycleReportDto>.ErrorResponse("Failed to generate billing cycle report");
        }
    }
}

// DTOs for automated billing service
public class BillingCycleReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalSubscriptionsProcessed { get; set; }
    public int SuccessfulPayments { get; set; }
    public int FailedPayments { get; set; }
    public decimal TotalRevenue { get; set; }
    public int SuspendedSubscriptions { get; set; }
    public int PaymentRetries { get; set; }
    public int UsageResets { get; set; }
    public decimal SuccessRate => TotalSubscriptionsProcessed > 0 ? (decimal)SuccessfulPayments / TotalSubscriptionsProcessed * 100 : 0;
    public decimal FailureRate => TotalSubscriptionsProcessed > 0 ? (decimal)FailedPayments / TotalSubscriptionsProcessed * 100 : 0;
} 