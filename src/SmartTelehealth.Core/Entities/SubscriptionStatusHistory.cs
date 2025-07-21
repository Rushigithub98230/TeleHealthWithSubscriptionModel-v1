using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class SubscriptionStatusHistory : BaseEntity
{
    [Required]
    public Guid SubscriptionId { get; set; }
    public virtual Subscription Subscription { get; set; } = null!;
    
    [Required]
    [MaxLength(50)]
    public string FromStatus { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string ToStatus { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Reason { get; set; }
    
    public Guid? ChangedByUserId { get; set; }
    public virtual User? ChangedByUser { get; set; }
    
    [Required]
    public DateTime ChangedAt { get; set; }
    
    [MaxLength(1000)]
    public string? Metadata { get; set; } // JSON for additional context
} 