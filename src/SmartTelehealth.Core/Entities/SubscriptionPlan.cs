using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class SubscriptionPlan : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public decimal MonthlyPrice { get; set; }
    
    public decimal QuarterlyPrice { get; set; }
    
    public decimal AnnualPrice { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int DisplayOrder { get; set; }
    
    // Remove all commented-out privilege-specific columns (ConsultationCount, MessagingCount, IncludesMedicationDelivery, IncludesFollowUpCare, IncludesPrioritySupport, IncludesVideoCalls, ConsultationFee, DeliveryFrequencyDays, MaxPauseDurationDays, Features)
    // Add navigation property for privileges
    public virtual ICollection<SubscriptionPlanPrivilege> PlanPrivileges { get; set; } = new List<SubscriptionPlanPrivilege>();
    
    // Stripe Integration
    public string? StripeProductId { get; set; }
    public string? StripeMonthlyPriceId { get; set; }
    public string? StripeQuarterlyPriceId { get; set; }
    public string? StripeAnnualPriceId { get; set; }
    
    // Foreign key
    public Guid CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
    
    // Navigation properties
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
} 