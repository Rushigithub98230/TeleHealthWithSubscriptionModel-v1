using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IAnalyticsService
    {
        Task<ApiResponse<SubscriptionAnalyticsDto>> GetSubscriptionAnalyticsAsync();
        Task<ApiResponse<BillingAnalyticsDto>> GetBillingAnalyticsAsync();
        Task<ApiResponse<UserAnalyticsDto>> GetUserAnalyticsAsync();
        Task<ApiResponse<SystemHealthDto>> GetSystemHealthAsync();
        Task<ApiResponse<ProviderAnalyticsDto>> GetProviderAnalyticsAsync();
        Task<ApiResponse<SystemAnalyticsDto>> GetSystemAnalyticsAsync();
        
        // Report Generation Methods
        Task<byte[]> GenerateSubscriptionReportAsync(DateTime startDate, DateTime endDate, string format = "pdf");
        Task<byte[]> GenerateBillingReportAsync(DateTime startDate, DateTime endDate, string format = "pdf");
        Task<byte[]> GenerateUserReportAsync(DateTime startDate, DateTime endDate, string format = "pdf");
        Task<byte[]> GenerateProviderReportAsync(DateTime startDate, DateTime endDate, string format = "pdf");
    }
} 