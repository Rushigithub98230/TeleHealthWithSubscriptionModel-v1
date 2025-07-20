using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class UserSubscriptionPrivilegeUsage : BaseEntity
{
    [Required]
    public Guid SubscriptionId { get; set; }
    public virtual Subscription Subscription { get; set; } = null!;

    [Required]
    public Guid SubscriptionPlanPrivilegeId { get; set; }
    public virtual SubscriptionPlanPrivilege SubscriptionPlanPrivilege { get; set; } = null!;

    public int UsedValue { get; set; } = 0;
    public DateTime UsagePeriodStart { get; set; }
    public DateTime UsagePeriodEnd { get; set; }
} 