using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class ChatRoomInvitation : BaseEntity
{
    public enum InvitationStatus
    {
        Pending,
        Accepted,
        Declined,
        Expired
    }

    // Foreign keys
    public Guid ChatRoomId { get; set; }
    public virtual ChatRoom ChatRoom { get; set; } = null!;

    public Guid InvitedByUserId { get; set; }
    public virtual User InvitedByUser { get; set; } = null!;

    public Guid InvitedUserId { get; set; }
    public virtual User InvitedUser { get; set; } = null!;

    // Invitation information
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    [MaxLength(500)]
    public string? Message { get; set; }

    public DateTime ExpiresAt { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
} 