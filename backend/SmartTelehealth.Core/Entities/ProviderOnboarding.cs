using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class ProviderOnboarding
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Specialty { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? SubSpecialty { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string LicenseNumber { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LicenseState { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? NPINumber { get; set; }
    
    [MaxLength(50)]
    public string? DEANumber { get; set; }
    
    [MaxLength(500)]
    public string? Education { get; set; }
    
    [MaxLength(1000)]
    public string? WorkHistory { get; set; }
    
    [MaxLength(500)]
    public string? MalpracticeInsurance { get; set; }
    
    [MaxLength(1000)]
    public string? Bio { get; set; }
    
    [MaxLength(255)]
    public string? ProfilePhotoUrl { get; set; }
    
    [MaxLength(255)]
    public string? GovernmentIdUrl { get; set; }
    
    [MaxLength(255)]
    public string? LicenseDocumentUrl { get; set; }
    
    [MaxLength(255)]
    public string? CertificationDocumentUrl { get; set; }
    
    [MaxLength(255)]
    public string? MalpracticeInsuranceUrl { get; set; }
    
    public OnboardingStatus Status { get; set; } = OnboardingStatus.Pending;
    
    [MaxLength(1000)]
    public string? AdminRemarks { get; set; }
    
    public DateTime? SubmittedAt { get; set; }
    
    public DateTime? ReviewedAt { get; set; }
    
    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedByUser { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
}

public enum OnboardingStatus
{
    Pending = 0,
    UnderReview = 1,
    Approved = 2,
    Rejected = 3,
    RequiresMoreInfo = 4
} 