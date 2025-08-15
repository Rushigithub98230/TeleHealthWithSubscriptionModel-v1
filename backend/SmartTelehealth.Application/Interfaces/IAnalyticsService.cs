using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IAnalyticsService
{
    Task<JsonModel> GetRevenueAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<JsonModel> GetUserActivityAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<JsonModel> GetAppointmentAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<JsonModel> GetSubscriptionAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<JsonModel> GetSystemAnalyticsAsync();
    
    // Additional Analytics Methods
    Task<JsonModel> GetBillingAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<JsonModel> GetUserAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<JsonModel> GetProviderAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<JsonModel> GetSystemHealthAsync();
    
    // Report Generation Methods
    Task<JsonModel> GenerateSubscriptionReportAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<JsonModel> GenerateBillingReportAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<JsonModel> GenerateUserReportAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<JsonModel> GenerateProviderReportAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<JsonModel> ExportSubscriptionAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
} 