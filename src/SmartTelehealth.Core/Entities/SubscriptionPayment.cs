using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class SubscriptionPayment : BaseEntity
{
    public Guid SubscriptionId { get; set; }
    public virtual Subscription Subscription { get; set; } = null!;
    
    public decimal Amount { get; set; }
    public Guid CurrencyId { get; set; }
    public virtual MasterCurrency Currency { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? FailureReason { get; set; }
    
    public string? PaymentIntentId { get; set; }
    public string? InvoiceId { get; set; }
    public string? ReceiptUrl { get; set; }
    
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    
    public string? StripePaymentIntentId { get; set; }
    public string? StripeInvoiceId { get; set; }
    
    public enum PaymentStatus
    {
        Pending,
        Processing,
        Succeeded,
        Failed,
        Cancelled,
        Refunded
    }
} 