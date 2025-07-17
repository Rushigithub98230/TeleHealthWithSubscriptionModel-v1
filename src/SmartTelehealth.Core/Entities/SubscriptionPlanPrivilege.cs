using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class SubscriptionPlanPrivilege : BaseEntity
{
    [Required]
    public Guid SubscriptionPlanId { get; set; }
    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;

    [Required]
    public Guid PrivilegeId { get; set; }
    public virtual Privilege Privilege { get; set; } = null!;

    [MaxLength(100)]
    public string Value { get; set; } = string.Empty; // e.g., "2" for 2 consults, "true" for medication supply
} 