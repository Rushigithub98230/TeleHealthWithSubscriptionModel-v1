using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface INotificationService
{
    // CRUD operations
    Task<ApiResponse<IEnumerable<NotificationDto>>> GetNotificationsAsync();
    Task<ApiResponse<NotificationDto>> GetNotificationAsync(Guid id);
    Task<ApiResponse<NotificationDto>> CreateNotificationAsync(CreateNotificationDto createNotificationDto);
    Task<ApiResponse<NotificationDto>> UpdateNotificationAsync(Guid id, object updateNotificationDto);
    Task<ApiResponse<bool>> DeleteNotificationAsync(Guid id);
    
    // Email notifications (void methods - no response needed)
    Task SendWelcomeEmailAsync(string email, string userName);
    Task SendEmailVerificationAsync(string email, string userName, string verificationToken);
    Task SendSubscriptionConfirmationAsync(string email, string userName, SubscriptionDto subscription);
    Task SendSubscriptionWelcomeEmailAsync(string email, string userName, SubscriptionDto subscription);
    Task SendSubscriptionCancellationEmailAsync(string email, string userName, SubscriptionDto subscription);
    Task SendSubscriptionSuspensionEmailAsync(string email, string userName, SubscriptionDto subscription);
    Task SendPaymentReminderAsync(string email, string userName, BillingRecordDto billingRecord);
    Task SendConsultationReminderAsync(string email, string userName, ConsultationDto consultation);
    Task SendPasswordResetEmailAsync(string email, string resetToken);
    Task SendDeliveryNotificationAsync(string email, string userName, MedicationDeliveryDto delivery);
    Task SendSubscriptionPausedNotificationAsync(string email, string userName, SubscriptionDto subscription);
    Task SendSubscriptionResumedNotificationAsync(string email, string userName, SubscriptionDto subscription);
    Task SendSubscriptionCancelledNotificationAsync(string email, string userName, SubscriptionDto subscription);
    Task SendProviderMessageNotificationAsync(string email, string userName, MessageDto message);
    
    // Billing email notifications (void methods - no response needed)
    Task SendPaymentSuccessEmailAsync(string email, string userName, BillingRecordDto billingRecord);
    Task SendPaymentFailedEmailAsync(string email, string userName, BillingRecordDto billingRecord);
    Task SendRefundProcessedEmailAsync(string email, string userName, BillingRecordDto billingRecord, decimal refundAmount);
    Task SendOverduePaymentEmailAsync(string email, string userName, BillingRecordDto billingRecord);
    
    // In-app notifications
    Task<ApiResponse<NotificationDto>> CreateInAppNotificationAsync(Guid userId, string title, string message);
    Task<ApiResponse<IEnumerable<NotificationDto>>> GetUserNotificationsAsync(Guid userId);
    Task<ApiResponse<bool>> MarkNotificationAsReadAsync(Guid notificationId);
    Task<ApiResponse<int>> GetUnreadNotificationCountAsync(Guid userId);
    
    // Utility methods
    Task<ApiResponse<bool>> IsEmailValidAsync(string email);
    Task<ApiResponse<bool>> SendSmsAsync(string phoneNumber, string message);
    Task SendNotificationAsync(string userId, string title, string message);
    
    // Added missing methods for BillingService and AutomatedBillingService
    Task SendSubscriptionSuspendedNotificationAsync(string userId, string subscriptionId);
    Task SendRefundNotificationAsync(string userId, decimal amount, string billingRecordId);
    Task SendSubscriptionReactivatedNotificationAsync(string userId, string subscriptionId);
} 