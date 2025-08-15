using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public async Task<JsonModel> GetNotificationsAsync()
        {
            try
            {
                // In a real implementation, this would fetch from a notification repository
                var notifications = new List<NotificationDto>();
                return new JsonModel { data = .SuccessResponse(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                return new JsonModel { data = new object(), Message = $"Failed to get notifications: {ex.Message}", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetNotificationAsync(Guid id)
        {
            try
            {
                // In a real implementation, this would fetch from a notification repository
                var notification = new NotificationDto
                {
                    Id = id.ToString(),
                    UserId = Guid.NewGuid().ToString(),
                    Title = "Sample Notification",
                    Message = "This is a sample notification",
                    Type = "Info",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                return new JsonModel { data = notification, Message = "Success", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification {Id}", id);
                return new JsonModel { data = new object(), Message = $"Failed to get notification: {ex.Message}", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> CreateNotificationAsync(CreateNotificationDto createNotificationDto)
        {
            try
            {
                // In a real implementation, this would save to a notification repository
                var notification = new NotificationDto
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = createNotificationDto.UserId.ToString(),
                    Title = createNotificationDto.Title,
                    Message = createNotificationDto.Message,
                    Type = createNotificationDto.Type,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                return new JsonModel { data = notification, Message = "Success", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                return new JsonModel { data = new object(), Message = $"Failed to create notification: {ex.Message}", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> UpdateNotificationAsync(Guid id, object updateNotificationDto)
        {
            try
            {
                // In a real implementation, this would update in a notification repository
                var notification = new NotificationDto
                {
                    Id = id.ToString(),
                    UserId = Guid.NewGuid().ToString(),
                    Title = "Updated Notification",
                    Message = "Updated message",
                    Type = "Info",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow
                };
                return new JsonModel { data = notification, Message = "Success", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification {Id}", id);
                return new JsonModel { data = new object(), Message = $"Failed to update notification: {ex.Message}", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> DeleteNotificationAsync(Guid id)
        {
            try
            {
                // In a real implementation, this would delete from a notification repository
                _logger.LogInformation("Deleting notification {Id}", id);
                return new JsonModel { data = true, Message = "Success", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {Id}", id);
                return new JsonModel { data = new object(), Message = $"Failed to delete notification: {ex.Message}", StatusCode = 500 };
            }
        }

        public async Task SendWelcomeEmailAsync(string email, string userName)
        {
            try
            {
                var subject = "Welcome to SmartTelehealth!";
                var body = $@"
                    <h2>Welcome to SmartTelehealth!</h2>
                    <p>Hi {userName},</p>
                    <p>Thank you for joining SmartTelehealth. We're excited to have you on board!</p>
                    <p>Best regards,<br>The SmartTelehealth Team</p>";

                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Email sending disabled - would have sent welcome email to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email to {Email}", email);
            }
        }

        public async Task SendEmailVerificationAsync(string email, string userName, string verificationToken)
        {
            try
            {
                var subject = "Verify Your Email Address";
                var body = $@"
                    <h2>Welcome to SmartTelehealth!</h2>
                    <p>Hi {userName},</p>
                    <p>Please verify your email address by clicking the link below:</p>
                    <p><a href='https://yourdomain.com/verify-email?token={verificationToken}&email={email}'>Verify Email</a></p>
                    <p>If you didn't create an account, please ignore this email.</p>
                    <p>Best regards,<br>The SmartTelehealth Team</p>";

                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Email sending disabled - would have sent email verification to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email verification to {Email}", email);
            }
        }

        public async Task SendSubscriptionConfirmationAsync(string email, string userName, SubscriptionDto subscription)
        {
            try
            {
                var subject = "Subscription Confirmed";
                var body = $@"
                    <h2>Subscription Confirmed</h2>
                    <p>Hi {userName},</p>
                    <p>Your subscription to {subscription.PlanName} has been confirmed.</p>
                    <p>Plan: {subscription.PlanName}</p>
                    <p>Start Date: {subscription.StartDate:MMM dd, yyyy}</p>
                    <p>Best regards,<br>The SmartTelehealth Team</p>";

                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Email sending disabled - would have sent subscription confirmation to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending subscription confirmation to {Email}", email);
            }
        }

        public async Task SendSubscriptionWelcomeEmailAsync(string email, string userName, SubscriptionDto subscription)
        {
            try
            {
                var subject = "Welcome to Your New Subscription";
                var body = $@"
                    <h2>Welcome to Your New Subscription!</h2>
                    <p>Hi {userName},</p>
                    <p>Welcome to your new {subscription.PlanName} subscription!</p>
                    <p>Best regards,<br>The SmartTelehealth Team</p>";

                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Email sending disabled - would have sent subscription welcome email to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending subscription welcome email to {Email}", email);
            }
        }

        public async Task SendSubscriptionCancellationEmailAsync(string email, string userName, SubscriptionDto subscription)
        {
            try
            {
                var subject = "Subscription Cancelled";
                var body = $@"
                    <h2>Subscription Cancelled</h2>
                    <p>Hi {userName},</p>
                    <p>Your subscription to {subscription.PlanName} has been cancelled.</p>
                    <p>Best regards,<br>The SmartTelehealth Team</p>";

                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Email sending disabled - would have sent subscription cancellation email to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending subscription cancellation email to {Email}", email);
            }
        }

        public async Task SendPaymentReminderAsync(string email, string userName, BillingRecordDto billingRecord)
        {
            try
            {
                var subject = "Payment Reminder";
                var body = $@"
                    <h2>Payment Reminder</h2>
                    <p>Hi {userName},</p>
                    <p>This is a reminder that your payment of ${billingRecord.Amount} is due on {billingRecord.DueDate:MMM dd, yyyy}.</p>
                    <p>Best regards,<br>The SmartTelehealth Team</p>";

                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Email sending disabled - would have sent payment reminder to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment reminder to {Email}", email);
            }
        }

        public async Task SendConsultationReminderAsync(string email, string userName, ConsultationDto consultation)
        {
            try
            {
                var subject = "Consultation Reminder";
                var body = $@"
                    <h2>Consultation Reminder</h2>
                    <p>Hi {userName},</p>
                    <p>This is a reminder for your consultation on {consultation.ScheduledAt:MMM dd, yyyy} at {consultation.ScheduledAt:HH:mm}.</p>
                    <p>Best regards,<br>The SmartTelehealth Team</p>";

                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Email sending disabled - would have sent consultation reminder to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending consultation reminder to {Email}", email);
            }
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetToken)
        {
            try
            {
                var subject = "Reset Your Password";
                var body = $@"
                    <h2>Password Reset Request</h2>
                    <p>You requested to reset your password. Click the link below to set a new password:</p>
                    <p><a href='https://yourdomain.com/reset-password?token={resetToken}&email={email}'>Reset Password</a></p>
                    <p>If you didn't request this, please ignore this email.</p>
                    <p>Best regards,<br>The SmartTelehealth Team</p>";

                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Email sending disabled - would have sent password reset email to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to {Email}", email);
            }
        }

        public async Task SendDeliveryNotificationAsync(string email, string userName, MedicationDeliveryDto delivery)
        {
            try
            {
                var subject = "Medication Delivery Update";
                var body = $@"
                    <h2>Medication Delivery Update</h2>
                    <p>Hi {userName},</p>
                    <p>Your medication delivery status: {delivery.Status}</p>
                    <p>Best regards,<br>The SmartTelehealth Team</p>";

                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Email sending disabled - would have sent delivery notification to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending delivery notification to {Email}", email);
            }
        }

        public async Task SendSubscriptionPausedNotificationAsync(string email, string userName, SubscriptionDto subscription)
        {
            try
            {
                var subject = "Subscription Paused";
                var body = $@"
                    <h2>Subscription Paused</h2>
                    <p>Hi {userName},</p>
                    <p>Your subscription to {subscription.PlanName} has been paused.</p>
                    <p>Best regards,<br>The SmartTelehealth Team</p>";

                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Email sending disabled - would have sent subscription paused notification to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending subscription paused notification to {Email}", email);
            }
        }

        public async Task SendSubscriptionResumedNotificationAsync(string email, string userName, SubscriptionDto subscription)
        {
            try
            {
                var subject = "Subscription Resumed";
                var body = $@"
                    <h2>Subscription Resumed</h2>
                    <p>Hi {userName},</p>
                    <p>Your subscription to {subscription.PlanName} has been resumed.</p>
                    <p>Best regards,<br>The SmartTelehealth Team</p>";

                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Email sending disabled - would have sent subscription resumed notification to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending subscription resumed notification to {Email}", email);
            }
        }

        public async Task SendSubscriptionCancelledNotificationAsync(string email, string userName, SubscriptionDto subscription)
        {
            try
            {
                var subject = "Subscription Cancelled";
                var body = $@"
                    <h2>Subscription Cancelled</h2>
                    <p>Hi {userName},</p>
                    <p>Your subscription to {subscription.PlanName} has been cancelled.</p>
                    <p>Best regards,<br>The SmartTelehealth Team</p>";

                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Email sending disabled - would have sent subscription cancelled notification to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending subscription cancelled notification to {Email}", email);
            }
        }

        public async Task SendProviderMessageNotificationAsync(string email, string userName, MessageDto message)
        {
            try
            {
                var subject = "New Message from Provider";
                var body = $@"
                    <h2>New Message</h2>
                    <p>Hi {userName},</p>
                    <p>You have a new message from your provider.</p>
                    <p>Best regards,<br>The SmartTelehealth Team</p>";

                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Email sending disabled - would have sent provider message notification to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending provider message notification to {Email}", email);
            }
        }

        public async Task SendPaymentSuccessEmailAsync(string email, string userName, BillingRecordDto billingRecord)
        {
            try
            {
                var subject = "Payment Successful";
                var body = $@"
                    <h2>Payment Successful</h2>
                    <p>Hi {userName},</p>
                    <p>Your payment of ${billingRecord.Amount} has been processed successfully.</p>
                    <p>Best regards,<br>The SmartTelehealth Team</p>";

                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Email sending disabled - would have sent payment success email to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment success email to {Email}", email);
            }
        }

        public async Task SendPaymentFailedEmailAsync(string email, string userName, BillingRecordDto billingRecord)
        {
            try
            {
                var subject = "Payment Failed";
                var body = $@"
                    <h2>Payment Failed</h2>
                    <p>Hi {userName},</p>
                    <p>Your payment of ${billingRecord.Amount} has failed. Please try again.</p>
                    <p>Best regards,<br>The SmartTelehealth Team</p>";

                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Email sending disabled - would have sent payment failed email to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment failed email to {Email}", email);
            }
        }

        public async Task SendRefundProcessedEmailAsync(string email, string userName, BillingRecordDto billingRecord, decimal refundAmount)
        {
            try
            {
                var subject = "Refund Processed";
                var body = $@"
                    <h2>Refund Processed</h2>
                    <p>Hi {userName},</p>
                    <p>Your refund of ${refundAmount} has been processed.</p>
                    <p>Best regards,<br>The SmartTelehealth Team</p>";

                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Email sending disabled - would have sent refund processed email to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending refund processed email to {Email}", email);
            }
        }

        public async Task SendOverduePaymentEmailAsync(string email, string userName, BillingRecordDto billingRecord)
        {
            try
            {
                var subject = "Payment Overdue";
                var body = $@"
                    <h2>Payment Overdue</h2>
                    <p>Hi {userName},</p>
                    <p>Your payment of ${billingRecord.Amount} is overdue. Please make payment as soon as possible.</p>
                    <p>Best regards,<br>The SmartTelehealth Team</p>";

                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Email sending disabled - would have sent overdue payment email to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending overdue payment email to {Email}", email);
            }
        }

        public async Task<JsonModel> CreateInAppNotificationAsync(Guid userId, string title, string message)
        {
            try
            {
                var notification = new NotificationDto
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId.ToString(),
                    Title = title,
                    Message = message,
                    Type = "Info",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                return new JsonModel { data = notification, Message = "Success", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating in-app notification for user {UserId}", userId);
                return new JsonModel { data = new object(), Message = $"Failed to create notification: {ex.Message}", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetUserNotificationsAsync(Guid userId)
        {
            try
            {
                // In a real implementation, this would fetch from a notification repository
                var notifications = new List<NotificationDto>();
                return new JsonModel { data = .SuccessResponse(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                return new JsonModel { data = new object(), Message = $"Failed to get notifications: {ex.Message}", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> MarkNotificationAsReadAsync(Guid notificationId)
        {
            try
            {
                // In a real implementation, this would update in a notification repository
                _logger.LogInformation("Marking notification {Id} as read", notificationId);
                return new JsonModel { data = true, Message = "Success", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {Id} as read", notificationId);
                return new JsonModel { data = new object(), Message = $"Failed to mark notification as read: {ex.Message}", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetUnreadNotificationCountAsync(Guid userId)
        {
            try
            {
                // In a real implementation, this would count from a notification repository
                return new JsonModel { data = 0, Message = "Success", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notification count for user {UserId}", userId);
                return new JsonModel { data = new object(), Message = $"Failed to get unread count: {ex.Message}", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> IsEmailValidAsync(string email)
        {
            try
            {
                // In a real implementation, this would validate email format and check if it exists
                var isValid = !string.IsNullOrEmpty(email) && email.Contains("@");
                return new JsonModel { data = isValid, Message = "Success", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating email {Email}", email);
                return new JsonModel { data = new object(), Message = $"Failed to validate email: {ex.Message}", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                // In a real implementation, this would use an SMS service like Twilio, AWS SNS, etc.
                _logger.LogInformation("Sending SMS to {PhoneNumber}: {Message}", phoneNumber, message);
                
                // Simulate SMS sending
                await Task.Delay(100);
                
                return new JsonModel { data = true, Message = "Success", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
                return new JsonModel { data = new object(), Message = $"Failed to send SMS: {ex.Message}", StatusCode = 500 };
            }
        }

        public Task SendNotificationAsync(string userId, string title, string message) => throw new NotImplementedException();

        public async Task<JsonModel> CreateInAppNotificationAsync(int userId, string title, string message)
        {
            try
            {
                // In a real implementation, this would save to a notification repository
                var notification = new NotificationDto
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId.ToString(),
                    Title = title,
                    Message = message,
                    Type = "InApp",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                return new JsonModel { data = notification, Message = "Success", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating in-app notification for user {UserId}", userId);
                return new JsonModel { data = new object(), Message = $"Failed to create in-app notification: {ex.Message}", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetUserNotificationsAsync(int userId)
        {
            try
            {
                // In a real implementation, this would fetch from a notification repository
                var notifications = new List<NotificationDto>();
                return new JsonModel { data = .SuccessResponse(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                return new JsonModel { data = new object(), Message = $"Failed to get user notifications: {ex.Message}", StatusCode = 500 };
            }
        }

        public async Task<JsonModel> GetUnreadNotificationCountAsync(int userId)
        {
            try
            {
                // In a real implementation, this would fetch from a notification repository
                var unreadCount = 0;
                return new JsonModel { data = unreadCount, Message = "Success", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notification count for user {UserId}", userId);
                return new JsonModel { data = new object(), Message = $"Failed to get unread notification count: {ex.Message}", StatusCode = 500 };
            }
        }

        public async Task SendSubscriptionSuspensionEmailAsync(string email, string userName, SubscriptionDto subscription)
        {
            // If using dependency injection, call the infrastructure service or implement logic here
            // For now, just a stub to match the interface and avoid build error
            // You should call the actual email sending logic here if needed
            await Task.CompletedTask;
        }

        public async Task SendSubscriptionSuspendedNotificationAsync(string userId, string subscriptionId)
        {
            try
            {
                var notification = new CreateNotificationDto
                {
                    UserId = int.Parse(userId),
                    Title = "Subscription Suspended",
                    Message = $"Your subscription {subscriptionId} has been suspended due to payment issues. Please update your payment method to reactivate your subscription.",
                    Type = "Warning"
                };

                await CreateNotificationAsync(notification);
                _logger.LogInformation("Sent subscription suspended notification to user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending subscription suspended notification to user {UserId}", userId);
            }
        }

        public async Task SendRefundNotificationAsync(string userId, decimal amount, string billingRecordId)
        {
            try
            {
                var notification = new CreateNotificationDto
                {
                    UserId = int.Parse(userId),
                    Title = "Refund Processed",
                    Message = $"A refund of ${amount:F2} has been processed for billing record {billingRecordId}. The refund will appear in your account within 3-5 business days.",
                    Type = "Success"
                };

                await CreateNotificationAsync(notification);
                _logger.LogInformation("Sent refund notification to user {UserId} for amount {Amount}", userId, amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending refund notification to user {UserId}", userId);
            }
        }

        public async Task SendSubscriptionReactivatedNotificationAsync(string userId, string subscriptionId)
        {
            try
            {
                var notification = new CreateNotificationDto
                {
                    UserId = int.Parse(userId),
                    Title = "Subscription Reactivated",
                    Message = $"Your subscription {subscriptionId} has been reactivated successfully. You now have full access to all features.",
                    Type = "Success"
                };

                await CreateNotificationAsync(notification);
                _logger.LogInformation("Sent subscription reactivated notification to user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending subscription reactivated notification to user {UserId}", userId);
            }
        }

        // EMAIL FUNCTIONALITY DISABLED - SendEmailAsync method removed
        // TODO: Re-enable email functionality when needed
    }
} 