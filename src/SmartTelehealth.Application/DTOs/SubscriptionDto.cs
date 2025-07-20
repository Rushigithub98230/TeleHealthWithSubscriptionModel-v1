using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs
{
    public class SubscriptionDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string PlanDescription { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public Guid BillingCycleId { get; set; }
        public Guid CurrencyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime NextBillingDate { get; set; }
        public DateTime? LastBillingDate { get; set; }
        public string StripeSubscriptionId { get; set; } = string.Empty;
        public string StripeCustomerId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsPaused { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        
        // Additional properties referenced in services
        public DateTime? PausedAt { get; set; }
        public DateTime? ResumedAt { get; set; }
        // Removed: CategoryName, BillingFrequency, CurrentPrice
    }

    public class CreateSubscriptionDto
    {
        public string UserId { get; set; } = string.Empty;
        public string SubscriptionId { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public Guid BillingCycleId { get; set; }
        public Guid CurrencyId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? StartDate { get; set; }
        public bool StartImmediately { get; set; } = true;
        public string? PaymentMethodId { get; set; }
        public bool AutoRenew { get; set; } = true;
        // Removed: MonthlyPrice, QuarterlyPrice, AnnualPrice, BillingCycle
    }

    public class UpdateSubscriptionDto
    {
        public string Id { get; set; } = string.Empty;
        public string? PlanId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? NextBillingDate { get; set; }
        public int? UsedConsultations { get; set; }
        public DateTime? PausedAt { get; set; }
        public DateTime? ResumedAt { get; set; }
        public bool? IsPaused { get; set; }
        public string? CancellationReason { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class UpgradeSubscriptionDto
    {
        public string SubscriptionId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        [Required]
        public int NewPlanId { get; set; }
        [Required]
        public string PaymentMethodId { get; set; } = string.Empty;
        public bool Prorate { get; set; } = true;
    }

    public class BillingHistoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string SubscriptionId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime BillingDate { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? TransactionId { get; set; }
        public string? FailureReason { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string? StripeInvoiceId { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AddPaymentMethodDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;
        public string PaymentMethodId { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;
        public bool SetAsDefault { get; set; } = false;
    }

    public class SubscriptionBenefitDto
    {
        public string Id { get; set; } = string.Empty;
        public string SubscriptionId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string BenefitName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string BenefitType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int Limit { get; set; }
        public int UsedQuantity { get; set; }
        public int Used { get; set; }
        public int RemainingQuantity { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PauseSubscriptionDto
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
        public DateTime? ResumeDate { get; set; }
        public DateTime? PauseDate { get; set; }
    }

    public class SubscriptionReminderDto
    {
        public string Id { get; set; } = string.Empty;
        public string SubscriptionId { get; set; } = string.Empty;
        public string ReminderType { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
        public string? RecipientEmail { get; set; }
        public string? RecipientPhone { get; set; }
    }

    public class UpdateSubscriptionPlanDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public Guid BillingCycleId { get; set; }
        public Guid CurrencyId { get; set; }
        public bool IsActive { get; set; }
        // Removed: MonthlyPrice, QuarterlyPrice, AnnualPrice
    }
} 