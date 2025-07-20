using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class Subscription : BaseEntity
{
    // Remove SubscriptionBillingFrequency enum and BillingFrequency property
    public Guid BillingCycleId { get; set; }
    public virtual MasterBillingCycle BillingCycle { get; set; } = null!;
    
    // Foreign keys
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    public Guid SubscriptionPlanId { get; set; }
    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    
    public Guid? ProviderId { get; set; }
    public virtual Provider? Provider { get; set; }
    
    // Subscription details
    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // e.g., 'Active', 'Paused', 'Cancelled', etc.
    [MaxLength(200)]
    public string? StatusReason { get; set; }
    
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
    
    // Remove SubscriptionStatus enum and all enum-based logic
    // Update IsActive, IsPaused, IsCancelled, CanPause, CanResume, CanCancel to use string Status
    public bool IsActive => Status == "Active";
    public bool IsPaused => Status == "Paused";
    public bool IsCancelled => Status == "Cancelled";
    public bool CanPause => Status == "Active" && (PausedDate == null);
    public bool CanResume => Status == "Paused";
    public bool CanCancel => Status == "Active" || Status == "Paused";
} 