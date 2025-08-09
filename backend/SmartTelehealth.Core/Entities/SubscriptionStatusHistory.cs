using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class SubscriptionStatusHistory : BaseEntity
{
    [Required]
    public Guid SubscriptionId { get; set; }
    public virtual Subscription Subscription { get; set; } = null!;
    
    [MaxLength(50)]
    public string? FromStatus { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string ToStatus { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Reason { get; set; }
    
    [MaxLength(100)]
    public string? ChangedByUserId { get; set; }
    
    [Required]
    public DateTime ChangedAt { get; set; }
    
    [MaxLength(1000)]
    public string? Metadata { get; set; }
    
                // Computed properties (not mapped to database)
            public bool IsStatusChange => !string.IsNullOrEmpty(FromStatus) && FromStatus != ToStatus;

            public TimeSpan DurationInPreviousStatus => FromStatus != null ? ChangedAt - CreatedAt : TimeSpan.Zero;
} 