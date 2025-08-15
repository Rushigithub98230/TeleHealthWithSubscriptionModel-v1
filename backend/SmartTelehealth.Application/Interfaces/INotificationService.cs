using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface INotificationService
{
    // CRUD operations
    Task<JsonModel> GetNotificationsAsync();
    Task<JsonModel> GetNotificationAsync(Guid id);
    Task<JsonModel> CreateNotificationAsync(CreateNotificationDto createNotificationDto);
    Task<JsonModel> UpdateNotificationAsync(Guid id, object updateNotificationDto);
    Task<JsonModel> DeleteNotificationAsync(Guid id);
    
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
    Task<JsonModel> CreateInAppNotificationAsync(int userId, string title, string message);
    Task<JsonModel> GetUserNotificationsAsync(int userId);
    Task<JsonModel> MarkNotificationAsReadAsync(Guid notificationId);
    Task<JsonModel> GetUnreadNotificationCountAsync(int userId);
    
    // Utility methods
    Task<JsonModel> IsEmailValidAsync(string email);
    Task<JsonModel> SendSmsAsync(string phoneNumber, string message);
    Task SendNotificationAsync(string userId, string title, string message);
    
    // Added missing methods for BillingService and AutomatedBillingService
    Task SendSubscriptionSuspendedNotificationAsync(string userId, string subscriptionId);
    Task SendRefundNotificationAsync(string userId, decimal amount, string billingRecordId);
    Task SendSubscriptionReactivatedNotificationAsync(string userId, string subscriptionId);
} 