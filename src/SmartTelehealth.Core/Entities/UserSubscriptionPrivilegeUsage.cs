using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class UserSubscriptionPrivilegeUsage : BaseEntity
{
    [Required]
    public Guid SubscriptionId { get; set; }
    public virtual Subscription Subscription { get; set; } = null!;

    [Required]
    public Guid PrivilegeId { get; set; }
    public virtual Privilege Privilege { get; set; } = null!;

    [MaxLength(100)]
    public string UsedValue { get; set; } = "0"; // e.g., number of consults used
} 