using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class MessageReaction : BaseEntity
{
    // Foreign keys
    public Guid MessageId { get; set; }
    public virtual Message Message { get; set; } = null!;

    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public Guid? ProviderId { get; set; }
    public virtual Provider? Provider { get; set; }

    // Reaction details
    [Required]
    [MaxLength(10)]
    public string Emoji { get; set; } = string.Empty;

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
} 