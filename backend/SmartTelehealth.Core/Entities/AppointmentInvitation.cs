using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class AppointmentInvitation : BaseEntity
{
    // Foreign keys
    public Guid AppointmentId { get; set; }
    public virtual Appointment Appointment { get; set; } = null!;

    public Guid InvitedByUserId { get; set; }
    public virtual User InvitedByUser { get; set; } = null!;

    // Internal or external invitee
    public Guid? InvitedUserId { get; set; }
    public virtual User? InvitedUser { get; set; }
    [MaxLength(256)]
    public string? InvitedEmail { get; set; }
    [MaxLength(32)]
    public string? InvitedPhone { get; set; }

    public Guid InvitationStatusId { get; set; }
    public virtual InvitationStatus? InvitationStatus { get; set; }

    [MaxLength(500)]
    public string? Message { get; set; }

    public DateTime ExpiresAt { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
} 