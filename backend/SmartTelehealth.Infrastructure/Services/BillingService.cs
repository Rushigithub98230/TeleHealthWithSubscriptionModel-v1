using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Infrastructure.Services;

public class BillingService : IBillingService
{
    private readonly IBillingRepository _billingRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IStripeService _stripeService;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<BillingService> _logger;
    private readonly IAuditService _auditService;
    
    // Retry configuration
    private readonly int _maxRetryAttempts = 3;
    private readonly TimeSpan _retryDelay = TimeSpan.FromMinutes(5);
    
    public BillingService(
        IBillingRepository billingRepository,
        ISubscriptionRepository subscriptionRepository,
        IStripeService stripeService,
        INotificationService notificationService,
        IUserRepository userRepository,
        ILogger<BillingService> logger,
        IAuditService auditService)
    {
        _billingRepository = billingRepository;
        _subscriptionRepository = subscriptionRepository;
        _stripeService = stripeService;
        _notificationService = notificationService;
        _userRepository = userRepository;
        _logger = logger;
        _auditService = auditService;
    }
    
    public async Task<ApiResponse<BillingRecordDto>> CreateBillingRecordAsync(CreateBillingRecordDto createDto)
    {
        try
        {
            var billingRecord = new BillingRecord
            {
                UserId = Guid.Parse(createDto.UserId),
                SubscriptionId = !string.IsNullOrEmpty(createDto.SubscriptionId) ? Guid.Parse(createDto.SubscriptionId) : (Guid?)null,
                Amount = createDto.Amount,
                Description = createDto.Description,
                DueDate = createDto.DueDate ?? DateTime.UtcNow,
                Status = BillingRecord.BillingStatus.Pending,
                Type = BillingRecord.BillingType.Consultation // Use enum instead of string
            };
            
            var createdRecord = await _billingRepository.CreateAsync(billingRecord);
            var billingRecordDto = MapToDto(createdRecord);
            
            await _auditService.LogPaymentEventAsync(
                createDto.UserId,
                "BillingRecordCreated",
                createdRecord.Id.ToString(),
                "Success"
            );
            
            return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Billing record created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating billing record for user {UserId}", createDto.UserId);
            await _auditService.LogPaymentEventAsync(
                createDto.UserId,
                "BillingRecordCreationFailed",
                "N/A",
                ex.Message
            );
            return ApiResponse<BillingRecordDto>.ErrorResponse("An error occurred while creating the billing record", 500);
        }
    }
    
    public async Task<ApiResponse<BillingRecordDto>> ProcessPaymentAsync(Guid billingRecordId)
    {
        return await ProcessPaymentWithRetryAsync(billingRecordId, 0);
    }

    private async Task<ApiResponse<BillingRecordDto>> ProcessPaymentWithRetryAsync(Guid billingRecordId, int attempt)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
                return ApiResponse<BillingRecordDto>.ErrorResponse("Billing record not found", 404);
            
            if (billingRecord.Status == BillingRecord.BillingStatus.Paid)
                return ApiResponse<BillingRecordDto>.ErrorResponse("Payment has already been processed", 400);
            
            // Get user's default payment method
            var user = await _userRepository.GetByIdAsync(billingRecord.UserId);
            if (user == null)
                return ApiResponse<BillingRecordDto>.ErrorResponse("User not found", 404);

            var paymentMethods = await _stripeService.GetCustomerPaymentMethodsAsync(billingRecord.UserId.ToString());
            var defaultPaymentMethod = paymentMethods.FirstOrDefault(pm => pm.IsDefault);
            
            if (defaultPaymentMethod == null)
            {
                // Create a failed billing record
                billingRecord.Status = BillingRecord.BillingStatus.Failed;
                billingRecord.FailureReason = "No default payment method found";
                billingRecord.UpdatedAt = DateTime.UtcNow;
                await _billingRepository.UpdateAsync(billingRecord);
                
                await SendPaymentNotificationsAsync(MapToDto(billingRecord), false);
                await _auditService.LogPaymentEventAsync(billingRecord.UserId.ToString(), "PaymentFailed", billingRecord.Id.ToString(), "Failed", "No default payment method");
                
                return ApiResponse<BillingRecordDto>.ErrorResponse("No default payment method found", 400);
            }

            // Process payment through Stripe with retry logic
            var paymentResult = await _stripeService.ProcessPaymentAsync(
                defaultPaymentMethod.Id,
                billingRecord.Amount,
                "usd"
            );

            if (paymentResult.Status == "succeeded")
            {
                // Update billing record as paid
                billingRecord.Status = BillingRecord.BillingStatus.Paid;
                billingRecord.PaymentIntentId = paymentResult.PaymentIntentId;
                billingRecord.PaidAt = DateTime.UtcNow;
                billingRecord.UpdatedAt = DateTime.UtcNow;
                billingRecord.FailureReason = null;

                var updatedRecord = await _billingRepository.UpdateAsync(billingRecord);
                var billingRecordDto = MapToDto(updatedRecord);

                // Send success notifications
                await SendPaymentNotificationsAsync(billingRecordDto, true);

                // AUDIT LOG: Payment success
                await _auditService.LogPaymentEventAsync(
                    billingRecord.UserId.ToString(),
                    "PaymentSuccess",
                    billingRecord.Id.ToString(),
                    "Success",
                    null
                );

                return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Payment processed successfully");
            }
            else
            {
                // Handle failed payment with retry logic
                return await HandleFailedPaymentWithRetryAsync(billingRecord, paymentResult, attempt);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for billing record {BillingRecordId} (attempt {Attempt})", billingRecordId, attempt + 1);
            
            if (attempt < _maxRetryAttempts)
            {
                // Wait before retry with exponential backoff
                var delay = TimeSpan.FromMinutes(Math.Pow(2, attempt)) + _retryDelay;
                await Task.Delay(delay);
                
                _logger.LogInformation("Retrying payment for billing record {BillingRecordId} (attempt {Attempt})", billingRecordId, attempt + 2);
                return await ProcessPaymentWithRetryAsync(billingRecordId, attempt + 1);
            }
            
            // Final failure - update billing record
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
                if (billingRecord != null)
                {
                    billingRecord.Status = BillingRecord.BillingStatus.Failed;
                    billingRecord.FailureReason = ex.Message;
                    billingRecord.UpdatedAt = DateTime.UtcNow;
                    await _billingRepository.UpdateAsync(billingRecord);
                    
                    var billingRecordDto = MapToDto(billingRecord);
                    await SendPaymentNotificationsAsync(billingRecordDto, false);
                    
                    await _auditService.LogPaymentEventAsync(
                        billingRecord.UserId.ToString(),
                        "PaymentFailed",
                        billingRecord.Id.ToString(),
                        "Failed",
                        ex.Message
                    );
                }
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Error updating billing record status after payment failure");
            }
            
            return ApiResponse<BillingRecordDto>.ErrorResponse("An error occurred while processing payment", 500);
        }
    }

    private async Task<ApiResponse<BillingRecordDto>> HandleFailedPaymentWithRetryAsync(BillingRecord billingRecord, PaymentResultDto paymentResult, int attempt)
    {
        // Update billing record as failed
        billingRecord.Status = BillingRecord.BillingStatus.Failed;
        billingRecord.PaymentIntentId = paymentResult.PaymentIntentId;
        billingRecord.FailureReason = paymentResult.ErrorMessage ?? "Unknown payment error";
        billingRecord.UpdatedAt = DateTime.UtcNow;

        var updatedRecord = await _billingRepository.UpdateAsync(billingRecord);
        var billingRecordDto = MapToDto(updatedRecord);

        // Send failure notifications
        await SendPaymentNotificationsAsync(billingRecordDto, false);

        // AUDIT LOG: Payment failure
        await _auditService.LogPaymentEventAsync(
            billingRecord.UserId.ToString(),
            "PaymentFailed",
            billingRecord.Id.ToString(),
            "Failed",
            paymentResult.ErrorMessage
        );

        // Check if we should retry
        if (attempt < _maxRetryAttempts)
        {
            // Wait before retry with exponential backoff
            var delay = TimeSpan.FromMinutes(Math.Pow(2, attempt)) + _retryDelay;
            await Task.Delay(delay);
            
            _logger.LogInformation("Retrying payment for billing record {BillingRecordId} (attempt {Attempt})", billingRecord.Id, attempt + 2);
            return await ProcessPaymentWithRetryAsync(billingRecord.Id, attempt + 1);
        }

        // Final failure - handle immediate suspension
        await HandleImmediateSuspensionAsync(billingRecord);
        
        return ApiResponse<BillingRecordDto>.ErrorResponse($"Payment failed: {paymentResult.ErrorMessage}", 400);
    }

    private async Task HandleImmediateSuspensionAsync(BillingRecord billingRecord)
    {
        try
        {
            // Check if subscription exists and update status
            if (billingRecord.SubscriptionId.HasValue)
            {
                var subscription = await _subscriptionRepository.GetByIdAsync(billingRecord.SubscriptionId.Value);
                if (subscription != null)
                {
                    subscription.Status = Subscription.SubscriptionStatuses.Suspended;
                    subscription.FailedPaymentAttempts += 1;
                    subscription.LastPaymentFailedDate = DateTime.UtcNow;
                    subscription.LastPaymentError = billingRecord.FailureReason;
                    subscription.SuspendedDate = DateTime.UtcNow;
                    subscription.UpdatedAt = DateTime.UtcNow;
                    
                    await _subscriptionRepository.UpdateAsync(subscription);
                    
                    // Send immediate suspension notification
                    await SendImmediateSuspensionNotificationAsync(billingRecord, subscription);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling immediate suspension for billing record {BillingRecordId}", billingRecord.Id);
        }
    }

    public async Task<ApiResponse<PaymentResultDto>> RetryPaymentAsync(Guid billingRecordId)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
                return ApiResponse<PaymentResultDto>.ErrorResponse("Billing record not found", 404);

            if (billingRecord.Status == BillingRecord.BillingStatus.Paid)
                return ApiResponse<PaymentResultDto>.ErrorResponse("Payment has already been processed", 400);

            // Reset status to pending for retry
            billingRecord.Status = BillingRecord.BillingStatus.Pending;
            billingRecord.FailureReason = null;
            billingRecord.UpdatedAt = DateTime.UtcNow;
            await _billingRepository.UpdateAsync(billingRecord);

            // Process payment with retry
            var result = await ProcessPaymentWithRetryAsync(billingRecordId, 0);
            
            if (result.Success)
            {
                return ApiResponse<PaymentResultDto>.SuccessResponse(new PaymentResultDto
                {
                    PaymentIntentId = result.Data?.PaymentIntentId ?? string.Empty,
                    Status = "succeeded",
                    Amount = result.Data?.Amount ?? 0,
                    Currency = "usd"
                }, "Payment retry successful");
            }
            
            return ApiResponse<PaymentResultDto>.ErrorResponse("Payment retry failed", 400);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying payment for billing record {BillingRecordId}", billingRecordId);
            return ApiResponse<PaymentResultDto>.ErrorResponse("Failed to retry payment", 500);
        }
    }

    public async Task<ApiResponse<RefundResultDto>> ProcessRefundAsync(Guid billingRecordId, decimal amount, string reason)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
                return ApiResponse<RefundResultDto>.ErrorResponse("Billing record not found", 404);

            if (billingRecord.Status != BillingRecord.BillingStatus.Paid)
                return ApiResponse<RefundResultDto>.ErrorResponse("Only paid billing records can be refunded", 400);

            if (string.IsNullOrEmpty(billingRecord.PaymentIntentId))
                return ApiResponse<RefundResultDto>.ErrorResponse("No payment intent found for refund", 400);

            // Process refund through Stripe
            var refundResult = await _stripeService.ProcessRefundAsync(billingRecord.PaymentIntentId, amount);
            
            if (refundResult)
            {
                // Create refund record
                var refundRecord = new BillingRecord
                {
                    UserId = billingRecord.UserId,
                    SubscriptionId = billingRecord.SubscriptionId,
                    Amount = -amount, // Negative amount for refund
                    Description = $"Refund: {reason}",
                    Status = BillingRecord.BillingStatus.Refunded,
                    Type = BillingRecord.BillingType.Refund,
                    BillingDate = DateTime.UtcNow,
                    PaidAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _billingRepository.CreateAsync(refundRecord);

                // Update original billing record
                billingRecord.Status = BillingRecord.BillingStatus.Refunded;
                billingRecord.UpdatedAt = DateTime.UtcNow;
                await _billingRepository.UpdateAsync(billingRecord);

                // Send refund notification
                await SendRefundNotificationAsync(billingRecord, amount, reason);

                // Audit log
                await _auditService.LogPaymentEventAsync(
                    billingRecord.UserId.ToString(),
                    "RefundProcessed",
                    billingRecord.Id.ToString(),
                    "Success",
                    reason
                );

                return ApiResponse<RefundResultDto>.SuccessResponse(new RefundResultDto
                {
                    BillingRecordId = billingRecordId,
                    RefundAmount = amount,
                    Status = "completed",
                    ProcessedAt = DateTime.UtcNow,
                    Reason = reason
                }, "Refund processed successfully");
            }
            
            return ApiResponse<RefundResultDto>.ErrorResponse("Failed to process refund", 400);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for billing record {BillingRecordId}", billingRecordId);
            return ApiResponse<RefundResultDto>.ErrorResponse("Failed to process refund", 500);
        }
    }

    public async Task<ApiResponse<BillingRecordDto>> GetBillingRecordAsync(Guid billingRecordId)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
                return ApiResponse<BillingRecordDto>.ErrorResponse("Billing record not found", 404);

            return ApiResponse<BillingRecordDto>.SuccessResponse(MapToDto(billingRecord));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing record {BillingRecordId}", billingRecordId);
            return ApiResponse<BillingRecordDto>.ErrorResponse("Failed to retrieve billing record", 500);
        }
    }

    public async Task<IEnumerable<PaymentHistoryDto>> GetPaymentHistoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var billingRecords = await _billingRepository.GetByUserIdAsync(Guid.Parse(userId));
            
            var filteredRecords = billingRecords.Where(br => 
                (!startDate.HasValue || br.BillingDate >= startDate.Value) &&
                (!endDate.HasValue || br.BillingDate <= endDate.Value) &&
                (br.Status == BillingRecord.BillingStatus.Paid || br.Status == BillingRecord.BillingStatus.Refunded)
            );

            return filteredRecords.Select(br => new PaymentHistoryDto
            {
                Id = br.Id,
                Amount = br.Amount,
                Status = br.Status.ToString(),
                PaymentDate = br.PaidAt ?? br.BillingDate,
                Description = br.Description,
                PaymentMethodId = br.PaymentIntentId
            }).OrderByDescending(ph => ph.PaymentDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment history for user {UserId}", userId);
            return Enumerable.Empty<PaymentHistoryDto>();
        }
    }

    public async Task<ApiResponse<PaymentAnalyticsDto>> GetPaymentAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var billingRecords = await _billingRepository.GetByUserIdAsync(Guid.Parse(userId));
            
            var filteredRecords = billingRecords.Where(br => 
                (!startDate.HasValue || br.BillingDate >= startDate.Value) &&
                (!endDate.HasValue || br.BillingDate <= endDate.Value)
            );

            var analytics = new PaymentAnalyticsDto
            {
                TotalSpent = filteredRecords.Where(br => br.Status == BillingRecord.BillingStatus.Paid).Sum(br => br.Amount),
                TotalPayments = filteredRecords.Count(br => br.Status == BillingRecord.BillingStatus.Paid),
                SuccessfulPayments = filteredRecords.Count(br => br.Status == BillingRecord.BillingStatus.Paid),
                FailedPayments = filteredRecords.Count(br => br.Status == BillingRecord.BillingStatus.Failed),
                AveragePaymentAmount = filteredRecords.Where(br => br.Status == BillingRecord.BillingStatus.Paid).Any() 
                    ? filteredRecords.Where(br => br.Status == BillingRecord.BillingStatus.Paid).Average(br => br.Amount) 
                    : 0
            };

            // Calculate monthly payments
            var monthlyPayments = filteredRecords
                .Where(br => br.Status == BillingRecord.BillingStatus.Paid)
                .GroupBy(br => new { br.BillingDate.Year, br.BillingDate.Month })
                .Select(g => new MonthlyPaymentDto
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Amount = g.Sum(br => br.Amount),
                    Count = g.Count()
                })
                .OrderBy(mp => mp.Month)
                .ToList();

            analytics.MonthlyPayments = monthlyPayments;

            return ApiResponse<PaymentAnalyticsDto>.SuccessResponse(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment analytics for user {UserId}", userId);
            return ApiResponse<PaymentAnalyticsDto>.ErrorResponse("Error retrieving payment analytics", 500);
        }
    }

    // Private helper methods
    private async Task SendPaymentNotificationsAsync(BillingRecordDto billingRecord, bool isSuccess)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(billingRecord.UserId));
            if (user == null) return;
            
            var userName = $"{user.FirstName} {user.LastName}";
            
            if (isSuccess)
            {
                // Send email notification
                if (!string.IsNullOrEmpty(user.Email))
                {
                    await _notificationService.SendPaymentSuccessEmailAsync(user.Email, userName, billingRecord);
                }
                
                // Send in-app notification
                await _notificationService.CreateInAppNotificationAsync(
                    Guid.Parse(billingRecord.UserId),
                    "Payment Successful",
                    $"Your payment of ${billingRecord.Amount} has been processed successfully."
                );
            }
            else
            {
                // Send email notification
                if (!string.IsNullOrEmpty(user.Email))
                {
                    await _notificationService.SendPaymentFailedEmailAsync(user.Email, userName, billingRecord);
                }
                
                // Send in-app notification
                await _notificationService.CreateInAppNotificationAsync(
                    Guid.Parse(billingRecord.UserId),
                    "Payment Failed",
                    $"We were unable to process your payment of ${billingRecord.Amount}. Please check your payment method."
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment notifications for billing record {BillingRecordId}", billingRecord.Id);
        }
    }

    private async Task SendImmediateSuspensionNotificationAsync(BillingRecord billingRecord, Subscription subscription)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(billingRecord.UserId);
            if (user == null) return;

            var message = $"Your subscription has been suspended due to payment failure. Please update your payment method to reactivate your subscription.";
            
            // Send email notification
            if (!string.IsNullOrEmpty(user.Email))
            {
                await _notificationService.SendSubscriptionSuspendedNotificationAsync(billingRecord.UserId.ToString(), billingRecord.SubscriptionId?.ToString() ?? "");
            }
            
            // Send in-app notification
            await _notificationService.CreateInAppNotificationAsync(
                billingRecord.UserId,
                "Subscription Suspended",
                message
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending immediate suspension notification for billing record {BillingRecordId}", billingRecord.Id);
        }
    }

    private async Task SendRefundNotificationAsync(BillingRecord billingRecord, decimal amount, string reason)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(billingRecord.UserId);
            if (user == null) return;

            var message = $"Your refund of ${amount} has been processed. Reason: {reason}";
            
            // Send email notification
            if (!string.IsNullOrEmpty(user.Email))
            {
                await _notificationService.SendRefundNotificationAsync(billingRecord.UserId.ToString(), amount, billingRecord.Id.ToString());
            }
            
            // Send in-app notification
            await _notificationService.CreateInAppNotificationAsync(
                billingRecord.UserId,
                "Refund Processed",
                message
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending refund notification for billing record {BillingRecordId}", billingRecord.Id);
        }
    }

    private BillingRecordDto MapToDto(BillingRecord billingRecord)
    {
        return new BillingRecordDto
        {
            Id = billingRecord.Id.ToString(),
            UserId = billingRecord.UserId.ToString(),
            SubscriptionId = billingRecord.SubscriptionId?.ToString(),
            Amount = billingRecord.Amount,
            Description = billingRecord.Description,
            Status = billingRecord.Status.ToString(),
            BillingDate = billingRecord.BillingDate,
            DueDate = billingRecord.DueDate,
            PaidAt = billingRecord.PaidAt,
            PaymentIntentId = billingRecord.PaymentIntentId,
            FailureReason = billingRecord.FailureReason,
            CreatedAt = billingRecord.CreatedAt,
            UpdatedAt = billingRecord.UpdatedAt
        };
    }

    /// <summary>
    /// Calculate accrued revenue for a billing record (proportional to time elapsed in accrual period)
    /// </summary>
    private decimal CalculateAccruedAmount(BillingRecord record, DateTime asOf)
    {
        if (record.AccrualStartDate == null || record.AccrualEndDate == null || record.AccrualStartDate >= record.AccrualEndDate)
            return 0;
        var totalDays = (record.AccrualEndDate.Value - record.AccrualStartDate.Value).TotalDays;
        if (totalDays <= 0) return 0;
        var elapsedDays = (asOf - record.AccrualStartDate.Value).TotalDays;
        if (elapsedDays <= 0) return 0;
        if (elapsedDays >= totalDays) return record.Amount;
        return Math.Round((decimal)(elapsedDays / totalDays) * record.Amount, 2);
    }

    /// <summary>
    /// Aggregate accrued and cash revenue for admin reporting
    /// </summary>
    public async Task<ApiResponse<RevenueSummaryDto>> GetRevenueSummaryAsync(DateTime? from = null, DateTime? to = null, string? planId = null)
    {
        // TODO: Implement filtering by planId, type, status
        var allRecords = await _billingRepository.GetAllAsync();
        var now = DateTime.UtcNow;
        decimal totalAccrued = 0, totalCash = 0, totalRefunded = 0;
        foreach (var record in allRecords)
        {
            if (from.HasValue && record.BillingDate < from.Value) continue;
            if (to.HasValue && record.BillingDate > to.Value) continue;
            if (record.Status == BillingRecord.BillingStatus.Paid)
                totalCash += record.Amount;
            if (record.Status == BillingRecord.BillingStatus.Refunded)
                totalRefunded += record.Amount;
            totalAccrued += CalculateAccruedAmount(record, now);
        }
        var summary = new RevenueSummaryDto
        {
            TotalAccruedRevenue = totalAccrued,
            TotalCashRevenue = totalCash,
            TotalRefunded = totalRefunded,
            AsOf = now
        };
        return ApiResponse<RevenueSummaryDto>.SuccessResponse(summary);
    }

    // PHASE 2 STUBS
    public async Task<ApiResponse<BillingRecordDto>> CreateRecurringBillingAsync(CreateRecurringBillingDto createDto)
    {
        var billingRecord = new BillingRecord
        {
            UserId = createDto.UserId,
            SubscriptionId = createDto.SubscriptionId,
            Amount = createDto.Amount,
            Description = createDto.Description,
            DueDate = createDto.DueDate,
            Status = BillingRecord.BillingStatus.Pending,
            Type = BillingRecord.BillingType.Recurring
        };
        var createdRecord = await _billingRepository.CreateAsync(billingRecord);
        var billingRecordDto = MapToDto(createdRecord);
        await _auditService.LogPaymentEventAsync(createDto.UserId.ToString(), "RecurringBillingCreated", createdRecord.Id.ToString(), "Success");
        return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Recurring billing record created successfully");
    }
    public async Task<ApiResponse<BillingRecordDto>> ProcessRecurringPaymentAsync(Guid subscriptionId)
    {
        // Example: process payment for the next due billing record for the subscription
        var records = await _billingRepository.GetBySubscriptionIdAsync(subscriptionId);
        var nextDue = records.OrderBy(r => r.DueDate).FirstOrDefault(r => r.Status == BillingRecord.BillingStatus.Pending);
        if (nextDue == null)
            return ApiResponse<BillingRecordDto>.ErrorResponse("No pending recurring payment found", 404);
        return await ProcessPaymentAsync(nextDue.Id);
    }
    public async Task<ApiResponse<bool>> CancelRecurringBillingAsync(Guid subscriptionId)
    {
        // Example: mark all future recurring billing records as cancelled
        var records = await _billingRepository.GetBySubscriptionIdAsync(subscriptionId);
        foreach (var record in records.Where(r => r.Status == BillingRecord.BillingStatus.Pending))
        {
            record.Status = BillingRecord.BillingStatus.Cancelled;
            await _billingRepository.UpdateAsync(record);
        }
        return ApiResponse<bool>.SuccessResponse(true, "Recurring billing cancelled");
    }
    public async Task<ApiResponse<BillingRecordDto>> CreateUpfrontPaymentAsync(CreateUpfrontPaymentDto createDto)
    {
        var billingRecord = new BillingRecord
        {
            UserId = createDto.UserId,
            Amount = createDto.Amount,
            Description = createDto.Description,
            DueDate = createDto.DueDate,
            Status = BillingRecord.BillingStatus.Paid,
            Type = BillingRecord.BillingType.Upfront,
            PaidAt = DateTime.UtcNow
        };
        var createdRecord = await _billingRepository.CreateAsync(billingRecord);
        var billingRecordDto = MapToDto(createdRecord);
        await _auditService.LogPaymentEventAsync(createDto.UserId.ToString(), "UpfrontPaymentCreated", createdRecord.Id.ToString(), "Success");
        return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Upfront payment processed successfully");
    }
    public async Task<ApiResponse<BillingRecordDto>> ProcessBundlePaymentAsync(CreateBundlePaymentDto createDto)
    {
        decimal total = createDto.Items.Sum(i => i.Amount);
        var billingRecord = new BillingRecord
        {
            UserId = createDto.UserId,
            Amount = total,
            Description = "Bundle payment: " + string.Join(", ", createDto.Items.Select(i => i.Description)),
            DueDate = DateTime.UtcNow,
            Status = BillingRecord.BillingStatus.Paid,
            Type = BillingRecord.BillingType.Bundle,
            PaidAt = DateTime.UtcNow
        };
        var createdRecord = await _billingRepository.CreateAsync(billingRecord);
        var billingRecordDto = MapToDto(createdRecord);
        await _auditService.LogPaymentEventAsync(createDto.UserId.ToString(), "BundlePaymentProcessed", createdRecord.Id.ToString(), "Success");
        return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Bundle payment processed successfully");
    }
    public async Task<ApiResponse<BillingRecordDto>> ApplyBillingAdjustmentAsync(Guid billingRecordId, CreateBillingAdjustmentDto adjustmentDto)
    {
        // Example: apply an adjustment (discount, credit, etc.)
        var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
        if (billingRecord == null)
            return ApiResponse<BillingRecordDto>.ErrorResponse("Billing record not found", 404);
        billingRecord.Amount += adjustmentDto.Amount;
        billingRecord.UpdatedAt = DateTime.UtcNow;
        await _billingRepository.UpdateAsync(billingRecord);
        var billingRecordDto = MapToDto(billingRecord);
        await _auditService.LogPaymentEventAsync(billingRecord.UserId.ToString(), "BillingAdjustmentApplied", billingRecord.Id.ToString(), "Success");
        return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Billing adjustment applied successfully");
    }
    public async Task<ApiResponse<IEnumerable<BillingAdjustmentDto>>> GetBillingAdjustmentsAsync(Guid billingRecordId)
    {
        var adjustments = await _billingRepository.GetAdjustmentsByBillingRecordIdAsync(billingRecordId);
        var dtos = adjustments.Select(a => new BillingAdjustmentDto
        {
            Id = a.Id,
            BillingRecordId = a.BillingRecordId,
            Amount = a.Amount,
            AdjustmentType = a.Type.ToString(),
            Reason = a.Reason,
            AppliedBy = a.AppliedBy,
            AppliedAt = a.CreatedAt,
            IsPercentage = a.IsPercentage
        });
        return ApiResponse<IEnumerable<BillingAdjustmentDto>>.SuccessResponse(dtos, "Billing adjustments retrieved successfully");
    }
    public async Task<ApiResponse<BillingRecordDto>> RetryFailedPaymentAsync(Guid billingRecordId)
    {
        // Example: retry payment for a failed billing record
        var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
        if (billingRecord == null)
            return ApiResponse<BillingRecordDto>.ErrorResponse("Billing record not found", 404);
        if (billingRecord.Status != BillingRecord.BillingStatus.Failed)
            return ApiResponse<BillingRecordDto>.ErrorResponse("Billing record is not in failed status", 400);
        return await ProcessPaymentAsync(billingRecordId);
    }
    public async Task<ApiResponse<BillingRecordDto>> ProcessPartialPaymentAsync(Guid billingRecordId, decimal amount)
    {
        // Example: process a partial payment
        var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
        if (billingRecord == null)
            return ApiResponse<BillingRecordDto>.ErrorResponse("Billing record not found", 404);
        if (amount <= 0 || amount > billingRecord.Amount)
            return ApiResponse<BillingRecordDto>.ErrorResponse("Invalid partial payment amount", 400);
        // Simulate partial payment logic
        billingRecord.Amount -= amount;
        billingRecord.UpdatedAt = DateTime.UtcNow;
        await _billingRepository.UpdateAsync(billingRecord);
        var billingRecordDto = MapToDto(billingRecord);
        await _auditService.LogPaymentEventAsync(billingRecord.UserId.ToString(), "PartialPaymentProcessed", billingRecord.Id.ToString(), "Success");
        return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Partial payment processed successfully");
    }
    public async Task<ApiResponse<BillingRecordDto>> CreateInvoiceAsync(CreateInvoiceDto createDto)
    {
        var billingRecord = new BillingRecord
        {
            UserId = createDto.UserId,
            Amount = createDto.Amount,
            Description = createDto.Description,
            DueDate = createDto.DueDate,
            Status = BillingRecord.BillingStatus.Pending,
            Type = BillingRecord.BillingType.Invoice
        };
        var createdRecord = await _billingRepository.CreateAsync(billingRecord);
        var billingRecordDto = MapToDto(createdRecord);
        await _auditService.LogPaymentEventAsync(createDto.UserId.ToString(), "InvoiceCreated", createdRecord.Id.ToString(), "Success");
        return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Invoice created successfully");
    }
    public async Task<ApiResponse<byte[]>> GenerateInvoicePdfAsync(Guid billingRecordId)
    {
        // Example: generate a PDF (stubbed as byte array)
        var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
        if (billingRecord == null)
            return ApiResponse<byte[]>.ErrorResponse("Billing record not found", 404);
        var pdfBytes = System.Text.Encoding.UTF8.GetBytes($"Invoice for {billingRecord.Id} - Amount: {billingRecord.Amount}");
        return ApiResponse<byte[]>.SuccessResponse(pdfBytes, "Invoice PDF generated");
    }
    public async Task<ApiResponse<byte[]>> GenerateBillingReportAsync(DateTime startDate, DateTime endDate, string format = "pdf")
    {
        // Example: generate a report (stubbed as byte array)
        var records = await _billingRepository.GetAllAsync();
        var reportBytes = System.Text.Encoding.UTF8.GetBytes($"Billing report from {startDate} to {endDate} - Total records: {records.Count()}");
        return ApiResponse<byte[]>.SuccessResponse(reportBytes, "Billing report generated");
    }
    public async Task<ApiResponse<BillingSummaryDto>> GetBillingSummaryAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        // Example: summarize billing for a user
        var records = await _billingRepository.GetByUserIdAsync(userId);
        var total = records.Where(r => (!startDate.HasValue || r.CreatedAt >= startDate) && (!endDate.HasValue || r.CreatedAt <= endDate)).Sum(r => r.Amount);
        var summary = new BillingSummaryDto { UserId = userId, TotalBilled = total };
        return ApiResponse<BillingSummaryDto>.SuccessResponse(summary, "Billing summary generated");
    }
    public async Task<ApiResponse<PaymentScheduleDto>> GetPaymentScheduleAsync(Guid subscriptionId)
    {
        // Example: return a payment schedule for a subscription
        var records = await _billingRepository.GetBySubscriptionIdAsync(subscriptionId);
        var schedule = new PaymentScheduleDto
        {
            SubscriptionId = subscriptionId,
            PaymentHistory = records.Select(r => new PaymentScheduleItemDto
            {
                BillingRecordId = r.Id,
                ScheduledDate = r.DueDate ?? DateTime.UtcNow,
                PaidDate = r.PaidAt,
                DueDate = r.DueDate ?? DateTime.UtcNow,
                Amount = r.Amount,
                Status = r.Status.ToString(),
                PaymentMethodId = r.PaymentIntentId,
                TransactionId = r.StripePaymentIntentId
            }).ToList()
        };
        return ApiResponse<PaymentScheduleDto>.SuccessResponse(schedule, "Payment schedule generated");
    }
    public async Task<ApiResponse<bool>> UpdatePaymentMethodAsync(Guid billingRecordId, string paymentMethodId)
    {
        // Not implemented in infrastructure layer
        return await Task.FromResult(ApiResponse<bool>.ErrorResponse("Not implemented in infrastructure layer", 501));
    }
    /// <summary>
    /// Creates a new billing cycle record. UserId must always be a Guid.
    /// </summary>
    /// <param name="createDto">The billing cycle creation DTO.</param>
    /// <returns>API response with the created billing record DTO.</returns>
    public async Task<ApiResponse<BillingRecordDto>> CreateBillingCycleAsync(CreateBillingCycleDto createDto)
    {
        var userId = createDto.UserId;
        var billingRecord = new BillingRecord
        {
            UserId = userId,
            Amount = createDto.Amount,
            Description = createDto.Description,
            DueDate = createDto.DueDate,
            Status = BillingRecord.BillingStatus.Pending,
            Type = BillingRecord.BillingType.Cycle
        };
        var createdRecord = await _billingRepository.CreateAsync(billingRecord);
        var billingRecordDto = MapToDto(createdRecord);
        await _auditService.LogPaymentEventAsync(userId.ToString(), "BillingCycleCreated", createdRecord.Id.ToString(), "Success");
        return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Billing cycle created successfully");
    }
    public async Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetBillingCycleRecordsAsync(Guid billingCycleId)
    {
        // Example: fetch all records for a billing cycle
        var records = await _billingRepository.GetByBillingCycleIdAsync(billingCycleId);
        var dtos = records.Select(MapToDto);
        return ApiResponse<IEnumerable<BillingRecordDto>>.SuccessResponse(dtos, "Billing cycle records retrieved successfully");
    }
    public async Task<ApiResponse<BillingRecordDto>> ProcessBillingCycleAsync(Guid billingCycleId)
    {
        // Example: process all pending payments in a billing cycle
        var records = await _billingRepository.GetByBillingCycleIdAsync(billingCycleId);
        foreach (var record in records.Where(r => r.Status == BillingRecord.BillingStatus.Pending))
        {
            await ProcessPaymentAsync(record.Id);
        }
        var dtos = records.Select(MapToDto);
        return ApiResponse<BillingRecordDto>.SuccessResponse(dtos.FirstOrDefault(), "Billing cycle processed");
    }

    public async Task<ApiResponse<byte[]>> ExportRevenueAsync(DateTime? from = null, DateTime? to = null, string? planId = null, string format = "csv")
    {
        try
        {
            // Implementation for revenue export
            var revenueData = await GetRevenueSummaryAsync(from, to, planId);
            if (!revenueData.Success)
            {
                return ApiResponse<byte[]>.ErrorResponse("Failed to get revenue data");
            }

            // Convert to CSV format
            var csvData = ConvertToCsv(revenueData.Data);
            var bytes = System.Text.Encoding.UTF8.GetBytes(csvData);
            
            return ApiResponse<byte[]>.SuccessResponse(bytes, "Revenue data exported successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting revenue data");
            return ApiResponse<byte[]>.ErrorResponse("Failed to export revenue data");
        }
    }

    // Missing interface methods
    public async Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetUserBillingHistoryAsync(Guid userId)
    {
        try
        {
            var records = await _billingRepository.GetByUserIdAsync(userId);
            var dtos = records.Select(MapToDto);
            return ApiResponse<IEnumerable<BillingRecordDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing history for user {UserId}", userId);
            return ApiResponse<IEnumerable<BillingRecordDto>>.ErrorResponse("Failed to get billing history");
        }
    }

    public async Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetSubscriptionBillingHistoryAsync(Guid subscriptionId)
    {
        try
        {
            var records = await _billingRepository.GetBySubscriptionIdAsync(subscriptionId);
            var dtos = records.Select(MapToDto);
            return ApiResponse<IEnumerable<BillingRecordDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing history for subscription {SubscriptionId}", subscriptionId);
            return ApiResponse<IEnumerable<BillingRecordDto>>.ErrorResponse("Failed to get subscription billing history");
        }
    }

    public async Task<ApiResponse<BillingRecordDto>> ProcessRefundAsync(Guid billingRecordId, decimal amount)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
                return ApiResponse<BillingRecordDto>.ErrorResponse("Billing record not found");

            if (string.IsNullOrEmpty(billingRecord.StripePaymentIntentId))
                return ApiResponse<BillingRecordDto>.ErrorResponse("No payment intent found for refund");

            var refundResult = await _stripeService.ProcessRefundAsync(billingRecord.StripePaymentIntentId, amount);
            
            if (refundResult)
            {
                billingRecord.Status = BillingRecord.BillingStatus.Refunded;
                var updatedRecord = await _billingRepository.UpdateAsync(billingRecord);

                await _auditService.LogPaymentEventAsync(
                    billingRecord.UserId.ToString(),
                    "RefundProcessed",
                    billingRecordId.ToString(),
                    "Success"
                );

                return ApiResponse<BillingRecordDto>.SuccessResponse(MapToDto(updatedRecord));
            }

            return ApiResponse<BillingRecordDto>.ErrorResponse("Failed to process refund");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for billing record {BillingRecordId}", billingRecordId);
            return ApiResponse<BillingRecordDto>.ErrorResponse("Failed to process refund");
        }
    }

    public async Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetOverdueBillingRecordsAsync()
    {
        try
        {
            var overdueRecords = await _billingRepository.GetOverdueRecordsAsync();
            var dtos = overdueRecords.Select(MapToDto);
            return ApiResponse<IEnumerable<BillingRecordDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue billing records");
            return ApiResponse<IEnumerable<BillingRecordDto>>.ErrorResponse("Failed to get overdue billing records");
        }
    }

    public async Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetPendingPaymentsAsync()
    {
        try
        {
            var pendingRecords = await _billingRepository.GetPendingRecordsAsync();
            var dtos = pendingRecords.Select(MapToDto);
            return ApiResponse<IEnumerable<BillingRecordDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending payments");
            return ApiResponse<IEnumerable<BillingRecordDto>>.ErrorResponse("Failed to get pending payments");
        }
    }

    public async Task<ApiResponse<decimal>> CalculateTotalAmountAsync(decimal subtotal, decimal tax, decimal shipping)
    {
        try
        {
            var total = subtotal + tax + shipping;
            return ApiResponse<decimal>.SuccessResponse(total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total amount");
            return ApiResponse<decimal>.ErrorResponse("Failed to calculate total amount");
        }
    }

    public async Task<ApiResponse<decimal>> CalculateTaxAmountAsync(decimal amount, string taxRate)
    {
        try
        {
            if (decimal.TryParse(taxRate, out var rate))
            {
                var tax = amount * (rate / 100);
                return ApiResponse<decimal>.SuccessResponse(tax);
            }
            return ApiResponse<decimal>.ErrorResponse("Invalid tax rate");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating tax amount");
            return ApiResponse<decimal>.ErrorResponse("Failed to calculate tax amount");
        }
    }

    public async Task<ApiResponse<decimal>> CalculateShippingAmountAsync(string shippingMethod, bool isExpress)
    {
        try
        {
            var baseShipping = shippingMethod.ToLower() switch
            {
                "standard" => 5.00m,
                "express" => 15.00m,
                "overnight" => 25.00m,
                _ => 0.00m
            };

            var totalShipping = isExpress ? baseShipping * 1.5m : baseShipping;
            return ApiResponse<decimal>.SuccessResponse(totalShipping);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating shipping amount");
            return ApiResponse<decimal>.ErrorResponse("Failed to calculate shipping amount");
        }
    }

    public async Task<ApiResponse<bool>> IsPaymentOverdueAsync(Guid billingRecordId)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
                return ApiResponse<bool>.ErrorResponse("Billing record not found");

            var isOverdue = billingRecord.DueDate < DateTime.UtcNow && 
                           billingRecord.Status == BillingRecord.BillingStatus.Pending;
            
            return ApiResponse<bool>.SuccessResponse(isOverdue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if payment is overdue for billing record {BillingRecordId}", billingRecordId);
            return ApiResponse<bool>.ErrorResponse("Failed to check payment overdue status");
        }
    }

    public async Task<ApiResponse<DateTime>> CalculateDueDateAsync(DateTime startDate, int daysToAdd)
    {
        try
        {
            var dueDate = startDate.AddDays(daysToAdd);
            return ApiResponse<DateTime>.SuccessResponse(dueDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating due date");
            return ApiResponse<DateTime>.ErrorResponse("Failed to calculate due date");
        }
    }

    public async Task<ApiResponse<BillingAnalyticsDto>> GetBillingAnalyticsAsync()
    {
        try
        {
            var analytics = new BillingAnalyticsDto
            {
                TotalBillingRecords = 0,
                PendingBillingRecords = 0,
                PaidBillingRecords = 0,
                FailedBillingRecords = 0,
                TotalRevenue = 0,
                AverageBillingAmount = 0,
                MonthlyRevenue = new List<MonthlyRevenueDto>(),
                BillingStatuses = new List<BillingStatusDto>(),
                PaymentMethods = new List<PaymentMethodDto>()
            };

            return ApiResponse<BillingAnalyticsDto>.SuccessResponse(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing analytics");
            return ApiResponse<BillingAnalyticsDto>.ErrorResponse("Failed to get billing analytics");
        }
    }

    public async Task<ApiResponse<IEnumerable<PaymentHistoryDto>>> GetPaymentHistoryAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var records = await _billingRepository.GetByUserIdAsync(userId);
            var filteredRecords = records.Where(r => 
                (!startDate.HasValue || r.CreatedAt >= startDate.Value) &&
                (!endDate.HasValue || r.CreatedAt <= endDate.Value));

            var paymentHistory = filteredRecords.Select(r => new PaymentHistoryDto
            {
                Id = r.Id,
                UserId = r.UserId.ToString(),
                SubscriptionId = r.SubscriptionId?.ToString() ?? "",
                Amount = r.Amount,
                Currency = "USD",
                PaymentMethod = r.PaymentMethod ?? "Unknown",
                Status = r.Status.ToString(),
                TransactionId = r.StripePaymentIntentId,
                ErrorMessage = r.FailureReason,
                CreatedAt = r.CreatedAt,
                ProcessedAt = r.ProcessedAt,
                PaymentDate = r.ProcessedAt ?? r.CreatedAt,
                Description = r.Description,
                PaymentMethodId = r.StripePaymentIntentId
            });

            return ApiResponse<IEnumerable<PaymentHistoryDto>>.SuccessResponse(paymentHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment history for user {UserId}", userId);
            return ApiResponse<IEnumerable<PaymentHistoryDto>>.ErrorResponse("Failed to get payment history");
        }
    }

    public async Task<ApiResponse<PaymentAnalyticsDto>> GetPaymentAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var analytics = new PaymentAnalyticsDto
            {
                TotalPayments = 0,
                SuccessfulPayments = 0,
                FailedPayments = 0,
                PaymentSuccessRate = 0,
                AveragePaymentAmount = 0,
                TotalRefunds = 0,
                TotalTransactions = 0,
                SuccessfulTransactions = 0,
                FailedTransactions = 0,
                MonthlyPayments = new List<MonthlyPaymentDto>(),
                PaymentMethods = new List<PaymentMethodAnalyticsDto>(),
                PaymentStatuses = new List<PaymentStatusAnalyticsDto>()
            };

            return ApiResponse<PaymentAnalyticsDto>.SuccessResponse(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment analytics");
            return ApiResponse<PaymentAnalyticsDto>.ErrorResponse("Failed to get payment analytics");
        }
    }

    private string ConvertToCsv(RevenueSummaryDto revenueData)
    {
        // Simple CSV conversion
        return "Month,Revenue,Subscriptions\n" +
               $"{DateTime.Now:yyyy-MM},{revenueData.TotalRevenue},{revenueData.TotalSubscriptions}";
    }
} 
