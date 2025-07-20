using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs;

public class SubscriptionPlanDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid BillingCycleId { get; set; }
    public Guid CurrencyId { get; set; }
    public int TotalConsultations { get; set; }
    public int MaxConsultationsPerMonth { get; set; }
    public bool IncludesMedicationDelivery { get; set; }
    public bool IncludesHealthAssessments { get; set; }
    public bool IsActive { get; set; }
    public string? Features { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ConsultationLimit { get; set; }
    public bool IsPopular { get; set; }
} 