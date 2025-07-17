using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

public class CreateSubscriptionPlanDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public decimal MonthlyPrice { get; set; }
    public decimal QuarterlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }
    public int MessagingCount { get; set; } = 10;
    public bool IncludesMedicationDelivery { get; set; } = true;
    public bool IncludesFollowUpCare { get; set; } = true;
    public int DeliveryFrequencyDays { get; set; } = 30;
    public int MaxPauseDurationDays { get; set; } = 90;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    [MaxLength(1000)]
    public string? Features { get; set; }
    [Required]
    public Guid CategoryId { get; set; }
} 