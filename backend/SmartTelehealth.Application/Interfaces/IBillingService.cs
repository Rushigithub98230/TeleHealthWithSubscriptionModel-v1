using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.Interfaces;

public interface IBillingService
{
    // Existing Methods
    Task<JsonModel> CreateBillingRecordAsync(CreateBillingRecordDto createDto);
    Task<JsonModel> GetBillingRecordAsync(Guid id);
    Task<JsonModel> GetUserBillingHistoryAsync(int userId);
    Task<JsonModel> GetSubscriptionBillingHistoryAsync(Guid subscriptionId);
    Task<JsonModel> GetAllBillingRecordsAsync();
    Task<JsonModel> ProcessPaymentAsync(Guid billingRecordId);
    Task<JsonModel> ProcessRefundAsync(Guid billingRecordId, decimal amount);
    Task<JsonModel> ProcessRefundAsync(Guid billingRecordId, decimal amount, string reason);
    Task<JsonModel> GetOverdueBillingRecordsAsync();
    Task<JsonModel> GetPendingPaymentsAsync();
    Task<JsonModel> CalculateTotalAmountAsync(decimal baseAmount, decimal taxAmount, decimal shippingAmount);
    Task<JsonModel> CalculateTaxAmountAsync(decimal baseAmount, string state);
    Task<JsonModel> CalculateShippingAmountAsync(string deliveryAddress, bool isExpress);
    Task<JsonModel> IsPaymentOverdueAsync(Guid billingRecordId);
    Task<JsonModel> CalculateDueDateAsync(DateTime billingDate, int gracePeriodDays);
    Task<JsonModel> GetBillingAnalyticsAsync();
    
    // Payment History Methods
    Task<JsonModel> GetPaymentHistoryAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<JsonModel> GetPaymentAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<JsonModel> GetPaymentAnalyticsAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
    
    // Phase 2: Enhanced Billing Features
    Task<JsonModel> CreateRecurringBillingAsync(CreateRecurringBillingDto createDto);
    Task<JsonModel> ProcessRecurringPaymentAsync(Guid subscriptionId);
    Task<JsonModel> CancelRecurringBillingAsync(Guid subscriptionId);
    Task<JsonModel> CreateUpfrontPaymentAsync(CreateUpfrontPaymentDto createDto);
    Task<JsonModel> ProcessBundlePaymentAsync(CreateBundlePaymentDto createDto);
    Task<JsonModel> ApplyBillingAdjustmentAsync(Guid billingRecordId, CreateBillingAdjustmentDto adjustmentDto);
    Task<JsonModel> GetBillingAdjustmentsAsync(Guid billingRecordId);
    Task<JsonModel> RetryFailedPaymentAsync(Guid billingRecordId);
    Task<JsonModel> RetryPaymentAsync(Guid billingRecordId);
    Task<JsonModel> ProcessPartialPaymentAsync(Guid billingRecordId, decimal amount);
    Task<JsonModel> CreateInvoiceAsync(CreateInvoiceDto createDto);
    Task<JsonModel> GenerateInvoicePdfAsync(Guid billingRecordId);
    Task<JsonModel> GenerateBillingReportAsync(DateTime startDate, DateTime endDate, string format = "pdf");
    Task<JsonModel> GetBillingSummaryAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<JsonModel> GetPaymentScheduleAsync(Guid subscriptionId);
    Task<JsonModel> UpdatePaymentMethodAsync(Guid billingRecordId, string paymentMethodId);
    Task<JsonModel> CreateBillingCycleAsync(CreateBillingCycleDto createDto);
    Task<JsonModel> ProcessBillingCycleAsync(Guid billingCycleId);
    Task<JsonModel> GetBillingCycleRecordsAsync(Guid billingCycleId);
    Task<JsonModel> GetRevenueSummaryAsync(DateTime? from = null, DateTime? to = null, string? planId = null);
    Task<JsonModel> ExportRevenueAsync(DateTime? from = null, DateTime? to = null, string? planId = null, string format = "csv");
} 