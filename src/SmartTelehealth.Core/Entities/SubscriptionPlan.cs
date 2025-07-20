using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class SubscriptionPlan : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
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
    public Guid BillingCycleId { get; set; }
    public virtual MasterBillingCycle BillingCycle { get; set; } = null!;

    public Guid CurrencyId { get; set; }
    public virtual MasterCurrency Currency { get; set; } = null!;

    public decimal Price { get; set; }
    
    // Navigation properties
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
} 