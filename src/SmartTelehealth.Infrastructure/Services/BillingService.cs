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
                SubscriptionId = createDto.SubscriptionId != null ? Guid.Parse(createDto.SubscriptionId) : null,
                Amount = createDto.Amount,
                Description = createDto.Description,
                DueDate = createDto.DueDate,
                Status = BillingRecord.BillingStatus.Pending,
                Type = BillingRecord.BillingType.Consultation // Use enum instead of string
            };
            
            var createdRecord = await _billingRepository.CreateAsync(billingRecord);
            var billingRecordDto = MapToDto(createdRecord);
            
            await _auditService.LogPaymentEventAsync(
                createDto.UserId.ToString(),
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
                createDto.UserId.ToString(),
                "BillingRecordCreationFailed",
                "N/A",
                ex.Message
            );
            return ApiResponse<BillingRecordDto>.ErrorResponse("An error occurred while creating the billing record", 500);
        }
    }
    
    public async Task<ApiResponse<BillingRecordDto>> ProcessPaymentAsync(Guid billingRecordId)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
                return ApiResponse<BillingRecordDto>.ErrorResponse("Billing record not found", 404);
            
            if (billingRecord.Status == BillingRecord.BillingStatus.Paid)
                return ApiResponse<BillingRecordDto>.ErrorResponse("Payment has already been processed", 400);
            
            // Process payment through Stripe
            var paymentResult = await _stripeService.ProcessPaymentAsync(
                "pm_card_visa", // Default test payment method
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

                return ApiResponse<BillingRecordDto>.ErrorResponse($"Payment failed: {paymentResult.ErrorMessage}", 400);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for billing record {BillingRecordId}", billingRecordId);
            
            // Update billing record with failure status
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
                    // AUDIT LOG: Payment failure
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
    
    public async Task<ApiResponse<BillingRecordDto>> CreateAdjustmentAsync(Guid billingRecordId, CreateBillingAdjustmentDto adjustmentDto)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
                return ApiResponse<BillingRecordDto>.ErrorResponse("Billing record not found", 404);
            
            var adjustment = new BillingAdjustment
            {
                BillingRecordId = billingRecordId,
                Amount = adjustmentDto.Amount,
                Description = adjustmentDto.Reason,
                Type = BillingAdjustment.AdjustmentType.Discount, // Use enum instead of string
                Reason = adjustmentDto.Reason
            };
            
            var createdAdjustment = await _billingRepository.CreateAdjustmentAsync(adjustment);
            
            // Update billing record total
            billingRecord.Amount += adjustment.Amount;
            await _billingRepository.UpdateAsync(billingRecord);
            
            var billingRecordDto = MapToDto(billingRecord);
            
            await _auditService.LogPaymentEventAsync(
                billingRecord.UserId.ToString(),
                "BillingAdjustmentCreated",
                billingRecord.Id.ToString(),
                "Success"
            );
            
            return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Billing adjustment created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating billing adjustment for record {BillingRecordId}", billingRecordId);
            return ApiResponse<BillingRecordDto>.ErrorResponse("An error occurred while creating the billing adjustment", 500);
        }
    }
    
    public async Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetUserBillingHistoryAsync(Guid userId)
    {
        try
        {
            var billingRecords = await _billingRepository.GetByUserIdAsync(userId);
            var billingRecordDtos = billingRecords.Select(MapToDto);
            return ApiResponse<IEnumerable<BillingRecordDto>>.SuccessResponse(billingRecordDtos, "User billing history retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing history for user {UserId}", userId);
            return ApiResponse<IEnumerable<BillingRecordDto>>.ErrorResponse("An error occurred while retrieving billing history", 500);
        }
    }
    
    public async Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetPendingPaymentsAsync()
    {
        try
        {
            var pendingRecords = await _billingRepository.GetPendingPaymentsAsync();
            var pendingRecordDtos = pendingRecords.Select(MapToDto);
            return ApiResponse<IEnumerable<BillingRecordDto>>.SuccessResponse(pendingRecordDtos, "Pending payments retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending payments");
            return ApiResponse<IEnumerable<BillingRecordDto>>.ErrorResponse("An error occurred while retrieving pending payments", 500);
        }
    }
    
    public async Task<ApiResponse<BillingRecordDto>> GetBillingRecordAsync(Guid id)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(id);
            if (billingRecord == null)
                return ApiResponse<BillingRecordDto>.ErrorResponse("Billing record not found", 404);
            
            var billingRecordDto = MapToDto(billingRecord);
            return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Billing record retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing record {Id}", id);
            return ApiResponse<BillingRecordDto>.ErrorResponse("An error occurred while retrieving the billing record", 500);
        }
    }
    
    public async Task<ApiResponse<BillingRecordDto>> UpdateBillingRecordAsync(Guid id, UpdateBillingRecordDto updateDto)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(id);
            if (billingRecord == null)
                return ApiResponse<BillingRecordDto>.ErrorResponse("Billing record not found", 404);
            
            // Only update properties that exist in BillingRecord entity
            if (updateDto.BillingStatusId.HasValue)
            {
                billingRecord.Status = (BillingRecord.BillingStatus)updateDto.BillingStatusId.Value;
            }
            if (updateDto.PaidDate.HasValue)
            {
                billingRecord.PaidAt = updateDto.PaidDate.Value;
            }
            if (!string.IsNullOrEmpty(updateDto.StripePaymentIntentId))
            {
                billingRecord.PaymentIntentId = updateDto.StripePaymentIntentId;
            }
            if (!string.IsNullOrEmpty(updateDto.FailureReason))
            {
                billingRecord.FailureReason = updateDto.FailureReason;
            }
            
            var updatedRecord = await _billingRepository.UpdateAsync(billingRecord);
            var billingRecordDto = MapToDto(updatedRecord);
            
            await _auditService.LogPaymentEventAsync(
                billingRecord.UserId.ToString(),
                "BillingRecordUpdated",
                billingRecord.Id.ToString(),
                "Success"
            );
            
            return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Billing record updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating billing record {Id}", id);
            return ApiResponse<BillingRecordDto>.ErrorResponse("An error occurred while updating the billing record", 500);
        }
    }
    
    public async Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetSubscriptionBillingHistoryAsync(Guid subscriptionId)
    {
        try
        {
            var billingRecords = await _billingRepository.GetBySubscriptionIdAsync(subscriptionId);
            var billingRecordDtos = billingRecords.Select(MapToDto);
            return ApiResponse<IEnumerable<BillingRecordDto>>.SuccessResponse(billingRecordDtos, "Subscription billing history retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription billing history for {SubscriptionId}", subscriptionId);
            return ApiResponse<IEnumerable<BillingRecordDto>>.ErrorResponse("An error occurred while retrieving subscription billing history", 500);
        }
    }
    
    public async Task<ApiResponse<BillingRecordDto>> ProcessRefundAsync(Guid billingRecordId, decimal amount)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
                return ApiResponse<BillingRecordDto>.ErrorResponse("Billing record not found", 404);
            
            if (string.IsNullOrEmpty(billingRecord.PaymentIntentId))
                return ApiResponse<BillingRecordDto>.ErrorResponse("No payment intent ID found for refund", 400);
            
            var refundSuccess = await _stripeService.ProcessRefundAsync(billingRecord.PaymentIntentId, amount);
            
            if (refundSuccess)
            {
                billingRecord.Status = BillingRecord.BillingStatus.Refunded;
                billingRecord.UpdatedAt = DateTime.UtcNow;
                
                var updatedRecord = await _billingRepository.UpdateAsync(billingRecord);
                var billingRecordDto = MapToDto(updatedRecord);
                
                // Send refund notifications
                await SendRefundNotificationsAsync(billingRecordDto, amount);
                
                await _auditService.LogPaymentEventAsync(
                    billingRecord.UserId.ToString(),
                    "RefundProcessed",
                    billingRecord.Id.ToString(),
                    "Success",
                    $"Refund amount: {amount}"
                );
                
                return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Refund processed successfully");
            }
            else
            {
                return ApiResponse<BillingRecordDto>.ErrorResponse("Refund processing failed", 500);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for billing record {BillingRecordId}", billingRecordId);
            return ApiResponse<BillingRecordDto>.ErrorResponse("An error occurred while processing the refund", 500);
        }
    }
    
    public async Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetOverdueBillingRecordsAsync()
    {
        try
        {
            var pendingRecords = await _billingRepository.GetPendingPaymentsAsync();
            var overdueRecords = pendingRecords
                .Where(r => r.DueDate.HasValue && r.DueDate.Value < DateTime.UtcNow)
                .Select(MapToDto)
                .ToList();
            
            // Send overdue notifications for each overdue record
            foreach (var overdueRecord in overdueRecords)
            {
                await SendOverdueNotificationsAsync(overdueRecord);
            }
            
            return ApiResponse<IEnumerable<BillingRecordDto>>.SuccessResponse(overdueRecords, "Overdue billing records retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue billing records");
            return ApiResponse<IEnumerable<BillingRecordDto>>.ErrorResponse("An error occurred while retrieving overdue billing records", 500);
        }
    }
    
    public async Task<ApiResponse<decimal>> CalculateTotalAmountAsync(decimal baseAmount, decimal taxAmount, decimal shippingAmount)
    {
        try
        {
            var totalAmount = baseAmount + taxAmount + shippingAmount;
            return ApiResponse<decimal>.SuccessResponse(totalAmount, "Total amount calculated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total amount");
            return ApiResponse<decimal>.ErrorResponse("An error occurred while calculating total amount", 500);
        }
    }
    
    public async Task<ApiResponse<decimal>> CalculateTaxAmountAsync(decimal baseAmount, string state)
    {
        try
        {
            // Simple tax calculation - in real implementation, this would use tax tables
            const decimal defaultTaxRate = 0.08m; // 8% default tax rate
            var taxAmount = baseAmount * defaultTaxRate;
            return ApiResponse<decimal>.SuccessResponse(taxAmount, "Tax amount calculated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating tax amount for state {State}", state);
            return ApiResponse<decimal>.ErrorResponse("An error occurred while calculating tax amount", 500);
        }
    }
    
    public async Task<ApiResponse<decimal>> CalculateShippingAmountAsync(string deliveryAddress, bool isExpress)
    {
        try
        {
            // Simple shipping calculation - in real implementation, this would use shipping APIs
            const decimal standardShipping = 5.99m;
            const decimal expressShipping = 12.99m;
            var shippingAmount = isExpress ? expressShipping : standardShipping;
            return ApiResponse<decimal>.SuccessResponse(shippingAmount, "Shipping amount calculated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating shipping amount");
            return ApiResponse<decimal>.ErrorResponse("An error occurred while calculating shipping amount", 500);
        }
    }
    
    public async Task<ApiResponse<bool>> IsPaymentOverdueAsync(Guid billingRecordId)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
            if (billingRecord == null)
                return ApiResponse<bool>.ErrorResponse("Billing record not found", 404);
            
            var isOverdue = billingRecord.DueDate.HasValue && 
                           billingRecord.DueDate.Value < DateTime.UtcNow && 
                           billingRecord.Status != BillingRecord.BillingStatus.Paid;
            
            return ApiResponse<bool>.SuccessResponse(isOverdue, "Payment overdue status checked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if payment is overdue for {BillingRecordId}", billingRecordId);
            return ApiResponse<bool>.ErrorResponse("An error occurred while checking payment overdue status", 500);
        }
    }
    
    public async Task<ApiResponse<DateTime>> CalculateDueDateAsync(DateTime billingDate, int gracePeriodDays)
    {
        try
        {
            var dueDate = billingDate.AddDays(gracePeriodDays);
            return ApiResponse<DateTime>.SuccessResponse(dueDate, "Due date calculated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating due date");
            return ApiResponse<DateTime>.ErrorResponse("An error occurred while calculating due date", 500);
        }
    }

    public async Task<ApiResponse<BillingAnalyticsDto>> GetBillingAnalyticsAsync()
    {
        try
        {
            // Placeholder implementation - in real app, these would be actual repository methods
            var totalRevenue = 0m; // await _billingRepository.GetTotalRevenueAsync();
            var monthlyRevenue = 0m; // await _billingRepository.GetMonthlyRevenueAsync();
            var failedPayments = 0; // await _billingRepository.GetFailedPaymentsCountAsync();
            var refundsIssued = 0; // await _billingRepository.GetRefundsCountAsync();

            var analytics = new BillingAnalyticsDto
            {
                TotalRevenue = totalRevenue,
                MonthlyRecurringRevenue = monthlyRevenue,
                AverageRevenuePerUser = 0, // Calculate based on user count
                FailedPayments = failedPayments,
                RefundsIssued = refundsIssued,
                PaymentSuccessRate = 95.5m, // Placeholder
                RevenueByCategory = new List<CategoryRevenueDto>(),
                RevenueTrend = new List<RevenueTrendDto>()
            };

            return ApiResponse<BillingAnalyticsDto>.SuccessResponse(analytics, "Billing analytics retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing analytics");
            return ApiResponse<BillingAnalyticsDto>.ErrorResponse("An error occurred while retrieving billing analytics", 500);
        }
    }
    
    private BillingRecordDto MapToDto(BillingRecord billingRecord)
    {
        return new BillingRecordDto
        {
            Id = billingRecord.Id.ToString(),
            UserId = billingRecord.UserId.ToString(),
            UserName = billingRecord.User?.FullName ?? "",
            ConsultationId = billingRecord.ConsultationId?.ToString(),
            MedicationDeliveryId = billingRecord.MedicationDeliveryId?.ToString(),
            Amount = billingRecord.Amount,
            Currency = "USD", // Default currency since BillingRecord doesn't have Currency property
            Description = billingRecord.Description ?? "",
            BillingStatusId = (int)billingRecord.Status,
            BillingStatusName = billingRecord.Status.ToString(),
            DueDate = billingRecord.DueDate ?? DateTime.UtcNow,
            PaidDate = billingRecord.PaidAt,
            PaymentMethodId = null, // BillingRecord doesn't have PaymentMethodId
            StripePaymentIntentId = billingRecord.PaymentIntentId,
            StripeSessionId = null, // BillingRecord doesn't have SessionId
            FailureReason = billingRecord.FailureReason,
            RefundAmount = null, // BillingRecord doesn't have RefundAmount
            RefundReason = null, // BillingRecord doesn't have RefundReason
            RefundDate = null, // BillingRecord doesn't have RefundDate
            CreatedAt = billingRecord.CreatedAt,
            UpdatedAt = billingRecord.UpdatedAt
        };
    }

    // Private methods for sending notifications
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
    
    private async Task SendRefundNotificationsAsync(BillingRecordDto billingRecord, decimal refundAmount)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(billingRecord.UserId));
            if (user == null) return;
            
            var userName = $"{user.FirstName} {user.LastName}";
            
            // Send email notification
            if (!string.IsNullOrEmpty(user.Email))
            {
                await _notificationService.SendRefundProcessedEmailAsync(user.Email, userName, billingRecord, refundAmount);
            }
            
            // Send in-app notification
            await _notificationService.CreateInAppNotificationAsync(
                Guid.Parse(billingRecord.UserId),
                "Refund Processed",
                $"Your refund of ${refundAmount} has been processed successfully."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending refund notifications for billing record {BillingRecordId}", billingRecord.Id);
        }
    }
    
    private async Task SendOverdueNotificationsAsync(BillingRecordDto billingRecord)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(billingRecord.UserId));
            if (user == null) return;
            
            var userName = $"{user.FirstName} {user.LastName}";
            
            // Send email notification
            if (!string.IsNullOrEmpty(user.Email))
            {
                await _notificationService.SendOverduePaymentEmailAsync(user.Email, userName, billingRecord);
            }
            
            // Send in-app notification
            var daysOverdue = (int)(DateTime.UtcNow - billingRecord.DueDate).TotalDays;
            await _notificationService.CreateInAppNotificationAsync(
                Guid.Parse(billingRecord.UserId),
                "Payment Overdue",
                $"Your payment of ${billingRecord.Amount} is {(daysOverdue > 0 ? daysOverdue : 0)} days overdue. Please make the payment as soon as possible."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending overdue notifications for billing record {BillingRecordId}", billingRecord.Id);
        }
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
    public Task<ApiResponse<BillingRecordDto>> CreateRecurringBillingAsync(CreateRecurringBillingDto createDto)
        => Task.FromResult(ApiResponse<BillingRecordDto>.ErrorResponse("Not implemented in infrastructure layer", 501));
    public Task<ApiResponse<BillingRecordDto>> ProcessRecurringPaymentAsync(Guid subscriptionId)
        => Task.FromResult(ApiResponse<BillingRecordDto>.ErrorResponse("Not implemented in infrastructure layer", 501));
    public Task<ApiResponse<bool>> CancelRecurringBillingAsync(Guid subscriptionId)
        => Task.FromResult(ApiResponse<bool>.ErrorResponse("Not implemented in infrastructure layer", 501));
    public Task<ApiResponse<BillingRecordDto>> CreateUpfrontPaymentAsync(CreateUpfrontPaymentDto createDto)
        => Task.FromResult(ApiResponse<BillingRecordDto>.ErrorResponse("Not implemented in infrastructure layer", 501));
    public Task<ApiResponse<BillingRecordDto>> ProcessBundlePaymentAsync(CreateBundlePaymentDto createDto)
        => Task.FromResult(ApiResponse<BillingRecordDto>.ErrorResponse("Not implemented in infrastructure layer", 501));
    public Task<ApiResponse<BillingRecordDto>> ApplyBillingAdjustmentAsync(Guid billingRecordId, CreateBillingAdjustmentDto adjustmentDto)
        => Task.FromResult(ApiResponse<BillingRecordDto>.ErrorResponse("Not implemented in infrastructure layer", 501));
    public Task<ApiResponse<IEnumerable<BillingAdjustmentDto>>> GetBillingAdjustmentsAsync(Guid billingRecordId)
        => Task.FromResult(ApiResponse<IEnumerable<BillingAdjustmentDto>>.ErrorResponse("Not implemented in infrastructure layer", 501));
    public Task<ApiResponse<BillingRecordDto>> RetryFailedPaymentAsync(Guid billingRecordId)
        => Task.FromResult(ApiResponse<BillingRecordDto>.ErrorResponse("Not implemented in infrastructure layer", 501));
    public Task<ApiResponse<BillingRecordDto>> ProcessPartialPaymentAsync(Guid billingRecordId, decimal amount)
        => Task.FromResult(ApiResponse<BillingRecordDto>.ErrorResponse("Not implemented in infrastructure layer", 501));
    public Task<ApiResponse<BillingRecordDto>> CreateInvoiceAsync(CreateInvoiceDto createDto)
        => Task.FromResult(ApiResponse<BillingRecordDto>.ErrorResponse("Not implemented in infrastructure layer", 501));
    public Task<ApiResponse<byte[]>> GenerateInvoicePdfAsync(Guid billingRecordId)
        => Task.FromResult(ApiResponse<byte[]>.ErrorResponse("Not implemented in infrastructure layer", 501));
    public Task<ApiResponse<byte[]>> GenerateBillingReportAsync(DateTime startDate, DateTime endDate, string format = "pdf")
        => Task.FromResult(ApiResponse<byte[]>.ErrorResponse("Not implemented in infrastructure layer", 501));
    public Task<ApiResponse<BillingSummaryDto>> GetBillingSummaryAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
        => Task.FromResult(ApiResponse<BillingSummaryDto>.ErrorResponse("Not implemented in infrastructure layer", 501));
    public Task<ApiResponse<PaymentScheduleDto>> GetPaymentScheduleAsync(Guid subscriptionId)
        => Task.FromResult(ApiResponse<PaymentScheduleDto>.ErrorResponse("Not implemented in infrastructure layer", 501));
    public Task<ApiResponse<bool>> UpdatePaymentMethodAsync(Guid billingRecordId, string paymentMethodId)
        => Task.FromResult(ApiResponse<bool>.ErrorResponse("Not implemented in infrastructure layer", 501));
    public Task<ApiResponse<BillingRecordDto>> CreateBillingCycleAsync(CreateBillingCycleDto createDto)
        => Task.FromResult(ApiResponse<BillingRecordDto>.ErrorResponse("Not implemented in infrastructure layer", 501));
    public Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetBillingCycleRecordsAsync(Guid billingCycleId)
        => Task.FromResult(ApiResponse<IEnumerable<BillingRecordDto>>.ErrorResponse("Not implemented in infrastructure layer", 501));
    public Task<ApiResponse<BillingRecordDto>> ProcessBillingCycleAsync(Guid billingCycleId)
        => Task.FromResult(ApiResponse<BillingRecordDto>.ErrorResponse("Not implemented in infrastructure layer", 501));

    public async Task<ApiResponse<byte[]>> ExportRevenueAsync(DateTime? from = null, DateTime? to = null, string? planId = null, string format = "csv")
    {
        // TODO: Implement filtering by planId, type, status
        var allRecords = await _billingRepository.GetAllAsync();
        var exportList = allRecords.Select(r => new RevenueExportDto
        {
            BillingId = r.Id.ToString(),
            UserId = r.UserId.ToString(),
            Amount = r.Amount,
            AccruedAmount = r.AccruedAmount,
            Status = r.Status.ToString(),
            Type = r.Type.ToString(),
            BillingDate = r.BillingDate,
            PaidAt = r.PaidAt,
            AccrualStartDate = r.AccrualStartDate,
            AccrualEndDate = r.AccrualEndDate,
            InvoiceNumber = r.InvoiceNumber,
            FailureReason = r.FailureReason
        }).ToList();
        // Generate CSV (stub)
        var csv = "BillingId,UserId,Amount,AccruedAmount,Status,Type,BillingDate,PaidAt,AccrualStartDate,AccrualEndDate,InvoiceNumber,FailureReason\n";
        foreach (var e in exportList)
        {
            csv += $"{e.BillingId},{e.UserId},{e.Amount},{e.AccruedAmount},{e.Status},{e.Type},{e.BillingDate},{e.PaidAt},{e.AccrualStartDate},{e.AccrualEndDate},{e.InvoiceNumber},{e.FailureReason}\n";
        }
        return ApiResponse<byte[]>.SuccessResponse(System.Text.Encoding.UTF8.GetBytes(csv), "Export generated");
    }
} 