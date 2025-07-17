using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class Subscription : BaseEntity
{
    public enum SubscriptionStatus
    {
        Active,
        Paused,
        Cancelled,
        Expired,
        Pending,
        PastDue
    }
    
    public enum SubscriptionBillingFrequency
    {
        Monthly,
        Quarterly,
        Annual
    }
    
    // Foreign keys
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    public Guid SubscriptionPlanId { get; set; }
    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    
    public Guid? ProviderId { get; set; }
    public virtual Provider? Provider { get; set; }
    
    // Subscription details
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;
    
    public SubscriptionBillingFrequency BillingFrequency { get; set; } = SubscriptionBillingFrequency.Monthly;
    
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public DateTime NextBillingDate { get; set; }
    
    public DateTime? PausedDate { get; set; }
    
    public DateTime? CancelledDate { get; set; }
    
    public decimal CurrentPrice { get; set; }
    
    public bool AutoRenew { get; set; } = true;
    
    [MaxLength(500)]
    public string? CancellationReason { get; set; }
    
    [MaxLength(500)]
    public string? PauseReason { get; set; }
    
    // Stripe integration
    [MaxLength(100)]
    public string? StripeSubscriptionId { get; set; }
    
    [MaxLength(100)]
    public string? StripeCustomerId { get; set; }
    
    // Additional properties for service compatibility
    public DateTime? PausedAt { get; set; }
    public DateTime? ResumedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    public virtual ICollection<MedicationDelivery> MedicationDeliveries { get; set; } = new List<MedicationDelivery>();
    public virtual ICollection<BillingRecord> BillingRecords { get; set; } = new List<BillingRecord>();
    public virtual ICollection<UserSubscriptionPrivilegeUsage> PrivilegeUsages { get; set; } = new List<UserSubscriptionPrivilegeUsage>();
    
    public bool IsActive => Status == SubscriptionStatus.Active;
    public bool IsPaused { get => Status == SubscriptionStatus.Paused; set { } }
    public bool IsCancelled => Status == SubscriptionStatus.Cancelled;
    
    // Remove MaxPauseDurationDays logic as the property no longer exists
    public bool CanPause => Status == SubscriptionStatus.Active && (PausedDate == null);
    
    public bool CanResume => Status == SubscriptionStatus.Paused;
    
    public bool CanCancel => Status == SubscriptionStatus.Active || Status == SubscriptionStatus.Paused;
} 