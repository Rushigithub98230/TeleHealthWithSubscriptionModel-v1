using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class MessageReadReceipt : BaseEntity
{
    // Foreign keys
    public Guid MessageId { get; set; }
    public virtual Message Message { get; set; } = null!;

    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public Guid? ProviderId { get; set; }
    public virtual Provider? Provider { get; set; }

    // Read receipt details
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    // Device information for audit
    [MaxLength(100)]
    public string? DeviceInfo { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
} 