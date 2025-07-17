using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class Privilege : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string DataType { get; set; } = "int"; // e.g., int, bool, string

    [MaxLength(100)]
    public string? DefaultValue { get; set; }

    // Navigation
    public virtual ICollection<SubscriptionPlanPrivilege> PlanPrivileges { get; set; } = new List<SubscriptionPlanPrivilege>();
    public virtual ICollection<UserSubscriptionPrivilegeUsage> UsageRecords { get; set; } = new List<UserSubscriptionPrivilegeUsage>();
} 