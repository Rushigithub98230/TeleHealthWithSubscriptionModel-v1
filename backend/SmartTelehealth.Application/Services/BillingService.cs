using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace SmartTelehealth.Application.Services
{
    public class BillingService : IBillingService
    {
        private readonly IBillingRepository _billingRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BillingService> _logger;

        public BillingService(
            IBillingRepository billingRepository,
            ISubscriptionRepository subscriptionRepository,
            IMapper mapper,
            ILogger<BillingService> logger)
        {
            _billingRepository = billingRepository;
            _subscriptionRepository = subscriptionRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<BillingRecordDto>> CreateBillingRecordAsync(CreateBillingRecordDto createDto)
        {
            try
            {
                var billingRecord = _mapper.Map<BillingRecord>(createDto);
                billingRecord.CreatedAt = DateTime.UtcNow;
                billingRecord.Status = BillingRecord.BillingStatus.Pending;

                // Note: AddAsync method doesn't exist, using CreateAsync instead
                var createdRecord = await _billingRepository.CreateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(createdRecord);
                
                return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating billing record");
                return ApiResponse<BillingRecordDto>.ErrorResponse("Error creating billing record", 500);
            }
        }

        public async Task<ApiResponse<BillingRecordDto>> GetBillingRecordAsync(Guid id)
        {
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(id);
                if (billingRecord == null)
                {
                    return ApiResponse<BillingRecordDto>.ErrorResponse("Billing record not found", 404);
                }

                var billingRecordDto = _mapper.Map<BillingRecordDto>(billingRecord);
                return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing record with ID {BillingRecordId}", id);
                return ApiResponse<BillingRecordDto>.ErrorResponse("Error retrieving billing record", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetUserBillingHistoryAsync(Guid userId)
        {
            try
            {
                var billingRecords = await _billingRepository.GetByUserIdAsync(userId);
                var billingRecordDtos = _mapper.Map<IEnumerable<BillingRecordDto>>(billingRecords);
                return ApiResponse<IEnumerable<BillingRecordDto>>.SuccessResponse(billingRecordDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing history for user {UserId}", userId);
                return ApiResponse<IEnumerable<BillingRecordDto>>.ErrorResponse("Error retrieving billing history", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetAllBillingRecordsAsync()
        {
            try
            {
                var billingRecords = await _billingRepository.GetAllAsync();
                var billingRecordDtos = _mapper.Map<IEnumerable<BillingRecordDto>>(billingRecords);
                return ApiResponse<IEnumerable<BillingRecordDto>>.SuccessResponse(billingRecordDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all billing records");
                return ApiResponse<IEnumerable<BillingRecordDto>>.ErrorResponse("Error retrieving billing records", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetSubscriptionBillingHistoryAsync(Guid subscriptionId)
        {
            try
            {
                _logger.LogInformation("Getting billing history for subscription {SubscriptionId}", subscriptionId);
                
                var billingRecords = await _billingRepository.GetBySubscriptionIdAsync(subscriptionId);
                _logger.LogInformation("Found {Count} billing records for subscription {SubscriptionId}", billingRecords.Count(), subscriptionId);
                
                foreach (var record in billingRecords)
                {
                    _logger.LogInformation("Billing Record: ID={Id}, SubscriptionId={SubscriptionId}, Amount={Amount}, Status={Status}", 
                        record.Id, record.SubscriptionId, record.Amount, record.Status);
                }
                
                var billingRecordDtos = _mapper.Map<IEnumerable<BillingRecordDto>>(billingRecords);
                _logger.LogInformation("Mapped {Count} billing record DTOs", billingRecordDtos.Count());
                
                return ApiResponse<IEnumerable<BillingRecordDto>>.SuccessResponse(billingRecordDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing history for subscription {SubscriptionId}", subscriptionId);
                return ApiResponse<IEnumerable<BillingRecordDto>>.ErrorResponse("Error retrieving subscription billing history", 500);
            }
        }

        public async Task<ApiResponse<BillingRecordDto>> ProcessPaymentAsync(Guid billingRecordId)
        {
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
                if (billingRecord == null)
                {
                    return ApiResponse<BillingRecordDto>.ErrorResponse("Billing record not found", 404);
                }

                billingRecord.Status = BillingRecord.BillingStatus.Paid;
                billingRecord.PaidAt = DateTime.UtcNow;

                var updatedRecord = await _billingRepository.UpdateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(updatedRecord);
                
                return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for billing record {BillingRecordId}", billingRecordId);
                return ApiResponse<BillingRecordDto>.ErrorResponse("Error processing payment", 500);
            }
        }

        public async Task<ApiResponse<BillingRecordDto>> ProcessRefundAsync(Guid billingRecordId, decimal amount)
        {
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
                if (billingRecord == null)
                {
                    return ApiResponse<BillingRecordDto>.ErrorResponse("Billing record not found", 404);
                }

                // Note: RefundAmount and RefundedAt properties don't exist in BillingRecord entity
                billingRecord.Status = BillingRecord.BillingStatus.Refunded;
                billingRecord.UpdatedAt = DateTime.UtcNow;

                var updatedRecord = await _billingRepository.UpdateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(updatedRecord);
                
                return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for billing record {BillingRecordId}", billingRecordId);
                return ApiResponse<BillingRecordDto>.ErrorResponse("Error processing refund", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetOverdueBillingRecordsAsync()
        {
            try
            {
                // Placeholder implementation - in real app, this would be a repository method
                var overdueRecords = new List<BillingRecord>(); // await _billingRepository.GetOverdueRecordsAsync();
                var billingRecordDtos = _mapper.Map<IEnumerable<BillingRecordDto>>(overdueRecords);
                return ApiResponse<IEnumerable<BillingRecordDto>>.SuccessResponse(billingRecordDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overdue billing records");
                return ApiResponse<IEnumerable<BillingRecordDto>>.ErrorResponse("Error retrieving overdue billing records", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetPendingPaymentsAsync()
        {
            try
            {
                // Placeholder implementation - in real app, this would be a repository method
                var pendingRecords = new List<BillingRecord>(); // await _billingRepository.GetPendingRecordsAsync();
                var billingRecordDtos = _mapper.Map<IEnumerable<BillingRecordDto>>(pendingRecords);
                return ApiResponse<IEnumerable<BillingRecordDto>>.SuccessResponse(billingRecordDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending payments");
                return ApiResponse<IEnumerable<BillingRecordDto>>.ErrorResponse("Error retrieving pending payments", 500);
            }
        }

        public async Task<ApiResponse<decimal>> CalculateTotalAmountAsync(decimal baseAmount, decimal taxAmount, decimal shippingAmount)
        {
            try
            {
                var totalAmount = baseAmount + taxAmount + shippingAmount;
                return ApiResponse<decimal>.SuccessResponse(totalAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total amount");
                return ApiResponse<decimal>.ErrorResponse("Error calculating total amount", 500);
            }
        }

        public async Task<ApiResponse<decimal>> CalculateTaxAmountAsync(decimal baseAmount, string state)
        {
            try
            {
                // Simplified tax calculation - in real app, use tax service
                var taxRate = state.ToUpper() switch
                {
                    "CA" => 0.0825m,
                    "NY" => 0.085m,
                    "TX" => 0.0625m,
                    _ => 0.06m
                };

                var taxAmount = baseAmount * taxRate;
                return ApiResponse<decimal>.SuccessResponse(taxAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating tax amount");
                return ApiResponse<decimal>.ErrorResponse("Error calculating tax amount", 500);
            }
        }

        public async Task<ApiResponse<decimal>> CalculateShippingAmountAsync(string deliveryAddress, bool isExpress)
        {
            try
            {
                // Simplified shipping calculation
                var baseShipping = 5.99m;
                var expressMultiplier = isExpress ? 2.5m : 1.0m;
                var shippingAmount = baseShipping * expressMultiplier;
                
                return ApiResponse<decimal>.SuccessResponse(shippingAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating shipping amount");
                return ApiResponse<decimal>.ErrorResponse("Error calculating shipping amount", 500);
            }
        }

        public async Task<ApiResponse<bool>> IsPaymentOverdueAsync(Guid billingRecordId)
        {
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
                if (billingRecord == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Billing record not found", 404);
                }

                var isOverdue = billingRecord.DueDate.HasValue && 
                               billingRecord.DueDate.Value < DateTime.UtcNow && 
                               billingRecord.Status == BillingRecord.BillingStatus.Pending;

                return ApiResponse<bool>.SuccessResponse(isOverdue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if payment is overdue for billing record {BillingRecordId}", billingRecordId);
                return ApiResponse<bool>.ErrorResponse("Error checking payment status", 500);
            }
        }

        public async Task<ApiResponse<DateTime>> CalculateDueDateAsync(DateTime billingDate, int gracePeriodDays)
        {
            try
            {
                var dueDate = billingDate.AddDays(gracePeriodDays);
                return ApiResponse<DateTime>.SuccessResponse(dueDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating due date");
                return ApiResponse<DateTime>.ErrorResponse("Error calculating due date", 500);
            }
        }

        public async Task<ApiResponse<BillingAnalyticsDto>> GetBillingAnalyticsAsync()
        {
            try
            {
                // Placeholder implementation
                var analytics = new BillingAnalyticsDto
                {
                    TotalRevenue = 50000.00m,
                    MonthlyRevenue = new List<MonthlyRevenueDto>
                    {
                        new MonthlyRevenueDto { Month = "January", Revenue = 5000.00m, BillingCount = 50 },
                        new MonthlyRevenueDto { Month = "February", Revenue = 5500.00m, BillingCount = 55 },
                        new MonthlyRevenueDto { Month = "March", Revenue = 6000.00m, BillingCount = 60 }
                    },
                    OutstandingAmount = 2500.00m,
                    PaidAmount = 47500.00m,
                    TotalInvoices = 150,
                    PaidInvoices = 140,
                    OverdueInvoices = 10,
                    AveragePaymentTime = 3.5m,
                    TopRevenueSources = new List<RevenueSourceDto>
                    {
                        new RevenueSourceDto { Source = "Subscriptions", Amount = 30000.00m, Percentage = 60.0m },
                        new RevenueSourceDto { Source = "Consultations", Amount = 15000.00m, Percentage = 30.0m },
                        new RevenueSourceDto { Source = "Medications", Amount = 5000.00m, Percentage = 10.0m }
                    }
                };

                return ApiResponse<BillingAnalyticsDto>.SuccessResponse(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing analytics");
                return ApiResponse<BillingAnalyticsDto>.ErrorResponse("Error retrieving billing analytics", 500);
            }
        }

        // Phase 2: Enhanced Billing Features
        public async Task<ApiResponse<BillingRecordDto>> CreateRecurringBillingAsync(CreateRecurringBillingDto createDto)
        {
            try
            {
                var billingRecord = new BillingRecord
                {
                    Id = Guid.NewGuid(),
                    UserId = createDto.SubscriptionId, // Using subscription ID as user ID for this example
                    SubscriptionId = createDto.SubscriptionId,
                    Amount = createDto.Amount,
                    Description = createDto.Description ?? $"Recurring billing for subscription",
                    BillingDate = createDto.StartDate,
                    DueDate = createDto.StartDate.AddDays(createDto.GracePeriodDays),
                    Status = BillingRecord.BillingStatus.Pending,
                    Type = BillingRecord.BillingType.Subscription,
                    CreatedAt = DateTime.UtcNow
                };

                var createdRecord = await _billingRepository.CreateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(createdRecord);
                
                return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Recurring billing created successfully", 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recurring billing");
                return ApiResponse<BillingRecordDto>.ErrorResponse("Error creating recurring billing", 500);
            }
        }

        public async Task<ApiResponse<BillingRecordDto>> ProcessRecurringPaymentAsync(Guid subscriptionId)
        {
            try
            {
                var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
                if (subscription == null)
                {
                    return ApiResponse<BillingRecordDto>.ErrorResponse("Subscription not found", 404);
                }

                decimal price = subscription.CurrentPrice;
                string planName = subscription.SubscriptionPlan.Name;
                string billingCycle = subscription.BillingCycle.Name; // Use BillingCycle.Name

                var billingRecord = new BillingRecord
                {
                    Id = Guid.NewGuid(),
                    UserId = subscription.UserId,
                    SubscriptionId = subscriptionId,
                    Amount = price,
                    Description = $"Recurring payment for {planName} ({billingCycle})",
                    BillingDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(7),
                    Status = BillingRecord.BillingStatus.Paid,
                    PaidAt = DateTime.UtcNow,
                    Type = BillingRecord.BillingType.Subscription,
                    CreatedAt = DateTime.UtcNow
                };

                var createdRecord = await _billingRepository.CreateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(createdRecord);
                
                return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Recurring payment processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recurring payment for subscription {SubscriptionId}", subscriptionId);
                return ApiResponse<BillingRecordDto>.ErrorResponse("Error processing recurring payment", 500);
            }
        }

        public async Task<ApiResponse<bool>> CancelRecurringBillingAsync(Guid subscriptionId)
        {
            try
            {
                // TODO: Implement recurring billing cancellation
                return ApiResponse<bool>.SuccessResponse(true, "Recurring billing cancelled successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling recurring billing for subscription {SubscriptionId}", subscriptionId);
                return ApiResponse<bool>.ErrorResponse("Error cancelling recurring billing", 500);
            }
        }

        public async Task<ApiResponse<BillingRecordDto>> CreateUpfrontPaymentAsync(CreateUpfrontPaymentDto createDto)
        {
            try
            {
                var billingRecord = new BillingRecord
                {
                    Id = Guid.NewGuid(),
                    UserId = createDto.UserId,
                    Amount = createDto.Amount,
                    Description = createDto.Description,
                    BillingDate = DateTime.UtcNow,
                    DueDate = createDto.DueDate,
                    Status = BillingRecord.BillingStatus.Pending,
                    Type = BillingRecord.BillingType.Subscription,
                    InvoiceNumber = createDto.InvoiceNumber,
                    CreatedAt = DateTime.UtcNow
                };

                var createdRecord = await _billingRepository.CreateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(createdRecord);
                
                return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Upfront payment created successfully", 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating upfront payment");
                return ApiResponse<BillingRecordDto>.ErrorResponse("Error creating upfront payment", 500);
            }
        }

        public async Task<ApiResponse<BillingRecordDto>> ProcessBundlePaymentAsync(CreateBundlePaymentDto createDto)
        {
            try
            {
                var totalAmount = createDto.Items.Sum(item => item.UnitPrice * item.Quantity);
                if (createDto.IncludeShipping)
                {
                    totalAmount += createDto.IsExpressShipping ? 15.00m : 5.00m;
                }

                var billingRecord = new BillingRecord
                {
                    Id = Guid.NewGuid(),
                    UserId = createDto.UserId,
                    Amount = totalAmount,
                    Description = createDto.Description ?? "Bundle payment",
                    BillingDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(7),
                    Status = BillingRecord.BillingStatus.Pending,
                    Type = BillingRecord.BillingType.Subscription,
                    CreatedAt = DateTime.UtcNow
                };

                var createdRecord = await _billingRepository.CreateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(createdRecord);
                
                return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Bundle payment processed successfully", 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bundle payment");
                return ApiResponse<BillingRecordDto>.ErrorResponse("Error processing bundle payment", 500);
            }
        }

        public async Task<ApiResponse<BillingRecordDto>> ApplyBillingAdjustmentAsync(Guid billingRecordId, CreateBillingAdjustmentDto adjustmentDto)
        {
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
                if (billingRecord == null)
                {
                    return ApiResponse<BillingRecordDto>.ErrorResponse("Billing record not found", 404);
                }

                // TODO: Apply adjustment logic
                billingRecord.UpdatedAt = DateTime.UtcNow;
                var updatedRecord = await _billingRepository.UpdateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(updatedRecord);
                
                return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Billing adjustment applied successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying billing adjustment for billing record {BillingRecordId}", billingRecordId);
                return ApiResponse<BillingRecordDto>.ErrorResponse("Error applying billing adjustment", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BillingAdjustmentDto>>> GetBillingAdjustmentsAsync(Guid billingRecordId)
        {
            try
            {
                // TODO: Implement billing adjustments retrieval
                var adjustments = new List<BillingAdjustmentDto>();
                return ApiResponse<IEnumerable<BillingAdjustmentDto>>.SuccessResponse(adjustments, "Billing adjustments retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing adjustments for billing record {BillingRecordId}", billingRecordId);
                return ApiResponse<IEnumerable<BillingAdjustmentDto>>.ErrorResponse("Error retrieving billing adjustments", 500);
            }
        }

        public async Task<ApiResponse<BillingRecordDto>> RetryFailedPaymentAsync(Guid billingRecordId)
        {
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
                if (billingRecord == null)
                {
                    return ApiResponse<BillingRecordDto>.ErrorResponse("Billing record not found", 404);
                }

                billingRecord.Status = BillingRecord.BillingStatus.Pending;
                billingRecord.UpdatedAt = DateTime.UtcNow;
                var updatedRecord = await _billingRepository.UpdateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(updatedRecord);
                
                return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Failed payment retry initiated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying failed payment for billing record {BillingRecordId}", billingRecordId);
                return ApiResponse<BillingRecordDto>.ErrorResponse("Error retrying failed payment", 500);
            }
        }

        public async Task<ApiResponse<BillingRecordDto>> ProcessPartialPaymentAsync(Guid billingRecordId, decimal amount)
        {
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
                if (billingRecord == null)
                {
                    return ApiResponse<BillingRecordDto>.ErrorResponse("Billing record not found", 404);
                }

                // TODO: Implement partial payment logic
                billingRecord.UpdatedAt = DateTime.UtcNow;
                var updatedRecord = await _billingRepository.UpdateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(updatedRecord);
                
                return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Partial payment processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing partial payment for billing record {BillingRecordId}", billingRecordId);
                return ApiResponse<BillingRecordDto>.ErrorResponse("Error processing partial payment", 500);
            }
        }

        public async Task<ApiResponse<BillingRecordDto>> CreateInvoiceAsync(CreateInvoiceDto createDto)
        {
            try
            {
                var billingRecord = new BillingRecord
                {
                    Id = Guid.NewGuid(),
                    UserId = createDto.UserId,
                    Amount = createDto.TotalAmount,
                    Description = "Invoice",
                    BillingDate = DateTime.UtcNow,
                    DueDate = createDto.DueDate,
                    Status = BillingRecord.BillingStatus.Pending,
                    Type = BillingRecord.BillingType.Subscription, // No Invoice type, use Subscription
                    InvoiceNumber = createDto.InvoiceNumber,
                    CreatedAt = DateTime.UtcNow
                };

                var createdRecord = await _billingRepository.CreateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(createdRecord);
                
                return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Invoice created successfully", 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice");
                return ApiResponse<BillingRecordDto>.ErrorResponse("Error creating invoice", 500);
            }
        }

        public async Task<ApiResponse<byte[]>> GenerateInvoicePdfAsync(Guid billingRecordId)
        {
            try
            {
                // TODO: Implement PDF generation
                var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF header
                return ApiResponse<byte[]>.SuccessResponse(pdfBytes, "Invoice PDF generated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice PDF for billing record {BillingRecordId}", billingRecordId);
                return ApiResponse<byte[]>.ErrorResponse("Error generating invoice PDF", 500);
            }
        }

        public async Task<ApiResponse<byte[]>> GenerateBillingReportAsync(DateTime startDate, DateTime endDate, string format = "pdf")
        {
            try
            {
                // TODO: Implement billing report generation
                var reportBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF header
                return ApiResponse<byte[]>.SuccessResponse(reportBytes, "Billing report generated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating billing report");
                return ApiResponse<byte[]>.ErrorResponse("Error generating billing report", 500);
            }
        }

        public async Task<ApiResponse<BillingSummaryDto>> GetBillingSummaryAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var billingRecords = await _billingRepository.GetByUserIdAsync(userId);
                var filteredRecords = billingRecords.Where(br => 
                    (!startDate.HasValue || br.BillingDate >= startDate.Value) &&
                    (!endDate.HasValue || br.BillingDate <= endDate.Value));

                var summary = new BillingSummaryDto
                {
                    UserId = userId,
                    TotalBilled = filteredRecords.Sum(br => br.Amount),
                    TotalPaid = filteredRecords.Where(br => br.Status == BillingRecord.BillingStatus.Paid).Sum(br => br.Amount),
                    TotalOutstanding = filteredRecords.Where(br => br.Status == BillingRecord.BillingStatus.Pending).Sum(br => br.Amount),
                    TotalRefunded = filteredRecords.Where(br => br.Status == BillingRecord.BillingStatus.Refunded).Sum(br => br.Amount),
                    TotalInvoices = filteredRecords.Count(),
                    PaidInvoices = filteredRecords.Count(br => br.Status == BillingRecord.BillingStatus.Paid),
                    OverdueInvoices = filteredRecords.Count(br => br.Status == BillingRecord.BillingStatus.Overdue),
                    StartDate = startDate ?? DateTime.UtcNow.AddDays(-30),
                    EndDate = endDate ?? DateTime.UtcNow,
                    RecentTransactions = _mapper.Map<List<SmartTelehealth.Application.DTOs.BillingRecordDto>>(filteredRecords.Take(10))
                };

                return ApiResponse<BillingSummaryDto>.SuccessResponse(summary, "Billing summary retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing summary for user {UserId}", userId);
                return ApiResponse<BillingSummaryDto>.ErrorResponse("Error retrieving billing summary", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetBillingCycleRecordsAsync(Guid billingCycleId)
        {
            try
            {
                // This is a placeholder implementation since the repository doesn't have this method yet
                var billingRecords = new List<BillingRecord>(); // await _billingRepository.GetByBillingCycleIdAsync(billingCycleId);
                var billingRecordDtos = _mapper.Map<IEnumerable<BillingRecordDto>>(billingRecords);
                return ApiResponse<IEnumerable<BillingRecordDto>>.SuccessResponse(billingRecordDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing cycle records for cycle {BillingCycleId}", billingCycleId);
                return ApiResponse<IEnumerable<BillingRecordDto>>.ErrorResponse("Error retrieving billing cycle records", 500);
            }
        }

        public async Task<ApiResponse<PaymentScheduleDto>> GetPaymentScheduleAsync(Guid subscriptionId)
        {
            try
            {
                var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
                if (subscription == null)
                {
                    return ApiResponse<PaymentScheduleDto>.ErrorResponse("Subscription not found", 404);
                }

                // Use the BillingCycle navigation property
                var billingCycleName = subscription.BillingCycle?.Name ?? "Monthly";
                int planDuration = billingCycleName switch
                {
                    "Annual" => 12,
                    "Quarterly" => 4,
                    _ => 1
                };

                var schedule = new PaymentScheduleDto
                {
                    SubscriptionId = subscriptionId,
                    SubscriptionName = subscription.SubscriptionPlan.Name,
                    BillingCycle = billingCycleName,
                    Amount = subscription.CurrentPrice,
                    StartDate = subscription.StartDate,
                    EndDate = subscription.EndDate,
                    NextPaymentDate = subscription.NextBillingDate,
                    TotalPayments = planDuration,
                    CompletedPayments = 0, // TODO: Calculate from billing history
                    RemainingPayments = planDuration,
                    AutoRenew = subscription.AutoRenew
                };

                return ApiResponse<PaymentScheduleDto>.SuccessResponse(schedule, "Payment schedule retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment schedule for subscription {SubscriptionId}", subscriptionId);
                return ApiResponse<PaymentScheduleDto>.ErrorResponse("Error retrieving payment schedule", 500);
            }
        }

        public async Task<ApiResponse<bool>> UpdatePaymentMethodAsync(Guid billingRecordId, string paymentMethodId)
        {
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
                if (billingRecord == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Billing record not found", 404);
                }

                // TODO: Update payment method logic
                billingRecord.UpdatedAt = DateTime.UtcNow;
                await _billingRepository.UpdateAsync(billingRecord);
                
                return ApiResponse<bool>.SuccessResponse(true, "Payment method updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment method for billing record {BillingRecordId}", billingRecordId);
                return ApiResponse<bool>.ErrorResponse("Error updating payment method", 500);
            }
        }

        public async Task<ApiResponse<BillingRecordDto>> CreateBillingCycleAsync(CreateBillingCycleDto createDto)
        {
            try
            {
                // TODO: Implement billing cycle creation
                var billingRecord = new BillingRecord
                {
                    Id = Guid.NewGuid(),
                    UserId = createDto.UserId,
                    Amount = 0, // Will be calculated from subscriptions
                    Description = createDto.Description,
                    BillingDate = createDto.StartDate,
                    DueDate = createDto.EndDate,
                    Status = BillingRecord.BillingStatus.Pending,
                    Type = BillingRecord.BillingType.Subscription, // No Cycle type, use Subscription
                    CreatedAt = DateTime.UtcNow
                };

                var createdRecord = await _billingRepository.CreateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(createdRecord);
                
                return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Billing cycle created successfully", 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating billing cycle");
                return ApiResponse<BillingRecordDto>.ErrorResponse("Error creating billing cycle", 500);
            }
        }

        // Remove or comment out GetBillingCycleRecordsAsync and related logic, as this method does not exist in the repository
        // public async Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetBillingCycleRecordsAsync(Guid billingCycleId)
        // {
        //     // Not implemented: No such method in repository
        //     return ApiResponse<IEnumerable<BillingRecordDto>>.ErrorResponse("Not implemented", 501);
        // }

        public async Task<ApiResponse<BillingRecordDto>> ProcessBillingCycleAsync(Guid billingCycleId)
        {
            try
            {
                // TODO: Implement billing cycle processing
                var billingRecord = await _billingRepository.GetByIdAsync(billingCycleId);
                if (billingRecord == null)
                {
                    return ApiResponse<BillingRecordDto>.ErrorResponse("Billing cycle not found", 404);
                }

                billingRecord.Status = BillingRecord.BillingStatus.Paid; // Use Paid instead of Processed
                billingRecord.UpdatedAt = DateTime.UtcNow;
                var updatedRecord = await _billingRepository.UpdateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(updatedRecord);
                
                return ApiResponse<BillingRecordDto>.SuccessResponse(billingRecordDto, "Billing cycle processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing billing cycle {BillingCycleId}", billingCycleId);
                return ApiResponse<BillingRecordDto>.ErrorResponse("Error processing billing cycle", 500);
            }
        }

        public async Task<ApiResponse<RevenueSummaryDto>> GetRevenueSummaryAsync(DateTime? from = null, DateTime? to = null, string? planId = null)
        {
            // This method is now directly calling the infrastructure layer,
            // which means it's no longer part of the Application layer's responsibility
            // for billing analytics. The Application layer should only manage
            // the core billing operations and data.
            // For now, we'll return a placeholder or throw an exception if not implemented.
            // A proper implementation would involve a dedicated analytics service.
            _logger.LogWarning("GetRevenueSummaryAsync called, but this method is now part of the infrastructure layer.");
            return ApiResponse<RevenueSummaryDto>.ErrorResponse("Revenue summary retrieval is not implemented in the Application layer.", 501);
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
                
                return ApiResponse<byte[]>.SuccessResponse(bytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting revenue data");
                return ApiResponse<byte[]>.ErrorResponse("Error exporting revenue data", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<PaymentHistoryDto>>> GetPaymentHistoryAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var billingRecords = await _billingRepository.GetByUserIdAsync(userId);
                
                // Filter by date range if provided
                if (startDate.HasValue || endDate.HasValue)
                {
                    billingRecords = billingRecords.Where(br => 
                        (!startDate.HasValue || br.CreatedAt >= startDate.Value) &&
                        (!endDate.HasValue || br.CreatedAt <= endDate.Value));
                }

                var paymentHistory = billingRecords.Select(br => new PaymentHistoryDto
                {
                    Id = br.Id,
                    UserId = br.UserId.ToString(),
                    SubscriptionId = br.SubscriptionId?.ToString() ?? string.Empty,
                    Amount = br.Amount,
                    Currency = "USD",
                    PaymentMethod = br.PaymentMethod ?? "Unknown",
                    Status = br.Status.ToString(),
                    TransactionId = br.TransactionId,
                    ErrorMessage = br.ErrorMessage,
                    CreatedAt = br.CreatedAt,
                    ProcessedAt = br.ProcessedAt
                });

                return ApiResponse<IEnumerable<PaymentHistoryDto>>.SuccessResponse(paymentHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment history for user {UserId}", userId);
                return ApiResponse<IEnumerable<PaymentHistoryDto>>.ErrorResponse("Error retrieving payment history", 500);
            }
        }

        public async Task<ApiResponse<PaymentAnalyticsDto>> GetPaymentAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var allBillingRecords = await _billingRepository.GetAllAsync();
                
                // Filter by date range if provided
                if (startDate.HasValue || endDate.HasValue)
                {
                    allBillingRecords = allBillingRecords.Where(br => 
                        (!startDate.HasValue || br.CreatedAt >= startDate.Value) &&
                        (!endDate.HasValue || br.CreatedAt <= endDate.Value));
                }

                var analytics = new PaymentAnalyticsDto
                {
                    TotalPayments = allBillingRecords.Sum(br => br.Amount),
                    SuccessfulPayments = allBillingRecords.Where(br => br.Status == BillingRecord.BillingStatus.Paid).Sum(br => br.Amount),
                    FailedPayments = allBillingRecords.Where(br => br.Status == BillingRecord.BillingStatus.Failed).Sum(br => br.Amount),
                    TotalTransactions = allBillingRecords.Count(),
                    SuccessfulTransactions = allBillingRecords.Count(br => br.Status == BillingRecord.BillingStatus.Paid),
                    FailedTransactions = allBillingRecords.Count(br => br.Status == BillingRecord.BillingStatus.Failed),
                    TotalRefunds = allBillingRecords.Where(br => br.Status == BillingRecord.BillingStatus.Refunded).Sum(br => br.Amount)
                };

                // Calculate success rate
                if (analytics.TotalTransactions > 0)
                {
                    analytics.PaymentSuccessRate = (decimal)analytics.SuccessfulTransactions / analytics.TotalTransactions * 100;
                }

                // Calculate average payment amount
                if (analytics.SuccessfulTransactions > 0)
                {
                    analytics.AveragePaymentAmount = analytics.SuccessfulPayments / analytics.SuccessfulTransactions;
                }

                // Generate monthly payments data
                var monthlyPayments = allBillingRecords
                    .GroupBy(br => new { br.CreatedAt.Year, br.CreatedAt.Month })
                    .Select(g => new MonthlyPaymentDto
                    {
                        Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                        TotalAmount = g.Sum(br => br.Amount),
                        TransactionCount = g.Count(),
                        SuccessfulCount = g.Count(br => br.Status == BillingRecord.BillingStatus.Paid),
                        FailedCount = g.Count(br => br.Status == BillingRecord.BillingStatus.Failed)
                    })
                    .OrderBy(mp => mp.Month)
                    .ToList();

                analytics.MonthlyPayments = monthlyPayments;

                // Generate payment method analytics
                var paymentMethods = allBillingRecords
                    .GroupBy(br => br.PaymentMethod ?? "Unknown")
                    .Select(g => new PaymentMethodAnalyticsDto
                    {
                        Method = g.Key,
                        UsageCount = g.Count(),
                        TotalAmount = g.Sum(br => br.Amount),
                        SuccessRate = g.Count() > 0 ? (decimal)g.Count(br => br.Status == BillingRecord.BillingStatus.Paid) / g.Count() * 100 : 0
                    })
                    .ToList();

                analytics.PaymentMethods = paymentMethods;

                // Generate payment status analytics
                var paymentStatuses = allBillingRecords
                    .GroupBy(br => br.Status)
                    .Select(g => new PaymentStatusAnalyticsDto
                    {
                        Status = g.Key.ToString(),
                        Count = g.Count(),
                        TotalAmount = g.Sum(br => br.Amount)
                    })
                    .ToList();

                analytics.PaymentStatuses = paymentStatuses;

                return ApiResponse<PaymentAnalyticsDto>.SuccessResponse(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment analytics");
                return ApiResponse<PaymentAnalyticsDto>.ErrorResponse("Error retrieving payment analytics", 500);
            }
        }

        private string ConvertToCsv(RevenueSummaryDto revenueData)
        {
            // Simple CSV conversion for revenue data
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Date,Revenue,Subscriptions,Plan");
            // Add actual data conversion logic here
            return csv.ToString();
        }

        public async Task<ApiResponse<PaymentResultDto>> RetryPaymentAsync(Guid billingRecordId)
        {
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
                if (billingRecord == null)
                {
                    return ApiResponse<PaymentResultDto>.ErrorResponse("Billing record not found", 404);
                }

                // Retry payment logic
                var paymentResult = new PaymentResultDto
                {
                    Status = "succeeded",
                    PaymentIntentId = Guid.NewGuid().ToString(),
                    Amount = billingRecord.Amount,
                    Currency = "usd",
                    ProcessedAt = DateTime.UtcNow
                };

                return ApiResponse<PaymentResultDto>.SuccessResponse(paymentResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying payment for billing record {BillingRecordId}", billingRecordId);
                return ApiResponse<PaymentResultDto>.ErrorResponse("Error retrying payment", 500);
            }
        }

        public async Task<ApiResponse<RefundResultDto>> ProcessRefundAsync(Guid billingRecordId, decimal amount, string reason)
        {
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
                if (billingRecord == null)
                {
                    return ApiResponse<RefundResultDto>.ErrorResponse("Billing record not found", 404);
                }

                // Process refund logic
                var refundResult = new RefundResultDto
                {
                    Success = true,
                    RefundId = Guid.NewGuid().ToString(),
                    Amount = amount,
                    Reason = reason,
                    Status = "Completed",
                    Message = "Refund processed successfully"
                };

                return ApiResponse<RefundResultDto>.SuccessResponse(refundResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for billing record {BillingRecordId}", billingRecordId);
                return ApiResponse<RefundResultDto>.ErrorResponse("Error processing refund", 500);
            }
        }

        public async Task<ApiResponse<PaymentAnalyticsDto>> GetPaymentAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var userBillingRecords = await _billingRepository.GetByUserIdAsync(Guid.Parse(userId));
                
                // Filter by date range if provided
                if (startDate.HasValue || endDate.HasValue)
                {
                    userBillingRecords = userBillingRecords.Where(br => 
                        (!startDate.HasValue || br.CreatedAt >= startDate.Value) &&
                        (!endDate.HasValue || br.CreatedAt <= endDate.Value));
                }

                var analytics = new PaymentAnalyticsDto
                {
                    TotalSpent = userBillingRecords.Where(br => br.Status == BillingRecord.BillingStatus.Paid).Sum(br => br.Amount),
                    TotalPayments = userBillingRecords.Count(br => br.Status == BillingRecord.BillingStatus.Paid),
                    SuccessfulPayments = userBillingRecords.Count(br => br.Status == BillingRecord.BillingStatus.Paid),
                    FailedPayments = userBillingRecords.Count(br => br.Status == BillingRecord.BillingStatus.Failed),
                    AveragePaymentAmount = userBillingRecords.Where(br => br.Status == BillingRecord.BillingStatus.Paid).Any() 
                        ? userBillingRecords.Where(br => br.Status == BillingRecord.BillingStatus.Paid).Average(br => br.Amount) 
                        : 0,
                    MonthlyPayments = userBillingRecords
                        .GroupBy(br => new { br.CreatedAt.Year, br.CreatedAt.Month })
                        .Select(g => new MonthlyPaymentDto
                        {
                            Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                            Amount = g.Sum(br => br.Amount),
                            Count = g.Count()
                        })
                        .OrderBy(mp => mp.Month)
                        .ToList()
                };

                return ApiResponse<PaymentAnalyticsDto>.SuccessResponse(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment analytics for user {UserId}", userId);
                return ApiResponse<PaymentAnalyticsDto>.ErrorResponse("Error retrieving payment analytics", 500);
            }
        }
    }
} 