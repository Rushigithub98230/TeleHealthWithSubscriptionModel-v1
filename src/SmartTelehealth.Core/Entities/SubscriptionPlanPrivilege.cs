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

    public int Value { get; set; } // -1 for unlimited, >0 for limited
    public Guid UsagePeriodId { get; set; }
    public virtual MasterBillingCycle UsagePeriod { get; set; } = null!;
    public int DurationMonths { get; set; }
} 