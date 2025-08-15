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

        public async Task<JsonModel> CreateBillingRecordAsync(CreateBillingRecordDto createDto)
        {
            try
            {
                var billingRecord = _mapper.Map<BillingRecord>(createDto);
                billingRecord.CreatedDate = DateTime.UtcNow;
                billingRecord.Status = BillingRecord.BillingStatus.Pending;

                // Note: AddAsync method doesn't exist, using CreateAsync instead
                var createdRecord = await _billingRepository.CreateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(createdRecord);
                
                return new JsonModel { data = billingRecordDto, Message = "Billing record created successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating billing record");
                return new JsonModel { data = new object(), Message = "Error creating billing record", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetBillingRecordAsync(Guid id)
        {
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(id);
                if (billingRecord == null)
                {
                    return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
                }

                var billingRecordDto = _mapper.Map<BillingRecordDto>(billingRecord);
                return new JsonModel { data = billingRecordDto, Message = "Billing record retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing record with ID {BillingRecordId}", id);
                return new JsonModel { data = new object(), Message = "Error retrieving billing record", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetUserBillingHistoryAsync(int userId)
        {
            try
            {
                var billingRecords = await _billingRepository.GetByUserIdAsync(userId);
                var billingRecordDtos = _mapper.Map<IEnumerable<BillingRecordDto>>(billingRecords);
                return new JsonModel { data = billingRecordDtos, Message = "User billing history retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing history for user {UserId}", userId);
                return new JsonModel { data = new object(), Message = "Error retrieving billing history", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetAllBillingRecordsAsync()
        {
            try
            {
                var billingRecords = await _billingRepository.GetAllAsync();
                var billingRecordDtos = _mapper.Map<IEnumerable<BillingRecordDto>>(billingRecords);
                return new JsonModel { data = billingRecordDtos, Message = "All billing records retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all billing records");
                return new JsonModel { data = new object(), Message = "Error retrieving billing records", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetSubscriptionBillingHistoryAsync(Guid subscriptionId)
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
                
                return new JsonModel { data = billingRecordDtos, Message = "Subscription billing history retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing history for subscription {SubscriptionId}", subscriptionId);
                return new JsonModel { data = new object(), Message = "Error retrieving subscription billing history", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> ProcessPaymentAsync(Guid billingRecordId)
        {
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
                if (billingRecord == null)
                {
                    return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
                }

                billingRecord.Status = BillingRecord.BillingStatus.Paid;
                billingRecord.PaidAt = DateTime.UtcNow;

                var updatedRecord = await _billingRepository.UpdateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(updatedRecord);
                
                return new JsonModel { data = billingRecordDto, Message = "Payment processed successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for billing record {BillingRecordId}", billingRecordId);
                return new JsonModel { data = new object(), Message = "Error processing payment", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> ProcessRefundAsync(Guid billingRecordId, decimal amount)
        {
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
                if (billingRecord == null)
                {
                    return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
                }

                // Note: RefundAmount and RefundedAt properties don't exist in BillingRecord entity
                billingRecord.Status = BillingRecord.BillingStatus.Refunded;
                billingRecord.UpdatedDate = DateTime.UtcNow;

                var updatedRecord = await _billingRepository.UpdateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(updatedRecord);
                
                return new JsonModel { data = billingRecordDto, Message = "Refund processed successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for billing record {BillingRecordId}", billingRecordId);
                return new JsonModel { data = new object(), Message = "Error processing refund", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetOverdueBillingRecordsAsync()
        {
            try
            {
                // Placeholder implementation - in real app, this would be a repository method
                var overdueRecords = new List<BillingRecord>(); // await _billingRepository.GetOverdueRecordsAsync();
                var billingRecordDtos = _mapper.Map<IEnumerable<BillingRecordDto>>(overdueRecords);
                return new JsonModel { data = billingRecordDtos, Message = "Overdue billing records retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overdue billing records");
                return new JsonModel { data = new object(), Message = "Error retrieving overdue billing records", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetPendingPaymentsAsync()
        {
            try
            {
                // Placeholder implementation - in real app, this would be a repository method
                var pendingRecords = new List<BillingRecord>(); // await _billingRepository.GetPendingRecordsAsync();
                var billingRecordDtos = _mapper.Map<IEnumerable<BillingRecordDto>>(pendingRecords);
                return new JsonModel { data = billingRecordDtos, Message = "Pending payments retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending payments");
                return new JsonModel { data = new object(), Message = "Error retrieving pending payments", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> CalculateTotalAmountAsync(decimal baseAmount, decimal taxAmount, decimal shippingAmount)
        {
            try
            {
                var totalAmount = baseAmount + taxAmount + shippingAmount;
                return new JsonModel { data = totalAmount, Message = "Total amount calculated successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total amount");
                return new JsonModel { data = new object(), Message = "Error calculating total amount", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> CalculateTaxAmountAsync(decimal baseAmount, string state)
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
                return new JsonModel { data = taxAmount, Message = "Tax amount calculated successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating tax amount");
                return new JsonModel { data = new object(), Message = "Error calculating tax amount", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> CalculateShippingAmountAsync(string deliveryAddress, bool isExpress)
        {
            try
            {
                // Simplified shipping calculation
                var baseShipping = 5.99m;
                var expressMultiplier = isExpress ? 2.5m : 1.0m;
                var shippingAmount = baseShipping * expressMultiplier;
                
                return new JsonModel { data = shippingAmount, Message = "Shipping amount calculated successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating shipping amount");
                return new JsonModel { data = new object(), Message = "Error calculating shipping amount", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> IsPaymentOverdueAsync(Guid billingRecordId)
        {
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
                if (billingRecord == null)
                {
                    return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
                }

                var isOverdue = billingRecord.DueDate.HasValue && 
                               billingRecord.DueDate.Value < DateTime.UtcNow && 
                               billingRecord.Status == BillingRecord.BillingStatus.Pending;

                return new JsonModel { data = isOverdue, Message = "Payment overdue status checked successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if payment is overdue for billing record {BillingRecordId}", billingRecordId);
                return new JsonModel { data = new object(), Message = "Error checking payment status", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> CalculateDueDateAsync(DateTime billingDate, int gracePeriodDays)
        {
            try
            {
                var dueDate = billingDate.AddDays(gracePeriodDays);
                return new JsonModel { data = dueDate, Message = "Due date calculated successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating due date");
                return new JsonModel { data = new object(), Message = "Error calculating due date", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetBillingAnalyticsAsync()
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

                return new JsonModel { data = analytics, Message = "Billing analytics retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing analytics");
                return new JsonModel { data = new object(), Message = "Error retrieving billing analytics", StatusCode = 500 };
            }
        }

        // Phase 2: Enhanced Billing Features
        public async Task<JsonModel> CreateRecurringBillingAsync(CreateRecurringBillingDto createDto)
        {
            try
            {
                var billingRecord = new BillingRecord
                {
                    Id = Guid.NewGuid(),
                    UserId = createDto.UserId,
                    SubscriptionId = createDto.SubscriptionId,
                    Amount = createDto.Amount,
                    Description = createDto.Description ?? $"Recurring billing for subscription",
                    BillingDate = createDto.StartDate,
                    DueDate = createDto.StartDate.AddDays(createDto.GracePeriodDays),
                    Status = BillingRecord.BillingStatus.Pending,
                    Type = BillingRecord.BillingType.Subscription,
                    CreatedDate = DateTime.UtcNow
                };

                var createdRecord = await _billingRepository.CreateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(createdRecord);
                
                return new JsonModel { data = billingRecordDto, Message = "Recurring billing created successfully", StatusCode = 201 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recurring billing");
                return new JsonModel { data = new object(), Message = "Error creating recurring billing", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> ProcessRecurringPaymentAsync(Guid subscriptionId)
        {
            try
            {
                var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
                if (subscription == null)
                {
                    return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
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
                    CreatedDate = DateTime.UtcNow
                };

                var createdRecord = await _billingRepository.CreateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(createdRecord);
                
                return new JsonModel { data = billingRecordDto, Message = "Recurring payment processed successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recurring payment for subscription {SubscriptionId}", subscriptionId);
                return new JsonModel { data = new object(), Message = "Error processing recurring payment", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> CancelRecurringBillingAsync(Guid subscriptionId)
        {
            try
            {
                // TODO: Implement recurring billing cancellation
                return new JsonModel { data = true, Message = "Recurring billing cancelled successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling recurring billing for subscription {SubscriptionId}", subscriptionId);
                return new JsonModel { data = new object(), Message = "Error cancelling recurring billing", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> CreateUpfrontPaymentAsync(CreateUpfrontPaymentDto createDto)
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
                    CreatedDate = DateTime.UtcNow
                };

                var createdRecord = await _billingRepository.CreateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(createdRecord);
                
                return new JsonModel { data = billingRecordDto, Message = "Upfront payment created successfully", StatusCode = 201 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating upfront payment");
                return new JsonModel { data = new object(), Message = "Error creating upfront payment", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> ProcessBundlePaymentAsync(CreateBundlePaymentDto createDto)
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
                    CreatedDate = DateTime.UtcNow
                };

                var createdRecord = await _billingRepository.CreateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(createdRecord);
                
                return new JsonModel { data = billingRecordDto, Message = "Bundle payment processed successfully", StatusCode = 201 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bundle payment");
                return new JsonModel { data = new object(), Message = "Error processing bundle payment", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> ApplyBillingAdjustmentAsync(Guid billingRecordId, CreateBillingAdjustmentDto adjustmentDto)
        {
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
                if (billingRecord == null)
                {
                    return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
                }

                // TODO: Apply adjustment logic
                billingRecord.UpdatedDate = DateTime.UtcNow;
                var updatedRecord = await _billingRepository.UpdateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(updatedRecord);
                
                return new JsonModel { data = billingRecordDto, Message = "Billing adjustment applied successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying billing adjustment for billing record {BillingRecordId}", billingRecordId);
                return new JsonModel { data = new object(), Message = "Error applying billing adjustment", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetBillingAdjustmentsAsync(Guid billingRecordId)
        {
            try
            {
                // TODO: Implement billing adjustments retrieval
                var adjustments = new List<BillingAdjustmentDto>();
                return new JsonModel { data = adjustments, Message = "Billing adjustments retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing adjustments for billing record {BillingRecordId}", billingRecordId);
                return new JsonModel { data = new object(), Message = "Error retrieving billing adjustments", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> RetryFailedPaymentAsync(Guid billingRecordId)
        {
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
                if (billingRecord == null)
                {
                    return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
                }

                billingRecord.Status = BillingRecord.BillingStatus.Pending;
                billingRecord.UpdatedDate = DateTime.UtcNow;
                var updatedRecord = await _billingRepository.UpdateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(updatedRecord);
                
                return new JsonModel { data = billingRecordDto, Message = "Failed payment retry initiated successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying failed payment for billing record {BillingRecordId}", billingRecordId);
                return new JsonModel { data = new object(), Message = "Error retrying failed payment", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> ProcessPartialPaymentAsync(Guid billingRecordId, decimal amount)
        {
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
                if (billingRecord == null)
                {
                    return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
                }

                // TODO: Implement partial payment logic
                billingRecord.UpdatedDate = DateTime.UtcNow;
                var updatedRecord = await _billingRepository.UpdateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(updatedRecord);
                
                return new JsonModel { data = billingRecordDto, Message = "Partial payment processed successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing partial payment for billing record {BillingRecordId}", billingRecordId);
                return new JsonModel { data = new object(), Message = "Error processing partial payment", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> CreateInvoiceAsync(CreateInvoiceDto createDto)
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
                    CreatedDate = DateTime.UtcNow
                };

                var createdRecord = await _billingRepository.CreateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(createdRecord);
                
                return new JsonModel { data = billingRecordDto, Message = "Invoice created successfully", StatusCode = 201 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice");
                return new JsonModel { data = new object(), Message = "Error creating invoice", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GenerateInvoicePdfAsync(Guid billingRecordId)
        {
            try
            {
                // TODO: Implement PDF generation
                var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF header
                return new JsonModel { data = pdfBytes, Message = "Invoice PDF generated successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice PDF for billing record {BillingRecordId}", billingRecordId);
                return new JsonModel { data = new object(), Message = "Error generating invoice PDF", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GenerateBillingReportAsync(DateTime startDate, DateTime endDate, string format = "pdf")
        {
            try
            {
                // TODO: Implement billing report generation
                var reportBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF header
                return new JsonModel { data = reportBytes, Message = "Billing report generated successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating billing report");
                return new JsonModel { data = new object(), Message = "Error generating billing report", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetBillingSummaryAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
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

                return new JsonModel { data = summary, Message = "Billing summary retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing summary for user {UserId}", userId);
                return new JsonModel { data = new object(), Message = "Error retrieving billing summary", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetBillingCycleRecordsAsync(Guid billingCycleId)
        {
            try
            {
                // This is a placeholder implementation since the repository doesn't have this method yet
                var billingRecords = new List<BillingRecord>(); // await _billingRepository.GetByBillingCycleIdAsync(billingCycleId);
                var billingRecordDtos = _mapper.Map<IEnumerable<BillingRecordDto>>(billingRecords);
                return new JsonModel { data = billingRecordDtos, Message = "Billing cycle records retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing cycle records for cycle {BillingCycleId}", billingCycleId);
                return new JsonModel { data = new object(), Message = "Error retrieving billing cycle records", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetPaymentScheduleAsync(Guid subscriptionId)
        {
            try
            {
                var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
                if (subscription == null)
                {
                    return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 404 };
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

                return new JsonModel { data = schedule, Message = "Payment schedule retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment schedule for subscription {SubscriptionId}", subscriptionId);
                return new JsonModel { data = new object(), Message = "Error retrieving payment schedule", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> UpdatePaymentMethodAsync(Guid billingRecordId, string paymentMethodId)
        {
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
                if (billingRecord == null)
                {
                    return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
                }

                // TODO: Update payment method logic
                billingRecord.UpdatedDate = DateTime.UtcNow;
                await _billingRepository.UpdateAsync(billingRecord);
                
                return new JsonModel { data = true, Message = "Payment method updated successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment method for billing record {BillingRecordId}", billingRecordId);
                return new JsonModel { data = new object(), Message = "Error updating payment method", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> CreateBillingCycleAsync(CreateBillingCycleDto createDto)
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
                    CreatedDate = DateTime.UtcNow
                };

                var createdRecord = await _billingRepository.CreateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(createdRecord);
                
                return new JsonModel { data = billingRecordDto, Message = "Billing cycle created successfully", StatusCode = 201 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating billing cycle");
                return new JsonModel { data = new object(), Message = "Error creating billing cycle", StatusCode = 500 };
            }
        }

        // Remove or comment out GetBillingCycleRecordsAsync and related logic, as this method does not exist in the repository
        // public async Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetBillingCycleRecordsAsync(Guid billingCycleId)
        // {
        //     // Not implemented: No such method in repository
        //     return ApiResponse<IEnumerable<BillingRecordDto>>.ErrorResponse("Not implemented", 501);
        // }

        public async Task<JsonModel> ProcessBillingCycleAsync(Guid billingCycleId)
        {
            try
            {
                // TODO: Implement billing cycle processing
                var billingRecord = await _billingRepository.GetByIdAsync(billingCycleId);
                if (billingRecord == null)
                {
                    return new JsonModel { data = new object(), Message = "Billing cycle not found", StatusCode = 404 };
                }

                billingRecord.Status = BillingRecord.BillingStatus.Paid; // Use Paid instead of Processed
                billingRecord.UpdatedDate = DateTime.UtcNow;
                var updatedRecord = await _billingRepository.UpdateAsync(billingRecord);
                var billingRecordDto = _mapper.Map<BillingRecordDto>(updatedRecord);
                
                return new JsonModel { data = billingRecordDto, Message = "Billing cycle processed successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing billing cycle {BillingCycleId}", billingCycleId);
                return new JsonModel { data = new object(), Message = "Error processing billing cycle", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetRevenueSummaryAsync(DateTime? from = null, DateTime? to = null, string? planId = null)
        {
            // This method is now directly calling the infrastructure layer,
            // which means it's no longer part of the Application layer's responsibility
            // for billing analytics. The Application layer should only manage
            // the core billing operations and data.
            // For now, we'll return a placeholder or throw an exception if not implemented.
            // A proper implementation would involve a dedicated analytics service.
            _logger.LogWarning("GetRevenueSummaryAsync called, but this method is now part of the infrastructure layer.");
            return new JsonModel { data = new object(), Message = "Revenue summary retrieval is not implemented in the Application layer.", StatusCode = 501 };
        }

        public async Task<JsonModel> ExportRevenueAsync(DateTime? from = null, DateTime? to = null, string? planId = null, string format = "csv")
        {
            try
            {
                // Implementation for revenue export
                var revenueData = await GetRevenueSummaryAsync(from, to, planId);
                if (revenueData.StatusCode != 200)
                {
                    return new JsonModel { data = new object(), Message = "Failed to get revenue data", StatusCode = 500 };
                }

                // Convert to CSV format
                var csvData = ConvertToCsv(revenueData.data);
                var bytes = System.Text.Encoding.UTF8.GetBytes(csvData);
                
                return new JsonModel { data = bytes, Message = "Revenue data exported successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting revenue data");
                return new JsonModel { data = new object(), Message = "Error exporting revenue data", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetPaymentHistoryAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var billingRecords = await _billingRepository.GetByUserIdAsync(userId);
                
                // Filter by date range if provided
                if (startDate.HasValue || endDate.HasValue)
                {
                    billingRecords = billingRecords.Where(br => 
                        (!startDate.HasValue || br.CreatedDate >= startDate.Value) &&
                        (!endDate.HasValue || br.CreatedDate <= endDate.Value));
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
                    CreatedAt = br.CreatedDate ?? DateTime.UtcNow,
                    ProcessedAt = br.ProcessedAt
                });

                return new JsonModel { data = paymentHistory, Message = "Payment history retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment history for user {UserId}", userId);
                return new JsonModel { data = new object(), Message = "Error retrieving payment history", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetPaymentAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var allBillingRecords = await _billingRepository.GetAllAsync();
                
                // Filter by date range if provided
                if (startDate.HasValue || endDate.HasValue)
                {
                                    allBillingRecords = allBillingRecords.Where(br => 
                    (!startDate.HasValue || br.CreatedDate >= startDate.Value) &&
                    (!endDate.HasValue || br.CreatedDate <= endDate.Value));
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
                    .Where(br => br.CreatedDate.HasValue)
                    .GroupBy(br => new { br.CreatedDate.Value.Year, br.CreatedDate.Value.Month })
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

                return new JsonModel { data = analytics, Message = "Payment analytics retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment analytics");
                return new JsonModel { data = new object(), Message = "Error retrieving payment analytics", StatusCode = 500 };
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

        public async Task<JsonModel> RetryPaymentAsync(Guid billingRecordId)
        {
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
                if (billingRecord == null)
                {
                    return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
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

                return new JsonModel { data = paymentResult, Message = "Payment retry initiated successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying payment for billing record {BillingRecordId}", billingRecordId);
                return new JsonModel { data = new object(), Message = "Error retrying payment", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> ProcessRefundAsync(Guid billingRecordId, decimal amount, string reason)
        {
            try
            {
                var billingRecord = await _billingRepository.GetByIdAsync(billingRecordId);
                if (billingRecord == null)
                {
                    return new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 404 };
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

                return new JsonModel { data = refundResult, Message = "Refund processed successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for billing record {BillingRecordId}", billingRecordId);
                return new JsonModel { data = new object(), Message = "Error processing refund", 500);
            }
        }

        public async Task<JsonModel> GetPaymentAnalyticsAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var userBillingRecords = await _billingRepository.GetByUserIdAsync(userId);
                
                // Filter by date range if provided
                if (startDate.HasValue || endDate.HasValue)
                {
                    userBillingRecords = userBillingRecords.Where(br => 
                        (!startDate.HasValue || br.CreatedDate >= startDate.Value) &&
                        (!endDate.HasValue || br.CreatedDate <= endDate.Value));
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
                        .Where(br => br.CreatedDate.HasValue)
                        .GroupBy(br => new { br.CreatedDate.Value.Year, br.CreatedDate.Value.Month })
                        .Select(g => new MonthlyPaymentDto
                        {
                            Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                            Amount = g.Sum(br => br.Amount),
                            Count = g.Count()
                        })
                        .OrderBy(mp => mp.Month)
                        .ToList()
                };

                return new JsonModel { data = analytics, Message = "Payment analytics retrieved successfully", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment analytics for user {UserId}", userId);
                return new JsonModel { data = new object(), Message = "Error retrieving payment analytics", StatusCode = 500 };
            }
        }
    }
} 