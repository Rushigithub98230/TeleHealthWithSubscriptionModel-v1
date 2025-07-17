using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class ChatRoomParticipant
{
    public enum ParticipantStatus
    {
        Active,
        Inactive,
        Banned,
        Left
    }

    public enum ParticipantRole
    {
        Member,
        Admin,
        Moderator,
        Guest,
        Provider,
        Patient,
        External
    }

    public Guid Id { get; set; }
    public Guid ChatRoomId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public ParticipantStatus Status { get; set; } = ParticipantStatus.Active;
    public bool CanSendMessages { get; set; } = true;
    public bool CanSendFiles { get; set; } = true;
    public bool CanInviteOthers { get; set; } = false;
    public bool CanModerate { get; set; } = false;
    public DateTime JoinedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public Guid? ProviderId { get; set; }
    public DateTime? LeftAt { get; set; }
    // Navigation properties
    public virtual ChatRoom? ChatRoom { get; set; }
    public virtual User? User { get; set; }
    public virtual Provider? Provider { get; set; }
} 