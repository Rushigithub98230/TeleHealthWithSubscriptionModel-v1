using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces
{
    public interface ISubscriptionService
    {
        Task<JsonModel> GetSubscriptionAsync(string subscriptionId);
        Task<JsonModel> GetUserSubscriptionsAsync(int userId);
        Task<JsonModel> CreateSubscriptionAsync(CreateSubscriptionDto createDto);
        Task<JsonModel> CancelSubscriptionAsync(string subscriptionId, string? reason = null);
        Task<JsonModel> PauseSubscriptionAsync(string subscriptionId);
        Task<JsonModel> ResumeSubscriptionAsync(string subscriptionId);
        Task<JsonModel> GetSubscriptionByPlanIdAsync(string planId);
        Task<JsonModel> GetPaymentMethodsAsync(int userId);
        Task<JsonModel> AddPaymentMethodAsync(int userId, string paymentMethodId);
        
        // Missing methods from controllers
        Task<JsonModel> GetActiveSubscriptionsAsync();
        Task<JsonModel> GetAllPlansAsync();
        Task<JsonModel> GetAllPlansAsync(int page, int pageSize, string? searchTerm, string? categoryId, bool? isActive);
        Task<JsonModel> GetPlanByIdAsync(string planId);
        Task<JsonModel> GetSubscriptionByIdAsync(string subscriptionId);
        Task<JsonModel> UpdateSubscriptionAsync(string subscriptionId, UpdateSubscriptionDto updateDto);
        Task<JsonModel> ReactivateSubscriptionAsync(string subscriptionId);
        Task<JsonModel> UpgradeSubscriptionAsync(string subscriptionId, string newPlanId);
        Task<JsonModel> GetBillingHistoryAsync(string subscriptionId);
        Task<JsonModel> ProcessPaymentAsync(string subscriptionId, PaymentRequestDto paymentRequest);
        Task<JsonModel> GetUsageStatisticsAsync(string subscriptionId);
        Task<JsonModel> GetAllSubscriptionsAsync();
        Task<JsonModel> GetSubscriptionAnalyticsAsync(string subscriptionId);
        Task<JsonModel> CreatePlanAsync(CreateSubscriptionPlanDto createPlanDto);
        Task<JsonModel> UpdatePlanAsync(string planId, UpdateSubscriptionPlanDto updatePlanDto);
        Task<JsonModel> ActivatePlanAsync(string planId);
        Task<JsonModel> DeactivatePlanAsync(string planId);
        Task<JsonModel> DeletePlanAsync(string planId);
        
        // Admin management methods
        Task<JsonModel> GetAllUserSubscriptionsAsync(int page, int pageSize, string? userId, string? planId, string? status, DateTime? startDate, DateTime? endDate);
        Task<JsonModel> CancelUserSubscriptionAsync(string subscriptionId, string? reason);
        Task<JsonModel> PauseUserSubscriptionAsync(string subscriptionId);
        Task<JsonModel> ResumeUserSubscriptionAsync(string subscriptionId);
        Task<JsonModel> ExtendUserSubscriptionAsync(string subscriptionId, int additionalDays);
        Task<JsonModel> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId);
        
        // Bulk operations
        Task<JsonModel> PerformBulkActionAsync(List<BulkActionRequestDto> actions);
        Task<JsonModel> GetAllSubscriptionPlansAsync();
        Task<JsonModel> GetActiveSubscriptionPlansAsync();
        Task<JsonModel> GetSubscriptionPlansByCategoryAsync(string category);
        Task<JsonModel> GetSubscriptionPlanAsync(string planId);
        
        // Category management
        Task<JsonModel> GetAllCategoriesAsync(int page, int pageSize, string? searchTerm, bool? isActive);
        Task<JsonModel> CreateSubscriptionPlanAsync(CreateSubscriptionPlanDto createDto);
        
        // Analytics methods
        Task<JsonModel> GetSubscriptionAnalyticsAsync(string subscriptionId, DateTime? startDate, DateTime? endDate);
        Task<JsonModel> UpdateSubscriptionPlanAsync(string planId, UpdateSubscriptionPlanDto updateDto);
        Task<JsonModel> DeleteSubscriptionPlanAsync(string planId);
        Task<JsonModel> HandleFailedPaymentAsync(string subscriptionId, string reason);
        Task<JsonModel> CanUsePrivilegeAsync(string subscriptionId, string privilegeName);
        Task<JsonModel> DeactivatePlanAsync(string planId, string adminUserId);
        Task<JsonModel> HandlePaymentProviderWebhookAsync(string eventType, string subscriptionId, string? errorMessage = null);
    }
} 