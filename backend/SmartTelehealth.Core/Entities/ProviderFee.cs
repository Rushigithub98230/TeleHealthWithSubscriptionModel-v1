using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class ProviderFee
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid ProviderId { get; set; }
    public User Provider { get; set; } = null!;
    
    [Required]
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    [Required]
    [Range(0, 10000)]
    public decimal ProposedFee { get; set; }
    
    [Required]
    [Range(0, 10000)]
    public decimal ApprovedFee { get; set; }
    
    public FeeStatus Status { get; set; } = FeeStatus.Pending;
    
    [MaxLength(1000)]
    public string? AdminRemarks { get; set; }
    
    [MaxLength(1000)]
    public string? ProviderNotes { get; set; }
    
    public DateTime? ProposedAt { get; set; }
    
    public DateTime? ReviewedAt { get; set; }
    
    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedByUser { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
}

public enum FeeStatus
{
    Pending = 0,
    UnderReview = 1,
    Approved = 2,
    Rejected = 3,
    RequiresMoreInfo = 4
} 