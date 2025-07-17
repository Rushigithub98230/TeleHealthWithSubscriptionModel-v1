using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.DTOs;

// Chat Notification DTO - This is unique to ChatDtos.cs
public class ChatNotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ChatRoomId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
} 