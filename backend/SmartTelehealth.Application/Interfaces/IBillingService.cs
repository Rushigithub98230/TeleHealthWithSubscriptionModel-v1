using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.Interfaces;

public interface IBillingService
{
    // Existing Methods
    Task<ApiResponse<BillingRecordDto>> CreateBillingRecordAsync(CreateBillingRecordDto createDto);
    Task<ApiResponse<BillingRecordDto>> GetBillingRecordAsync(Guid id);
    Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetUserBillingHistoryAsync(Guid userId);
    Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetSubscriptionBillingHistoryAsync(Guid subscriptionId);
    Task<ApiResponse<BillingRecordDto>> ProcessPaymentAsync(Guid billingRecordId);
    Task<ApiResponse<BillingRecordDto>> ProcessRefundAsync(Guid billingRecordId, decimal amount);
    Task<ApiResponse<RefundResultDto>> ProcessRefundAsync(Guid billingRecordId, decimal amount, string reason);
    Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetOverdueBillingRecordsAsync();
    Task<ApiResponse<IEnumerable<BillingRecordDto>>> GetPendingPaymentsAsync();
    Task<ApiResponse<decimal>> CalculateTotalAmountAsync(decimal baseAmount, decimal taxAmount, decimal shippingAmount);
    Task<ApiResponse<decimal>> CalculateTaxAmountAsync(decimal baseAmount, string state);
    Task<ApiResponse<decimal>> CalculateShippingAmountAsync(string deliveryAddress, bool isExpress);
    Task<ApiResponse<bool>> IsPaymentOverdueAsync(Guid billingRecordId);
    Task<ApiResponse<DateTime>> CalculateDueDateAsync(DateTime billingDate, int gracePeriodDays);
    Task<ApiResponse<BillingAnalyticsDto>> GetBillingAnalyticsAsync();
    
    // Payment History Methods
    Task<ApiResponse<IEnumerable<PaymentHistoryDto>>> GetPaymentHistoryAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<PaymentAnalyticsDto>> GetPaymentAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<PaymentAnalyticsDto>> GetPaymentAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    
    // Phase 2: Enhanced Billing Features
    Task<ApiResponse<BillingRecordDto>> CreateRecurringBillingAsync(CreateRecurringBillingDto createDto);
    Task<ApiResponse<BillingRecordDto>> ProcessRecurringPaymentAsync(Guid subscriptionId);
    Task<ApiResponse<bool>> CancelRecurringBillingAsync(Guid subscriptionId);
    Task<ApiResponse<BillingRecordDto>> CreateUpfrontPaymentAsync(CreateUpfrontPaymentDto createDto);
    Task<ApiResponse<BillingRecordDto>> ProcessBundlePaymentAsync(CreateBundlePaymentDto createDto);
    Task<ApiResponse<BillingRecordDto>> ApplyBillingAdjustmentAsync(Guid billingRecordId, CreateBillingAdjustmentDto adjustmentDto);
    Task<ApiResponse<IEnumerable<BillingAdjustmentDto>>> GetBillingAdjustmentsAsync(Guid billingRecordId);
    Task<ApiResponse<BillingRecordDto>> RetryFailedPaymentAsync(Guid billingRecordId);
    Task<ApiResponse<PaymentResultDto>> RetryPaymentAsync(Guid billingRecordId);
    Task<ApiResponse<BillingRecordDto>> ProcessPartialPaymentAsync(Guid billingRecordId, decimal amount);
    Task<ApiResponse<BillingRecordDto>> CreateInvoiceAsync(CreateInvoiceDto createDto);
    Task<ApiResponse<byte[]>> GenerateInvoicePdfAsync(Guid billingRecordId);
    Task<ApiResponse<byte[]>> GenerateBillingReportAsync(DateTime startDate, DateTime endDate, string format = "pdf");
    Task<ApiResponse<BillingSummaryDto>> GetBillingSummaryAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<PaymentScheduleDto>> GetPaymentScheduleAsync(Guid subscriptionId);
    Task<ApiResponse<bool>> UpdatePaymentMethodAsync(Guid billingRecordId, string paymentMethodId);
    Task<ApiResponse<BillingRecordDto>> CreateBillingCycleAsync(CreateBillingCycleDto createDto);
    Task<ApiResponse<BillingRecordDto>> ProcessBillingCycleAsync(Guid billingCycleId);
    Task<ApiResponse<RevenueSummaryDto>> GetRevenueSummaryAsync(DateTime? from = null, DateTime? to = null, string? planId = null);
    Task<ApiResponse<byte[]>> ExportRevenueAsync(DateTime? from = null, DateTime? to = null, string? planId = null, string format = "csv");
} 