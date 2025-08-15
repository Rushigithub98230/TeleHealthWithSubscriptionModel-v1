using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System.Net.Mail;
using System.Net;

namespace SmartTelehealth.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    
    public NotificationService(
        IConfiguration configuration, 
        ILogger<NotificationService> logger,
        INotificationRepository notificationRepository,
        IUserRepository userRepository)
    {
        _configuration = configuration;
        _logger = logger;
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
    }
    
    // Email notifications with SMTP implementation
    public async Task SendWelcomeEmailAsync(string email, string userName)
    {
        try
        {
            var subject = "Welcome to Smart Telehealth!";
            var body = $@"
                <h2>Welcome to Smart Telehealth, {userName}!</h2>
                <p>Thank you for joining our platform. We're excited to provide you with quality healthcare services.</p>
                <p>If you have any questions, please don't hesitate to contact our support team.</p>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent welcome email to {Email} for user {UserName}", email, userName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending welcome email to {Email}", email);
            throw;
        }
    }
    
    public async Task SendSubscriptionConfirmationAsync(string email, string userName, SubscriptionDto subscription)
    {
        try
        {
            var subject = "Subscription Confirmation";
            var body = $@"
                <h2>Subscription Confirmed!</h2>
                <p>Hello {userName},</p>
                <p>Your subscription to <strong>{subscription.PlanName}</strong> has been successfully activated.</p>
                <p><strong>Subscription Details:</strong></p>
                <ul>
                    <li>Plan: {subscription.PlanName}</li>
                    <li>Status: {subscription.Status}</li>
                    <li>Billing Cycle: {subscription.BillingCycleId}</li>
                    <li>Price: ${subscription.CurrentPrice}</li>
                </ul>
                <p>You can now access all the features included in your subscription.</p>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent subscription confirmation to {Email} for subscription {SubscriptionId}", email, subscription.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending subscription confirmation to {Email}", email);
            throw;
        }
    }
    
    public async Task SendPaymentReminderAsync(string email, string userName, BillingRecordDto billingRecord)
    {
        try
        {
            var subject = "Payment Reminder";
            var body = $@"
                <h2>Payment Reminder</h2>
                <p>Hello {userName},</p>
                <p>This is a friendly reminder that your payment of <strong>${billingRecord.Amount}</strong> is due on {billingRecord.DueDate:MM/dd/yyyy}.</p>
                <p><strong>Payment Details:</strong></p>
                <ul>
                    <li>Amount: ${billingRecord.Amount}</li>
                    <li>Due Date: {billingRecord.DueDate:MM/dd/yyyy}</li>
                    <li>Description: {billingRecord.Description}</li>
                </ul>
                <p>Please log in to your account to make the payment.</p>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent payment reminder to {Email} for billing record {BillingRecordId}", email, billingRecord.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment reminder to {Email}", email);
            throw;
        }
    }
    
    // Billing email notifications
    public async Task SendPaymentSuccessEmailAsync(string email, string userName, BillingRecordDto billingRecord)
    {
        try
        {
            var subject = "Payment Successful";
            var body = $@"
                <h2>Payment Successful!</h2>
                <p>Hello {userName},</p>
                <p>Your payment of <strong>${billingRecord.Amount}</strong> has been processed successfully.</p>
                <p><strong>Payment Details:</strong></p>
                <ul>
                    <li>Amount: ${billingRecord.Amount}</li>
                    <li>Date: {billingRecord.PaidDate?.ToString("MM/dd/yyyy") ?? "Pending"}</li>
                    <li>Description: {billingRecord.Description}</li>
                </ul>
                <p>Thank you for your payment!</p>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent payment success email to {Email} for billing record {BillingRecordId}", email, billingRecord.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment success email to {Email}", email);
            throw;
        }
    }
    
    public async Task SendPaymentFailedEmailAsync(string email, string userName, BillingRecordDto billingRecord)
    {
        try
        {
            var subject = "Payment Failed";
            var body = $@"
                <h2>Payment Failed</h2>
                <p>Hello {userName},</p>
                <p>We were unable to process your payment of <strong>${billingRecord.Amount}</strong>.</p>
                <p><strong>Payment Details:</strong></p>
                <ul>
                    <li>Amount: ${billingRecord.Amount}</li>
                    <li>Due Date: {billingRecord.DueDate:MM/dd/yyyy}</li>
                    <li>Description: {billingRecord.Description}</li>
                </ul>
                <p>Please check your payment method and try again, or contact our support team for assistance.</p>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent payment failed email to {Email} for billing record {BillingRecordId}", email, billingRecord.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment failed email to {Email}", email);
            throw;
        }
    }
    
    public async Task SendRefundProcessedEmailAsync(string email, string userName, BillingRecordDto billingRecord, decimal refundAmount)
    {
        try
        {
            var subject = "Refund Processed";
            var body = $@"
                <h2>Refund Processed</h2>
                <p>Hello {userName},</p>
                <p>Your refund of <strong>${refundAmount}</strong> has been processed successfully.</p>
                <p><strong>Refund Details:</strong></p>
                <ul>
                    <li>Original Amount: ${billingRecord.Amount}</li>
                    <li>Refund Amount: ${refundAmount}</li>
                    <li>Date: {DateTime.UtcNow:MM/dd/yyyy}</li>
                    <li>Description: {billingRecord.Description}</li>
                </ul>
                <p>The refund will be credited to your original payment method within 3-5 business days.</p>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent refund processed email to {Email} for billing record {BillingRecordId}", email, billingRecord.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending refund processed email to {Email}", email);
            throw;
        }
    }
    
    public async Task SendOverduePaymentEmailAsync(string email, string userName, BillingRecordDto billingRecord)
    {
        try
        {
            var subject = "Payment Overdue";
            var body = $@"
                <h2>Payment Overdue</h2>
                <p>Hello {userName},</p>
                <p>Your payment of <strong>${billingRecord.Amount}</strong> is overdue.</p>
                <p><strong>Payment Details:</strong></p>
                <ul>
                    <li>Amount: ${billingRecord.Amount}</li>
                    <li>Due Date: {billingRecord.DueDate:MM/dd/yyyy}</li>
                    <li>Days Overdue: {(int)((billingRecord.DueDate.HasValue ? (DateTime.UtcNow - billingRecord.DueDate.Value) : TimeSpan.Zero).TotalDays)}</li>
                    <li>Description: {billingRecord.Description}</li>
                </ul>
                <p>Please make the payment as soon as possible to avoid any service interruptions.</p>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent overdue payment email to {Email} for billing record {BillingRecordId}", email, billingRecord.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending overdue payment email to {Email}", email);
            throw;
        }
    }
    
    // In-app notifications
    public async Task<JsonModel> CreateInAppNotificationAsync(int userId, string title, string message)
    {
        try
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Message = message,
                Type = NotificationType.InApp,
                Status = NotificationStatus.Unread,
                CreatedDate = DateTime.UtcNow,
                ScheduledAt = DateTime.UtcNow
            };

            var createdNotification = await _notificationRepository.CreateAsync(notification);
            var notificationDto = new NotificationDto
            {
                Id = createdNotification.Id.ToString(),
                UserId = createdNotification.UserId.ToString(),
                Title = createdNotification.Title,
                Message = createdNotification.Message,
                Type = createdNotification.Type.ToString(),
                Status = createdNotification.Status.ToString(),
                IsRead = createdNotification.IsRead,
                CreatedAt = createdNotification.CreatedDate ?? DateTime.UtcNow,
                ReadAt = createdNotification.ReadAt,
                ScheduledAt = createdNotification.ScheduledAt
            };

            return new JsonModel { data = notificationDto, Message = "In-app notification created successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating in-app notification for user {UserId}", userId);
            return new JsonModel { data = new object(), Message = "Error creating in-app notification", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetUserNotificationsAsync(int userId)
    {
        try
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId);
            var notificationDtos = notifications.Select(n => new NotificationDto
            {
                Id = n.Id.ToString(),
                UserId = n.UserId.ToString(),
                Title = n.Title,
                Message = n.Message,
                Type = n.Type.ToString(),
                Status = n.Status.ToString(),
                IsRead = n.IsRead,
                CreatedAt = n.CreatedDate ?? DateTime.UtcNow,
                ReadAt = n.ReadAt,
                ScheduledAt = n.ScheduledAt
            });

            return new JsonModel { data = notificationDtos, Message = "User notifications retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
            return new JsonModel { data = new object(), Message = "Error retrieving user notifications", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> MarkNotificationAsReadAsync(Guid notificationId)
    {
        try
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null)
                return new JsonModel { data = new object(), Message = "Notification not found", StatusCode = 404 };

            notification.Status = NotificationStatus.Read;
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _notificationRepository.UpdateAsync(notification);

            return new JsonModel { data = true, Message = "Notification marked as read successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read {NotificationId}", notificationId);
            return new JsonModel { data = new object(), Message = "Error marking notification as read", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetUnreadNotificationCountAsync(int userId)
    {
        try
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId);
            var unreadCount = notifications.Count(n => n.Status == NotificationStatus.Unread);

            return new JsonModel { data = unreadCount, Message = "Unread notification count retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread notification count for user {UserId}", userId);
            return new JsonModel { data = new object(), Message = "Error retrieving unread notification count", StatusCode = 500 };
        }
    }
    
    // Existing methods (keeping for compatibility)
    public async Task SendConsultationReminderAsync(string email, string userName, ConsultationDto consultation)
    {
        try
        {
            var subject = "Consultation Reminder";
            var body = $@"
                <h2>Consultation Reminder</h2>
                <p>Hello {userName},</p>
                <p>This is a reminder for your upcoming consultation.</p>
                <p><strong>Consultation Details:</strong></p>
                <ul>
                    <li>Provider: {consultation.ProviderName}</li>
                    <li>Date: {consultation.ScheduledAt:MM/dd/yyyy}</li>
                    <li>Time: {consultation.ScheduledAt:HH:mm}</li>
                </ul>
                <p>Please log in to your account 5 minutes before the scheduled time.</p>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent consultation reminder to {Email} for consultation {ConsultationId}", email, consultation.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending consultation reminder to {Email}", email);
            throw;
        }
    }
    
    public async Task SendPasswordResetEmailAsync(string email, string resetToken)
    {
        try
        {
            var subject = "Password Reset Request";
            var body = $@"
                <h2>Password Reset Request</h2>
                <p>You have requested to reset your password.</p>
                <p>Click the link below to reset your password:</p>
                <p><a href='{_configuration["AppUrl"]}/reset-password?token={resetToken}'>Reset Password</a></p>
                <p>If you didn't request this, please ignore this email.</p>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent password reset email to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to {Email}", email);
            throw;
        }
    }
    
    public async Task SendDeliveryNotificationAsync(string email, string userName, MedicationDeliveryDto delivery)
    {
        try
        {
            var subject = "Medication Delivery Update";
            var body = $@"
                <h2>Medication Delivery Update</h2>
                <p>Hello {userName},</p>
                <p>Your medication delivery has been updated.</p>
                <p><strong>Delivery Details:</strong></p>
                <ul>
                    <li>Medication: {delivery.MedicationName}</li>
                    <li>Tracking Number: {delivery.TrackingNumber}</li>
                    <li>Status: {delivery.Status}</li>
                </ul>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent delivery notification to {Email} for delivery {DeliveryId}", email, delivery.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending delivery notification to {Email}", email);
            throw;
        }
    }
    
    public async Task SendSubscriptionPausedNotificationAsync(string email, string userName, SubscriptionDto subscription)
    {
        try
        {
            var subject = "Subscription Paused";
            var body = $@"
                <h2>Subscription Paused</h2>
                <p>Hello {userName},</p>
                <p>Your subscription to <strong>{subscription.PlanName}</strong> has been paused.</p>
                <p>You can resume your subscription at any time by logging into your account.</p>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent subscription paused notification to {Email} for subscription {SubscriptionId}", email, subscription.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending subscription paused notification to {Email}", email);
            throw;
        }
    }
    
    public async Task SendSubscriptionResumedNotificationAsync(string email, string userName, SubscriptionDto subscription)
    {
        try
        {
            var subject = "Subscription Resumed";
            var body = $@"
                <h2>Subscription Resumed</h2>
                <p>Hello {userName},</p>
                <p>Your subscription to <strong>{subscription.PlanName}</strong> has been resumed.</p>
                <p>You now have access to all the features included in your subscription.</p>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent subscription resumed notification to {Email} for subscription {SubscriptionId}", email, subscription.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending subscription resumed notification to {Email}", email);
            throw;
        }
    }
    
    public async Task SendSubscriptionCancelledNotificationAsync(string email, string userName, SubscriptionDto subscription)
    {
        try
        {
            var subject = "Subscription Cancelled";
            var body = $@"
                <h2>Subscription Cancelled</h2>
                <p>Hello {userName},</p>
                <p>Your subscription to <strong>{subscription.PlanName}</strong> has been cancelled.</p>
                <p>You can reactivate your subscription at any time by logging into your account.</p>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent subscription cancelled notification to {Email} for subscription {SubscriptionId}", email, subscription.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending subscription cancelled notification to {Email}", email);
            throw;
        }
    }
    
    public async Task SendProviderMessageNotificationAsync(string email, string userName, MessageDto message)
    {
        try
        {
            var subject = "New Message from Provider";
            var body = $@"
                <h2>New Message</h2>
                <p>Hello {userName},</p>
                <p>You have received a new message from your provider.</p>
                <p><strong>Message:</strong></p>
                <p>{message.Content}</p>
                <p>Please log in to your account to view the complete message.</p>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent provider message notification to {Email} for message {MessageId}", email, message.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending provider message notification to {Email}", email);
            throw;
        }
    }
    
    public async Task<JsonModel> IsEmailValidAsync(string email)
    {
        try
        {
            // Simple email validation
            var isValid = !string.IsNullOrWhiteSpace(email) && 
                         email.Contains("@") && 
                         email.Contains(".") &&
                         email.Length > 5;

            return new JsonModel { data = isValid, Message = "Email validation completed", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating email {Email}", email);
            return new JsonModel { data = new object(), Message = "Error validating email", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            // Placeholder SMS implementation
            // In a real application, this would integrate with an SMS service like Twilio
            _logger.LogInformation("SMS sent to {PhoneNumber}: {Message}", phoneNumber, message);
            
            return new JsonModel { data = true, Message = "SMS sent successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
            return new JsonModel { data = new object(), Message = "Error sending SMS", StatusCode = 500 };
        }
    }
    
    // EMAIL FUNCTIONALITY DISABLED - SendEmailAsync method removed
    // TODO: Re-enable email functionality when needed

    public async Task<JsonModel> GetNotificationsAsync()
    {
        try
        {
            var notifications = await _notificationRepository.GetAllAsync();
            var dtos = notifications.Select(n => new NotificationDto
            {
                Id = n.Id.ToString(),
                UserId = n.UserId.ToString(),
                Title = n.Title,
                Message = n.Message,
                Type = n.Type.ToString(),
                Status = n.Status.ToString(),
                IsRead = n.IsRead,
                CreatedAt = n.CreatedDate ?? DateTime.UtcNow,
                ReadAt = n.ReadAt,
                ScheduledAt = n.ScheduledAt
            });
            return new JsonModel { data = .SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all notifications");
            return new JsonModel { data = new object(), Message = "An error occurred while retrieving notifications", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetNotificationAsync(Guid id)
    {
        try
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
                return new JsonModel { data = new object(), Message = "Notification not found", StatusCode = 404 };
            var dto = new NotificationDto
            {
                Id = notification.Id.ToString(),
                UserId = notification.UserId.ToString(),
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type.ToString(),
                Status = notification.Status.ToString(),
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedDate ?? DateTime.UtcNow,
                ReadAt = notification.ReadAt,
                ScheduledAt = notification.ScheduledAt
            };
            return new JsonModel { data = dto, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification {Id}", id);
            return new JsonModel { data = new object(), Message = "An error occurred while retrieving the notification", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> CreateNotificationAsync(CreateNotificationDto createNotificationDto)
    {
        try
        {
            var notification = new Notification
            {
                UserId = createNotificationDto.UserId,
                Title = createNotificationDto.Title,
                Message = createNotificationDto.Message,
                Type = Enum.TryParse<NotificationType>(createNotificationDto.Type, out var type) ? type : NotificationType.InApp,
                Status = NotificationStatus.Unread,
                IsRead = false,
                CreatedDate = DateTime.UtcNow,
                ScheduledAt = createNotificationDto.ScheduledAt
            };
            var created = await _notificationRepository.CreateAsync(notification);
            var dto = new NotificationDto
            {
                Id = created.Id.ToString(),
                UserId = created.UserId.ToString(),
                Title = created.Title,
                Message = created.Message,
                Type = created.Type.ToString(),
                Status = created.Status.ToString(),
                IsRead = created.IsRead,
                CreatedAt = created.CreatedDate ?? DateTime.UtcNow,
                ReadAt = created.ReadAt,
                ScheduledAt = created.ScheduledAt
            };
            return new JsonModel { data = dto, Message = "Notification created", 201, StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification");
            return new JsonModel { data = new object(), Message = "An error occurred while creating the notification", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdateNotificationAsync(Guid id, UpdateNotificationDto updateNotificationDto)
    {
        try
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
                return new JsonModel { data = new object(), Message = "Notification not found", StatusCode = 404 };
            if (!string.IsNullOrEmpty(updateNotificationDto.Title))
                notification.Title = updateNotificationDto.Title;
            if (!string.IsNullOrEmpty(updateNotificationDto.Message))
                notification.Message = updateNotificationDto.Message;
            if (updateNotificationDto.IsRead.HasValue)
                notification.IsRead = updateNotificationDto.IsRead.Value;
            if (updateNotificationDto.ScheduledAt.HasValue)
                notification.ScheduledAt = updateNotificationDto.ScheduledAt.Value;
            var updatedNotification = await _notificationRepository.UpdateAsync(notification);
            var notificationDto = new NotificationDto
            {
                Id = updatedNotification.Id.ToString(),
                UserId = updatedNotification.UserId.ToString(),
                Title = updatedNotification.Title,
                Message = updatedNotification.Message,
                Type = updatedNotification.Type.ToString(),
                Status = updatedNotification.Status.ToString(),
                IsRead = updatedNotification.IsRead,
                CreatedAt = updatedNotification.CreatedDate ?? DateTime.UtcNow,
                ReadAt = updatedNotification.ReadAt,
                ScheduledAt = updatedNotification.ScheduledAt
            };
            return new JsonModel { data = notificationDto, Message = "Notification updated", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification {Id}", id);
            return new JsonModel { data = new object(), Message = "An error occurred while updating the notification", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DeleteNotificationAsync(Guid id)
    {
        try
        {
            var result = await _notificationRepository.DeleteAsync(id);
            if (!result)
                return new JsonModel { data = new object(), Message = "Notification not found", StatusCode = 404 };
            return new JsonModel { data = true, Message = "Notification deleted", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification {Id}", id);
            return new JsonModel { data = new object(), Message = "An error occurred while deleting the notification", StatusCode = 500 };
        }
    }

    public Task<JsonModel> UpdateNotificationAsync(Guid id, object updateNotificationDto) => throw new NotImplementedException();
    public async Task SendEmailVerificationAsync(string email, string userName, string verificationToken)
    {
        try
        {
            var subject = "Email Verification";
            var body = $@"
                <h2>Email Verification</h2>
                <p>Hello {userName},</p>
                <p>Please verify your email address by clicking the link below:</p>
                <p><a href='{_configuration["AppUrl"]}/verify-email?token={verificationToken}'>Verify Email</a></p>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent email verification to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email verification to {Email}", email);
            throw;
        }
    }

    public async Task SendSubscriptionWelcomeEmailAsync(string email, string userName, SubscriptionDto subscription)
    {
        try
        {
            var subject = "Welcome to Smart Telehealth";
            var body = $@"
                <h2>Welcome to Smart Telehealth!</h2>
                <p>Hello {userName},</p>
                <p>Thank you for subscribing to <strong>{subscription.PlanName}</strong>.</p>
                <p>Your subscription is now active and you can start using our services.</p>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent subscription welcome email to {Email} for subscription {SubscriptionId}", email, subscription.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending subscription welcome email to {Email}", email);
            throw;
        }
    }
    public async Task SendSubscriptionCancellationEmailAsync(string email, string userName, SubscriptionDto subscription)
    {
        try
        {
            var subject = "Subscription Cancelled";
            var body = $@"
                <h2>Subscription Cancelled</h2>
                <p>Hello {userName},</p>
                <p>Your subscription to <strong>{subscription.PlanName}</strong> has been cancelled.</p>
                <p>If you have any questions or would like to reactivate your subscription, please contact support.</p>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent subscription cancellation email to {Email} for subscription {SubscriptionId}", email, subscription.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending subscription cancellation email to {Email}", email);
            throw;
        }
    }
    public async Task SendSubscriptionSuspensionEmailAsync(string email, string userName, SubscriptionDto subscription)
    {
        try
        {
            var subject = "Subscription Suspended";
            var body = $@"
                <h2>Subscription Suspended</h2>
                <p>Hello {userName},</p>
                <p>Your subscription to <strong>{subscription.PlanName}</strong> has been suspended.</p>
                <p>If you believe this is a mistake or have questions, please contact support.</p>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent subscription suspension email to {Email} for subscription {SubscriptionId}", email, subscription.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending subscription suspension email to {Email}", email);
            throw;
        }
    }

    public async Task SendNotificationAsync(string userId, string title, string message)
    {
        try
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = int.Parse(userId),
                Title = title,
                Message = message,
                Type = NotificationType.InApp,
                Status = NotificationStatus.Unread,
                IsRead = false,
                CreatedDate = DateTime.UtcNow,
                ScheduledAt = DateTime.UtcNow
            };
            await _notificationRepository.CreateAsync(notification);
            _logger.LogInformation("In-app notification sent to {UserId}: {Title}", userId, title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending in-app notification to {UserId}", userId);
            throw;
        }
    }

    public async Task SendSubscriptionSuspendedNotificationAsync(string userId, string subscriptionId)
    {
        try
        {
            // Get user details from the user service
            var user = await _userRepository.GetByIdAsync(int.Parse(userId));
            if (user == null) return;

            var subject = "Subscription Suspended";
            var body = $@"
                <h2>Subscription Suspended</h2>
                <p>Hello {user.FullName},</p>
                <p>Your subscription (ID: {subscriptionId}) has been suspended due to payment issues.</p>
                <p>Please update your payment method to reactivate your subscription.</p>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(user.Email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent subscription suspended notification to {Email} for subscription {SubscriptionId}", user.Email, subscriptionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending subscription suspended notification to user {UserId} for subscription {SubscriptionId}", userId, subscriptionId);
            throw;
        }
    }

    public async Task SendRefundNotificationAsync(string userId, decimal amount, string billingRecordId)
    {
        try
        {
            // Get user details from the user service
            var user = await _userRepository.GetByIdAsync(int.Parse(userId));
            if (user == null) return;

            var subject = "Refund Processed";
            var body = $@"
                <h2>Refund Processed</h2>
                <p>Hello {user.FullName},</p>
                <p>A refund of <strong>${amount}</strong> has been processed for billing record {billingRecordId}.</p>
                <p>The refund will appear in your account within 3-5 business days.</p>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(user.Email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent refund notification to {Email} for amount {Amount} and billing record {BillingRecordId}", user.Email, amount, billingRecordId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending refund notification to user {UserId} for amount {Amount} and billing record {BillingRecordId}", userId, amount, billingRecordId);
            throw;
        }
    }

    public async Task SendSubscriptionReactivatedNotificationAsync(string userId, string subscriptionId)
    {
        try
        {
            // Get user details from the user service
            var user = await _userRepository.GetByIdAsync(int.Parse(userId));
            if (user == null) return;

            var subject = "Subscription Reactivated";
            var body = $@"
                <h2>Subscription Reactivated</h2>
                <p>Hello {user.FullName},</p>
                <p>Your subscription (ID: {subscriptionId}) has been successfully reactivated.</p>
                <p>You now have access to all your subscription benefits.</p>
                <br>
                <p>Best regards,<br>Smart Telehealth Team</p>";
            
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await SendEmailAsync(user.Email, subject, body);
            _logger.LogInformation("Email sending disabled - would have sent subscription reactivated notification to {Email} for subscription {SubscriptionId}", user.Email, subscriptionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending subscription reactivated notification to user {UserId} for subscription {SubscriptionId}", userId, subscriptionId);
            throw;
        }
    }
} 