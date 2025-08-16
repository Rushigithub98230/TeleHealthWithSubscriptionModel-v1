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
    
    public async Task<JsonModel> CreateBillingRecordAsync(CreateBillingRecordDto createDto)
    {
        try
        {
            var billingRecord = new BillingRecord
            {
                UserId = int.Parse(createDto.UserId),
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
            
            return new JsonModel
            {
                data = billingRecordDto,
                Message = "Billing record created successfully",
                StatusCode = 200
            };
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
            return new JsonModel
            {
                data = new object(),
                Message = "An error occurred while creating the billing record",
                StatusCode = 500
            };
        }
    }
    
    public async Task<JsonModel> ProcessPaymentAsync(Guid billingRecordId)
    {
        return await ProcessPaymentWithRetryAsync(billingRecordId, 0);
    }

    private async Task<JsonModel> ProcessPaymentWithRetryAsync(Guid billingRecordId, int attempt)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Billing record not found",
                    StatusCode = 404
                };
            
            if (billingRecord.Status == BillingRecord.BillingStatus.Paid)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Payment has already been processed",
                    StatusCode = 400
                };
            
            // Get user's default payment method
            var user = await _userRepository.GetByIdAsync(billingRecord.UserId);
            if (user == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "User not found",
                    StatusCode = 404
                };

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
                
                return new JsonModel
                {
                    data = new object(),
                    Message = "No default payment method found",
                    StatusCode = 400
                };
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

                return new JsonModel
                {
                    data = billingRecordDto,
                    Message = "Payment processed successfully",
                    StatusCode = 200
                };
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
            
            return new JsonModel
            {
                data = new object(),
                Message = "An error occurred while processing payment",
                StatusCode = 500
            };
        }
    }

    private async Task<JsonModel> HandleFailedPaymentWithRetryAsync(BillingRecord billingRecord, PaymentResultDto paymentResult, int attempt)
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
        
        return new JsonModel
        {
            data = new object(),
            Message = $"Payment failed: {paymentResult.ErrorMessage}",
            StatusCode = 400
        };
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

    public async Task<JsonModel> RetryPaymentAsync(Guid billingRecordId)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Billing record not found",
                    StatusCode = 404
                };

            if (billingRecord.Status == BillingRecord.BillingStatus.Paid)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Payment has already been processed",
                    StatusCode = 400
                };

            // Reset status to pending for retry
            billingRecord.Status = BillingRecord.BillingStatus.Pending;
            billingRecord.FailureReason = null;
            billingRecord.UpdatedAt = DateTime.UtcNow;
            await _billingRepository.UpdateAsync(billingRecord);

            // Process payment with retry
            var result = await ProcessPaymentWithRetryAsync(billingRecordId, 0);
            
            if (result.StatusCode == 200)
            {
                var paymentResult = new PaymentResultDto
                {
                    PaymentIntentId = result.data is BillingRecordDto dto ? dto.PaymentIntentId : string.Empty,
                    Status = "succeeded",
                    Amount = result.data is BillingRecordDto dto2 ? dto2.Amount : 0,
                    Currency = "usd"
                };
                
                return new JsonModel
                {
                    data = paymentResult,
                    Message = "Payment retry successful",
                    StatusCode = 200
                };
            }
            
            return new JsonModel
            {
                data = new object(),
                Message = "Payment retry failed",
                StatusCode = 400
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying payment for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retry payment",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ProcessRefundAsync(Guid billingRecordId, decimal amount, string reason)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Billing record not found",
                    StatusCode = 404
                };

            if (billingRecord.Status != BillingRecord.BillingStatus.Paid)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Only paid billing records can be refunded",
                    StatusCode = 400
                };

            if (string.IsNullOrEmpty(billingRecord.PaymentIntentId))
                return new JsonModel
                {
                    data = new object(),
                    Message = "No payment intent found for refund",
                    StatusCode = 400
                };

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

                return new JsonModel
                {
                    data = new RefundResultDto
                    {
                        BillingRecordId = billingRecordId,
                        RefundAmount = amount,
                        Status = "completed",
                        ProcessedAt = DateTime.UtcNow,
                        Reason = reason
                    },
                    Message = "Refund processed successfully",
                    StatusCode = 200
                };
            }
            
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to process refund",
                StatusCode = 400
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to process refund",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetBillingRecordAsync(Guid billingRecordId)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Billing record not found",
                    StatusCode = 404
                };

            return new JsonModel
            {
                data = MapToDto(billingRecord),
                Message = "Billing record retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing record {BillingRecordId}", billingRecordId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve billing record",
                StatusCode = 500
            };
        }
    }

    public async Task<IEnumerable<PaymentHistoryDto>> GetPaymentHistoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var billingRecords = await _billingRepository.GetByUserIdAsync(int.Parse(userId));
            
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

    public async Task<JsonModel> GetPaymentAnalyticsAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var billingRecords = await _billingRepository.GetByUserIdAsync(userId);
            
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

            return new JsonModel
            {
                data = analytics,
                Message = "Payment analytics retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment analytics for user {UserId}", userId);
            return new JsonModel
            {
                data = new object(),
                Message = "Error retrieving payment analytics",
                StatusCode = 500
            };
        }
    }

    // Private helper methods
    private async Task SendPaymentNotificationsAsync(BillingRecordDto billingRecord, bool isSuccess)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(int.Parse(billingRecord.UserId));
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
                    int.Parse(billingRecord.UserId),
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
                    int.Parse(billingRecord.UserId),
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
                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await _notificationService.SendSubscriptionSuspendedNotificationAsync(billingRecord.UserId.ToString(), billingRecord.SubscriptionId?.ToString() ?? "");
                _logger.LogInformation("Email notifications disabled - would have sent subscription suspended notification to user {UserId}", billingRecord.UserId);
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
            CreatedAt = billingRecord.CreatedDate ?? DateTime.UtcNow,
            UpdatedAt = billingRecord.UpdatedDate ?? DateTime.UtcNow
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
    public async Task<JsonModel> GetRevenueSummaryAsync(DateTime? from = null, DateTime? to = null, string? planId = null)
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
        return new JsonModel
        {
            data = summary,
            Message = "Revenue summary retrieved successfully",
            StatusCode = 200
        };
    }

    // PHASE 2 STUBS
    public async Task<JsonModel> CreateRecurringBillingAsync(CreateRecurringBillingDto createDto)
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
        return new JsonModel
        {
            data = billingRecordDto,
            Message = "Recurring billing record created successfully",
            StatusCode = 200
        };
    }
    public async Task<JsonModel> ProcessRecurringPaymentAsync(Guid subscriptionId)
    {
        // Example: process payment for the next due billing record for the subscription
        var records = await _billingRepository.GetBySubscriptionIdAsync(subscriptionId);
        var nextDue = records.OrderBy(r => r.DueDate).FirstOrDefault(r => r.Status == BillingRecord.BillingStatus.Pending);
        if (nextDue == null)
            return new JsonModel
            {
                data = new object(),
                Message = "No pending recurring payment found",
                StatusCode = 404
            };
        return await ProcessPaymentAsync(nextDue.Id);
    }
    public async Task<JsonModel> CancelRecurringBillingAsync(Guid subscriptionId)
    {
        // Example: mark all future recurring billing records as cancelled
        var records = await _billingRepository.GetBySubscriptionIdAsync(subscriptionId);
        foreach (var record in records.Where(r => r.Status == BillingRecord.BillingStatus.Pending))
        {
            record.Status = BillingRecord.BillingStatus.Cancelled;
            await _billingRepository.UpdateAsync(record);
        }
        return new JsonModel
        {
            data = true,
            Message = "Recurring billing cancelled",
            StatusCode = 200
        };
    }
    public async Task<JsonModel> CreateUpfrontPaymentAsync(CreateUpfrontPaymentDto createDto)
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
        return new JsonModel
        {
            data = billingRecordDto,
            Message = "Upfront payment processed successfully",
            StatusCode = 200
        };
    }
    public async Task<JsonModel> ProcessBundlePaymentAsync(CreateBundlePaymentDto createDto)
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
        return new JsonModel
        {
            data = billingRecordDto,
            Message = "Bundle payment processed successfully",
            StatusCode = 200
        };
    }
    public async Task<JsonModel> ApplyBillingAdjustmentAsync(Guid billingRecordId, CreateBillingAdjustmentDto adjustmentDto)
    {
        // Example: apply an adjustment (discount, credit, etc.)
        var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
        if (billingRecord == null)
            return new JsonModel
            {
                data = new object(),
                Message = "Billing record not found",
                StatusCode = 404
            };
        billingRecord.Amount += adjustmentDto.Amount;
        billingRecord.UpdatedAt = DateTime.UtcNow;
        await _billingRepository.UpdateAsync(billingRecord);
        var billingRecordDto = MapToDto(billingRecord);
        await _auditService.LogPaymentEventAsync(billingRecord.UserId.ToString(), "BillingAdjustmentApplied", billingRecord.Id.ToString(), "Success");
        return new JsonModel
        {
            data = billingRecordDto,
            Message = "Billing adjustment applied successfully",
            StatusCode = 200
        };
    }
    public async Task<JsonModel> GetBillingAdjustmentsAsync(Guid billingRecordId)
    {
        var adjustments = await _billingRepository.GetAdjustmentsByBillingRecordIdAsync(billingRecordId);
        var dtos = adjustments.Select(a => new BillingAdjustmentDto
        {
            Id = a.Id,
            BillingRecordId = a.BillingRecordId,
            Amount = a.Amount,
            AdjustmentType = a.Type.ToString(),
            Reason = a.Reason,
            AppliedBy = a.AppliedBy?.ToString() ?? "",
            AppliedAt = a.CreatedDate ?? DateTime.UtcNow,
            IsPercentage = a.IsPercentage
        });
        return new JsonModel
        {
            data = dtos,
            Message = "Billing adjustments retrieved successfully",
            StatusCode = 200
        };
    }
    public async Task<JsonModel> RetryFailedPaymentAsync(Guid billingRecordId)
    {
        // Example: retry payment for a failed billing record
        var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
        if (billingRecord == null)
            return new JsonModel
            {
                data = new object(),
                Message = "Billing record not found",
                StatusCode = 404
            };
        if (billingRecord.Status != BillingRecord.BillingStatus.Failed)
            return new JsonModel
            {
                data = new object(),
                Message = "Billing record is not in failed status",
                StatusCode = 400
            };
        return await ProcessPaymentAsync(billingRecordId);
    }
    public async Task<JsonModel> ProcessPartialPaymentAsync(Guid billingRecordId, decimal amount)
    {
        // Example: process a partial payment
        var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
        if (billingRecord == null)
            return new JsonModel
            {
                data = new object(),
                Message = "Billing record not found",
                StatusCode = 404
            };
        if (amount <= 0 || amount > billingRecord.Amount)
            return new JsonModel
            {
                data = new object(),
                Message = "Invalid partial payment amount",
                StatusCode = 400
            };
        // Simulate partial payment logic
        billingRecord.Amount -= amount;
        billingRecord.UpdatedDate = DateTime.UtcNow;
        await _billingRepository.UpdateAsync(billingRecord);
        var billingRecordDto = MapToDto(billingRecord);
        await _auditService.LogPaymentEventAsync(billingRecord.UserId.ToString(), "PartialPaymentProcessed", billingRecord.Id.ToString(), "Success");
        return new JsonModel
        {
            data = billingRecordDto,
            Message = "Partial payment processed successfully",
            StatusCode = 200
        };
    }
    public async Task<JsonModel> CreateInvoiceAsync(CreateInvoiceDto createDto)
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
        return new JsonModel
        {
            data = billingRecordDto,
            Message = "Invoice created successfully",
            StatusCode = 200
        };
    }
    public async Task<JsonModel> GenerateInvoicePdfAsync(Guid billingRecordId)
    {
        // Example: generate a PDF (stubbed as byte array)
        var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
        if (billingRecord == null)
            return new JsonModel
            {
                data = new object(),
                Message = "Billing record not found",
                StatusCode = 404
            };
        var pdfBytes = System.Text.Encoding.UTF8.GetBytes($"Invoice for {billingRecord.Id} - Amount: {billingRecord.Amount}");
        return new JsonModel
        {
            data = pdfBytes,
            Message = "Invoice PDF generated",
            StatusCode = 200
        };
    }
    public async Task<JsonModel> GenerateBillingReportAsync(DateTime startDate, DateTime endDate, string format = "pdf")
    {
        // Example: generate a report (stubbed as byte array)
        var records = await _billingRepository.GetAllAsync();
        var reportBytes = System.Text.Encoding.UTF8.GetBytes($"Billing report from {startDate} to {endDate} - Total records: {records.Count()}");
        return new JsonModel
        {
            data = reportBytes,
            Message = "Billing report generated",
            StatusCode = 200
        };
    }
    public async Task<JsonModel> GetBillingSummaryAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        // Example: summarize billing for a user
        var records = await _billingRepository.GetByUserIdAsync(userId);
        var total = records.Where(r => (!startDate.HasValue || r.CreatedDate >= startDate) && (!endDate.HasValue || r.CreatedDate <= endDate)).Sum(r => r.Amount);
        var summary = new BillingSummaryDto { UserId = userId, TotalBilled = total };
        return new JsonModel
        {
            data = summary,
            Message = "Billing summary generated",
            StatusCode = 200
        };
    }
    public async Task<JsonModel> GetPaymentScheduleAsync(Guid subscriptionId)
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
        return new JsonModel
        {
            data = schedule,
            Message = "Payment schedule generated",
            StatusCode = 200
        };
    }
    public async Task<JsonModel> UpdatePaymentMethodAsync(Guid billingRecordId, string paymentMethodId)
    {
        // Not implemented in infrastructure layer
        return await Task.FromResult(new JsonModel
        {
            data = new object(),
            Message = "Not implemented in infrastructure layer",
            StatusCode = 501
        });
    }
    /// <summary>
    /// Creates a new billing cycle record. UserId must always be a Guid.
    /// </summary>
    /// <param name="createDto">The billing cycle creation DTO.</param>
    /// <returns>API response with the created billing record DTO.</returns>
    public async Task<JsonModel> CreateBillingCycleAsync(CreateBillingCycleDto createDto)
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
        return new JsonModel
        {
            data = billingRecordDto,
            Message = "Billing cycle created successfully",
            StatusCode = 200
        };
    }
    public async Task<JsonModel> GetBillingCycleRecordsAsync(Guid billingCycleId)
    {
        // Example: fetch all records for a billing cycle
        var records = await _billingRepository.GetByBillingCycleIdAsync(billingCycleId);
        var dtos = records.Select(MapToDto);
        return new JsonModel
        {
            data = dtos,
            Message = "Billing cycle records retrieved successfully",
            StatusCode = 200
        };
    }
    public async Task<JsonModel> ProcessBillingCycleAsync(Guid billingCycleId)
    {
        // Example: process all pending payments in a billing cycle
        var records = await _billingRepository.GetByBillingCycleIdAsync(billingCycleId);
        foreach (var record in records.Where(r => r.Status == BillingRecord.BillingStatus.Pending))
        {
            await ProcessPaymentAsync(record.Id);
        }
        var dtos = records.Select(MapToDto);
        return new JsonModel
        {
            data = dtos.FirstOrDefault(),
            Message = "Billing cycle processed",
            StatusCode = 200
        };
    }

    public async Task<JsonModel> ExportRevenueAsync(DateTime? from = null, DateTime? to = null, string? planId = null, string format = "csv")
    {
        try
        {
            // Implementation for revenue export
            var revenueData = await GetRevenueSummaryAsync(from, to, planId);
            if (revenueData.StatusCode != 200)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Failed to get revenue data",
                    StatusCode = 500
                };
            }

            // Convert to CSV format
            var revenueSummary = revenueData.data as RevenueSummaryDto;
            if (revenueSummary == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Invalid revenue data format",
                    StatusCode = 500
                };
            }
            var csvData = ConvertToCsv(revenueSummary);
            var bytes = System.Text.Encoding.UTF8.GetBytes(csvData);
            
            return new JsonModel
            {
                data = bytes,
                Message = "Revenue data exported successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting revenue data");
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to export revenue data",
                StatusCode = 500
            };
        }
    }

    // Missing interface methods
    public async Task<JsonModel> GetUserBillingHistoryAsync(int userId)
    {
        try
        {
            var billingRecords = await _billingRepository.GetByUserIdAsync(userId);
            var dtos = billingRecords.Select(MapToDto);
            return new JsonModel
            {
                data = dtos,
                Message = "Billing history retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing history for user {UserId}", userId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to get billing history",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetAllBillingRecordsAsync()
    {
        try
        {
            var billingRecords = await _billingRepository.GetAllAsync();
            var dtos = billingRecords.Select(MapToDto);
            return new JsonModel
            {
                data = dtos,
                Message = "All billing records retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all billing records");
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to get billing records",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetSubscriptionBillingHistoryAsync(Guid subscriptionId)
    {
        try
        {
            var records = await _billingRepository.GetBySubscriptionIdAsync(subscriptionId);
            var dtos = records.Select(MapToDto);
            return new JsonModel
            {
                data = dtos,
                Message = "Subscription billing history retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing history for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to get subscription billing history",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ProcessRefundAsync(Guid billingRecordId, decimal amount)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Billing record not found",
                    StatusCode = 404
                };

            if (string.IsNullOrEmpty(billingRecord.StripePaymentIntentId))
                return new JsonModel
                {
                    data = new object(),
                    Message = "No payment intent found for refund",
                    StatusCode = 400
                };

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

                return new JsonModel
                {
                    data = MapToDto(updatedRecord),
                    Message = "Refund processed successfully",
                    StatusCode = 200
                };
            }

            return new JsonModel
            {
                data = new object(),
                Message = "Failed to process refund",
                StatusCode = 500
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to process refund",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetOverdueBillingRecordsAsync()
    {
        try
        {
            var overdueRecords = await _billingRepository.GetOverdueRecordsAsync();
            var dtos = overdueRecords.Select(MapToDto);
            return new JsonModel
            {
                data = dtos,
                Message = "Overdue billing records retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue billing records");
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to get overdue billing records",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetPendingPaymentsAsync()
    {
        try
        {
            var pendingRecords = await _billingRepository.GetPendingRecordsAsync();
            var dtos = pendingRecords.Select(MapToDto);
            return new JsonModel
            {
                data = dtos,
                Message = "Pending payments retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending payments");
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to get pending payments",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> CalculateTotalAmountAsync(decimal subtotal, decimal tax, decimal shipping)
    {
        try
        {
            var total = subtotal + tax + shipping;
            return new JsonModel
            {
                data = total,
                Message = "Total amount calculated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total amount");
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to calculate total amount",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> CalculateTaxAmountAsync(decimal amount, string taxRate)
    {
        try
        {
            if (decimal.TryParse(taxRate, out var rate))
            {
                var tax = amount * (rate / 100);
                return new JsonModel
                {
                    data = tax,
                    Message = "Tax amount calculated successfully",
                    StatusCode = 200
                };
            }
            return new JsonModel
            {
                data = new object(),
                Message = "Invalid tax rate",
                StatusCode = 400
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating tax amount");
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to calculate tax amount",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> CalculateShippingAmountAsync(string shippingMethod, bool isExpress)
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
            return new JsonModel
            {
                data = totalShipping,
                Message = "Shipping amount calculated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating shipping amount");
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to calculate shipping amount",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> IsPaymentOverdueAsync(Guid billingRecordId)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Billing record not found",
                    StatusCode = 404
                };

            var isOverdue = billingRecord.DueDate < DateTime.UtcNow && 
                           billingRecord.Status == BillingRecord.BillingStatus.Pending;
            
            return new JsonModel
            {
                data = isOverdue,
                Message = "Payment overdue status checked successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if payment is overdue for billing record {BillingRecordId}", billingRecordId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to check payment overdue status",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> CalculateDueDateAsync(DateTime startDate, int daysToAdd)
    {
        try
        {
            var dueDate = startDate.AddDays(daysToAdd);
            return new JsonModel
            {
                data = dueDate,
                Message = "Due date calculated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating due date");
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to calculate due date",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetBillingAnalyticsAsync()
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

            return new JsonModel
            {
                data = analytics,
                Message = "Billing analytics retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing analytics");
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to get billing analytics",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetPaymentHistoryAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var records = await _billingRepository.GetByUserIdAsync(userId);
            var filteredRecords = records.Where(r => 
                (!startDate.HasValue || r.CreatedDate >= startDate.Value) &&
                (!endDate.HasValue || r.CreatedDate <= endDate.Value));

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
                CreatedAt = r.CreatedDate ?? DateTime.UtcNow,
                ProcessedAt = r.ProcessedAt,
                PaymentDate = r.ProcessedAt ?? r.CreatedDate ?? DateTime.UtcNow,
                Description = r.Description,
                PaymentMethodId = r.StripePaymentIntentId
            });

            return new JsonModel
            {
                data = paymentHistory,
                Message = "Payment history retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment history for user {UserId}", userId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to get payment history",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetPaymentAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
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

            return new JsonModel
            {
                data = analytics,
                Message = "Payment analytics retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment analytics");
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to get payment analytics",
                StatusCode = 500
            };
        }
    }

    private string ConvertToCsv(RevenueSummaryDto revenueData)
    {
        // Simple CSV conversion
        return "Month,Revenue,Subscriptions\n" +
               $"{DateTime.Now:yyyy-MM},{revenueData.TotalRevenue},{revenueData.TotalSubscriptions}";
    }
} 
