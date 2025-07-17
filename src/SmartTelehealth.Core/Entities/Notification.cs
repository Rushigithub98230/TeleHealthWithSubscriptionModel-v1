using System;

namespace SmartTelehealth.Core.Entities;

public enum NotificationType
{
    InApp,
    Email,
    Sms
}

public enum NotificationStatus
{
    Unread,
    Read,
    Archived
}

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.InApp;
    public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public DateTime? ScheduledAt { get; set; }
} 