using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class CategoryFeeRange
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    [Required]
    [Range(0, 10000)]
    public decimal MinimumFee { get; set; }
    
    [Required]
    [Range(0, 10000)]
    public decimal MaximumFee { get; set; }
    
    [Required]
    [Range(0, 100)]
    public decimal PlatformCommission { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
} 