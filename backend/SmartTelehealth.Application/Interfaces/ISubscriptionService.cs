using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces
{
    public interface ISubscriptionService
    {
        Task<ApiResponse<SubscriptionDto>> GetSubscriptionAsync(string subscriptionId);
        Task<ApiResponse<IEnumerable<SubscriptionDto>>> GetUserSubscriptionsAsync(string userId);
        Task<ApiResponse<SubscriptionDto>> CreateSubscriptionAsync(CreateSubscriptionDto createDto);
        Task<ApiResponse<SubscriptionDto>> CancelSubscriptionAsync(string subscriptionId, string? reason = null);
        Task<ApiResponse<SubscriptionDto>> PauseSubscriptionAsync(string subscriptionId);
        Task<ApiResponse<SubscriptionDto>> ResumeSubscriptionAsync(string subscriptionId);
        Task<ApiResponse<SubscriptionDto>> GetSubscriptionByPlanIdAsync(string planId);
        Task<ApiResponse<IEnumerable<PaymentMethodDto>>> GetPaymentMethodsAsync(string userId);
        Task<ApiResponse<PaymentMethodDto>> AddPaymentMethodAsync(string userId, string paymentMethodId);
        
        // Missing methods from controllers
        Task<ApiResponse<IEnumerable<SubscriptionDto>>> GetActiveSubscriptionsAsync();
        Task<ApiResponse<IEnumerable<SubscriptionPlanDto>>> GetAllPlansAsync();
        Task<ApiResponse<SubscriptionPlanDto>> GetPlanByIdAsync(string planId);
        Task<ApiResponse<SubscriptionDto>> GetSubscriptionByIdAsync(string subscriptionId);
        Task<ApiResponse<SubscriptionDto>> UpdateSubscriptionAsync(string subscriptionId, UpdateSubscriptionDto updateDto);
        Task<ApiResponse<SubscriptionDto>> ReactivateSubscriptionAsync(string subscriptionId);
        Task<ApiResponse<SubscriptionDto>> UpgradeSubscriptionAsync(string subscriptionId, string newPlanId);
        Task<ApiResponse<IEnumerable<BillingHistoryDto>>> GetBillingHistoryAsync(string subscriptionId);
        Task<ApiResponse<PaymentResultDto>> ProcessPaymentAsync(string subscriptionId, PaymentRequestDto paymentRequest);
        Task<ApiResponse<UsageStatisticsDto>> GetUsageStatisticsAsync(string subscriptionId);
        Task<ApiResponse<IEnumerable<SubscriptionDto>>> GetAllSubscriptionsAsync();
        Task<ApiResponse<SubscriptionAnalyticsDto>> GetSubscriptionAnalyticsAsync(string subscriptionId);
        Task<ApiResponse<SubscriptionPlanDto>> CreatePlanAsync(CreateSubscriptionPlanDto createPlanDto);
        Task<ApiResponse<SubscriptionPlanDto>> UpdatePlanAsync(string planId, UpdateSubscriptionPlanDto updatePlanDto);
        Task<ApiResponse<bool>> ActivatePlanAsync(string planId);
        Task<ApiResponse<bool>> DeactivatePlanAsync(string planId);
        Task<ApiResponse<bool>> DeletePlanAsync(string planId);
        Task<ApiResponse<SubscriptionDto>> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId);
        Task<ApiResponse<IEnumerable<SubscriptionPlanDto>>> GetAllSubscriptionPlansAsync();
        Task<ApiResponse<IEnumerable<SubscriptionPlanDto>>> GetActiveSubscriptionPlansAsync();
        Task<ApiResponse<IEnumerable<SubscriptionPlanDto>>> GetSubscriptionPlansByCategoryAsync(string category);
        Task<ApiResponse<SubscriptionPlanDto>> GetSubscriptionPlanAsync(string planId);
        Task<ApiResponse<SubscriptionPlanDto>> CreateSubscriptionPlanAsync(CreateSubscriptionPlanDto createDto);
        Task<ApiResponse<SubscriptionPlanDto>> UpdateSubscriptionPlanAsync(string planId, UpdateSubscriptionPlanDto updateDto);
        Task<ApiResponse<bool>> DeleteSubscriptionPlanAsync(string planId);
        Task<ApiResponse<PaymentResultDto>> HandleFailedPaymentAsync(string subscriptionId, string reason);
        Task<ApiResponse<bool>> CanUsePrivilegeAsync(string subscriptionId, string privilegeName);
        Task<ApiResponse<bool>> DeactivatePlanAsync(string planId, string adminUserId);
        Task<ApiResponse<bool>> HandlePaymentProviderWebhookAsync(string eventType, string subscriptionId, string? errorMessage = null);
    }
} 