using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class ProviderPayout
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid ProviderId { get; set; }
    public User Provider { get; set; } = null!;
    
    [Required]
    public Guid PayoutPeriodId { get; set; }
    public PayoutPeriod PayoutPeriod { get; set; } = null!;
    
    [Required]
    [Range(0, 1000000)]
    public decimal TotalEarnings { get; set; }
    
    [Required]
    [Range(0, 1000000)]
    public decimal PlatformCommission { get; set; }
    
    [Required]
    [Range(0, 1000000)]
    public decimal NetPayout { get; set; }
    
    public int TotalConsultations { get; set; }
    
    public int TotalOneTimeConsultations { get; set; }
    
    public int TotalSubscriptionConsultations { get; set; }
    
    public PayoutStatus Status { get; set; } = PayoutStatus.Pending;
    
    [MaxLength(1000)]
    public string? AdminRemarks { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    
    public Guid? ProcessedByUserId { get; set; }
    public User? ProcessedByUser { get; set; }
    
    [MaxLength(255)]
    public string? TransactionId { get; set; }
    
    [MaxLength(255)]
    public string? PaymentMethodId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties for detailed breakdown
    public ICollection<PayoutDetail> PayoutDetails { get; set; } = new List<PayoutDetail>();
}

public class PayoutPeriod
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public PayoutPeriodStatus Status { get; set; } = PayoutPeriodStatus.Open;
    
    public DateTime? ProcessedAt { get; set; }
    
    public Guid? ProcessedByUserId { get; set; }
    public User? ProcessedByUser { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<ProviderPayout> ProviderPayouts { get; set; } = new List<ProviderPayout>();
}

public class PayoutDetail
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid PayoutId { get; set; }
    public ProviderPayout Payout { get; set; } = null!;
    
    [Required]
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    
    [Required]
    [Range(0, 10000)]
    public decimal ConsultationFee { get; set; }
    
    [Required]
    [Range(0, 10000)]
    public decimal PlatformCommission { get; set; }
    
    [Required]
    [Range(0, 10000)]
    public decimal ProviderEarnings { get; set; }
    
    public ConsultationType ConsultationType { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum PayoutStatus
{
    Pending = 0,
    UnderReview = 1,
    Approved = 2,
    Processed = 3,
    OnHold = 4,
    Cancelled = 5
}

public enum PayoutPeriodStatus
{
    Open = 0,
    Processing = 1,
    Completed = 2,
    Cancelled = 3
}

public enum ConsultationType
{
    OneTime = 0,
    Subscription = 1
} 