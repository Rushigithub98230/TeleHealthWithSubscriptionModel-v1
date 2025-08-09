using System.ComponentModel.DataAnnotations;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Application.DTOs;

public class CreateBillingRecordDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public string? SubscriptionId { get; set; }
    
    public string? ConsultationId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public DateTime? DueDate { get; set; }
    
    public string Type { get; set; } = BillingRecord.BillingType.Subscription.ToString();
    
    public string? StripeInvoiceId { get; set; }
    
    public string? StripePaymentIntentId { get; set; }
    
    public string? Currency { get; set; } = "usd";
    
    public decimal? TaxAmount { get; set; }
    
    public decimal? ShippingAmount { get; set; }
    
    public string? InvoiceNumber { get; set; }
    
    public Dictionary<string, string> Metadata { get; set; } = new();
}


