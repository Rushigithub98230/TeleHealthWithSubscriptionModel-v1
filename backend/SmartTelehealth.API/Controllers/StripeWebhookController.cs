using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using Stripe;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Controllers
{
[ApiController]
[Route("api/[controller]")]
public class StripeWebhookController : BaseController
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IBillingService _billingService;
    private readonly IConfiguration _configuration;
    private readonly int _maxRetries;
    private readonly int _retryDelaySeconds;

    public StripeWebhookController(
        ISubscriptionService subscriptionService,
        IBillingService billingService,
        IConfiguration configuration)
    {
        _subscriptionService = subscriptionService;
        _billingService = billingService;
        _configuration = configuration;
        _maxRetries = configuration.GetValue<int>("Stripe:WebhookRetryAttempts", 3);
        _retryDelaySeconds = configuration.GetValue<int>("Stripe:WebhookRetryDelaySeconds", 5);
    }

    [HttpPost]
    public async Task<JsonModel> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var webhookSecret = _configuration["Stripe:WebhookSecret"];

        if (string.IsNullOrEmpty(webhookSecret) || webhookSecret == "whsec_test_webhook_secret_replace_in_production")
        {
            return new JsonModel { data = new object(), Message = "Webhook secret not configured", StatusCode = 400 };
        }

        var stripeEvent = EventUtility.ConstructEvent(
            json,
            Request.Headers["Stripe-Signature"],
            webhookSecret
        );

        // Process webhook with retry logic
        await ProcessWebhookWithRetryAsync(stripeEvent);

        return new JsonModel { data = new object(), Message = "Webhook processed successfully", StatusCode = 200 };
    }

    private async Task ProcessWebhookWithRetryAsync(Event stripeEvent)
    {
        for (int attempt = 1; attempt <= _maxRetries; attempt++)
        {
            try
            {
                await ProcessWebhookEventAsync(stripeEvent);
                return; // Success, exit retry loop
            }
            catch (Exception ex)
            {
                if (attempt == _maxRetries)
                {
                    throw; // Re-throw after all retries exhausted
                }
                
                await Task.Delay(TimeSpan.FromSeconds(_retryDelaySeconds * attempt));
            }
        }
    }

    private async Task ProcessWebhookEventAsync(Event stripeEvent)
    {
        switch (stripeEvent.Type)
        {
            case "customer.subscription.created":
                await HandleSubscriptionCreated(stripeEvent);
                break;
            case "customer.subscription.updated":
                await HandleSubscriptionUpdated(stripeEvent);
                break;
            case "customer.subscription.deleted":
                await HandleSubscriptionDeleted(stripeEvent);
                break;
            case "invoice.payment_succeeded":
                await HandlePaymentSucceeded(stripeEvent);
                break;
            case "invoice.payment_failed":
                await HandlePaymentFailed(stripeEvent);
                break;
            case "payment_intent.succeeded":
                await HandlePaymentIntentSucceeded(stripeEvent);
                break;
            case "payment_intent.payment_failed":
                await HandlePaymentIntentFailed(stripeEvent);
                break;
            case "customer.subscription.trial_will_end":
                await HandleSubscriptionTrialWillEnd(stripeEvent);
                break;
            case "invoice.payment_action_required":
                await HandlePaymentActionRequired(stripeEvent);
                break;
            default:
                // Unhandled event type
                break;
        }
    }

    private async Task HandleSubscriptionCreated(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;
        var adminToken = new TokenModel { UserID = 1, RoleID = 1 }; // Admin token for webhook operations
        var existingSubscriptionResponse = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, adminToken);
        var existingSubscription = existingSubscriptionResponse.data;
        if (existingSubscription != null)
        {
            // Cast the object to the correct type to access properties
            if (existingSubscription is SubscriptionDto subscriptionDto)
            {
                var updateDto = new UpdateSubscriptionDto
                {
                    Id = subscriptionDto.Id
                };
                await _subscriptionService.UpdateSubscriptionAsync(subscriptionDto.Id.ToString(), updateDto, adminToken);
            }
        }
    }

    private async Task HandleSubscriptionUpdated(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;
        var adminToken = new TokenModel { UserID = 1, RoleID = 1 }; // Admin token for webhook operations
        var existingSubscriptionResponse = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, adminToken);
        var existingSubscription = existingSubscriptionResponse.data;
        if (existingSubscription != null)
        {
            // Cast the object to the correct type to access properties
            if (existingSubscription is SubscriptionDto subscriptionDto)
            {
                var updateDto = new UpdateSubscriptionDto
                {
                    Id = subscriptionDto.Id
                };
                await _subscriptionService.UpdateSubscriptionAsync(subscriptionDto.Id.ToString(), updateDto, adminToken);
            }
        }
    }

    private async Task HandleSubscriptionDeleted(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;
        var adminToken = new TokenModel { UserID = 1, RoleID = 1 }; // Admin token for webhook operations
        var existingSubscriptionResponse = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id, adminToken);
        var existingSubscription = existingSubscriptionResponse.data;
        if (existingSubscription != null)
        {
            // Cast the object to the correct type to access properties
            if (existingSubscription is SubscriptionDto subscriptionDto)
            {
                await _subscriptionService.CancelSubscriptionAsync(subscriptionDto.Id.ToString(), "Cancelled via Stripe webhook", adminToken);
            }
        }
    }

    private async Task HandlePaymentSucceeded(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        // Validate customer ID format before parsing
        if (!int.TryParse(invoice.CustomerId, out int userId))
        {
            return;
        }

        // Create billing record for successful payment
        await _billingService.CreateBillingRecordAsync(new CreateBillingRecordDto
        {
            UserId = userId,
            Amount = invoice.AmountPaid / 100m, // Convert from cents
            Currency = invoice.Currency,
            PaymentMethod = "stripe",
            StripeInvoiceId = invoice.Id,
            // TODO: Restore when correct property names are known
            // StripePaymentIntentId = invoice.PaymentIntentId ?? invoice.PaymentIntent?.ToString() ?? string.Empty,
            Status = BillingRecord.BillingStatus.Paid.ToString(),
            Description = $"Stripe Invoice Payment: {invoice.Id}",
            BillingDate = DateTime.UtcNow,
            ConsultationId = null,
            // SubscriptionId = invoice.SubscriptionId ?? invoice.Subscription?.ToString() ?? string.Empty
        }, GetToken(HttpContext));
    }

    private async Task HandlePaymentFailed(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        // Validate customer ID format before parsing
        if (!int.TryParse(invoice.CustomerId, out int userId))
        {
            return;
        }

        // Create billing record for failed payment
        await _billingService.CreateBillingRecordAsync(new CreateBillingRecordDto
        {
            UserId = userId,
            Amount = invoice.AmountDue / 100m, // Convert from cents
            Currency = invoice.Currency,
            PaymentMethod = "stripe",
            StripeInvoiceId = invoice.Id,
            Status = BillingRecord.BillingStatus.Failed.ToString(),
            Description = $"Failed payment for invoice {invoice.Number}",
            BillingDate = DateTime.UtcNow
        }, GetToken(HttpContext));
    }

    private async Task HandlePaymentIntentSucceeded(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
        if (paymentIntent == null) return;

        // Handle successful payment intent
    }

    private async Task HandlePaymentIntentFailed(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
        if (paymentIntent == null) return;

        // Handle failed payment intent
    }

    private async Task HandleSubscriptionTrialWillEnd(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;

        // TODO: Send notification to user about trial ending
    }

    private async Task HandlePaymentActionRequired(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        // TODO: Send notification to user about payment action required
    }
}
} 