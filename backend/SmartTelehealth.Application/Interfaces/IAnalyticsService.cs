using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IAnalyticsService
{
    Task<ApiResponse<RevenueAnalyticsDto>> GetRevenueAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<UserActivityAnalyticsDto>> GetUserActivityAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<AppointmentAnalyticsDto>> GetAppointmentAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<SubscriptionAnalyticsDto>> GetSubscriptionAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<SystemAnalyticsDto>> GetSystemAnalyticsAsync();
    
    // Additional Analytics Methods
    Task<ApiResponse<BillingAnalyticsDto>> GetBillingAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<UserAnalyticsDto>> GetUserAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<ProviderAnalyticsDto>> GetProviderAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<SystemHealthDto>> GetSystemHealthAsync();
    
    // Report Generation Methods
    Task<ApiResponse<byte[]>> GenerateSubscriptionReportAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<byte[]>> GenerateBillingReportAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<byte[]>> GenerateUserReportAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<byte[]>> GenerateProviderReportAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<byte[]>> ExportSubscriptionAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
} 