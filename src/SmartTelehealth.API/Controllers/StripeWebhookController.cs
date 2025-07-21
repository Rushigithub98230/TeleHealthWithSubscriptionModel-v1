using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using Stripe;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StripeWebhookController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IBillingService _billingService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeWebhookController> _logger;
    private readonly int _maxRetries;
    private readonly int _retryDelaySeconds;

    public StripeWebhookController(
        ISubscriptionService subscriptionService,
        IBillingService billingService,
        IConfiguration configuration,
        ILogger<StripeWebhookController> logger)
    {
        _subscriptionService = subscriptionService;
        _billingService = billingService;
        _configuration = configuration;
        _logger = logger;
        _maxRetries = configuration.GetValue<int>("Stripe:WebhookRetryAttempts", 3);
        _retryDelaySeconds = configuration.GetValue<int>("Stripe:WebhookRetryDelaySeconds", 5);
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var webhookSecret = _configuration["Stripe:WebhookSecret"];

        if (string.IsNullOrEmpty(webhookSecret) || webhookSecret == "whsec_test_webhook_secret_replace_in_production")
        {
            _logger.LogWarning("Webhook secret not properly configured");
            return BadRequest("Webhook secret not configured");
        }

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                webhookSecret
            );

            _logger.LogInformation("Received Stripe webhook event: {EventType} for {EventId}", 
                stripeEvent.Type, stripeEvent.Id);

            // Process webhook with retry logic
            await ProcessWebhookWithRetryAsync(stripeEvent);

            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error processing webhook: {Message}", ex.Message);
            return BadRequest(new { error = "Invalid webhook signature" });
        }
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
                _logger.LogWarning(ex, "Attempt {Attempt} failed for webhook event {EventType}", 
                    attempt, stripeEvent.Type);
                
                if (attempt == _maxRetries)
                {
                    _logger.LogError(ex, "All {MaxRetries} attempts failed for webhook event {EventType}", 
                        _maxRetries, stripeEvent.Type);
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
                _logger.LogInformation("Unhandled event type: {EventType}", stripeEvent.Type);
                break;
        }
    }

    private async Task HandleSubscriptionCreated(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;
        var existingSubscriptionResponse = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id);
        var existingSubscription = existingSubscriptionResponse.Data;
        if (existingSubscription != null)
        {
            var updateDto = new UpdateSubscriptionDto
            {
                Id = existingSubscription.Id
            };
            await _subscriptionService.UpdateSubscriptionAsync(existingSubscription.Id, updateDto);
            _logger.LogInformation("Updated subscription status for Stripe subscription: {StripeSubscriptionId}", subscription.Id);
        }
    }

    private async Task HandleSubscriptionUpdated(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;
        var existingSubscriptionResponse = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id);
        var existingSubscription = existingSubscriptionResponse.Data;
        if (existingSubscription != null)
        {
            var updateDto = new UpdateSubscriptionDto
            {
                Id = existingSubscription.Id
            };
            await _subscriptionService.UpdateSubscriptionAsync(existingSubscription.Id, updateDto);
            _logger.LogInformation("Updated subscription for Stripe subscription: {StripeSubscriptionId}", subscription.Id);
        }
    }

    private async Task HandleSubscriptionDeleted(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;
        var existingSubscriptionResponse = await _subscriptionService.GetByStripeSubscriptionIdAsync(subscription.Id);
        var existingSubscription = existingSubscriptionResponse.Data;
        if (existingSubscription != null)
        {
            await _subscriptionService.CancelSubscriptionAsync(existingSubscription.Id.ToString(), "Cancelled via Stripe webhook");
            _logger.LogInformation("Cancelled subscription for Stripe subscription: {StripeSubscriptionId}", subscription.Id);
        }
    }

    private async Task HandlePaymentSucceeded(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        try
        {
            // Log all properties of the invoice for debugging
            foreach (var prop in invoice.GetType().GetProperties())
            {
                _logger.LogInformation($"Invoice property: {prop.Name} = {prop.GetValue(invoice)}");
            }

            // Validate customer ID format before parsing
            if (!Guid.TryParse(invoice.CustomerId, out Guid userId))
            {
                _logger.LogWarning("Invalid customer ID format in invoice: {CustomerId}", invoice.CustomerId);
                return;
            }

            // Create billing record for successful payment
            await _billingService.CreateBillingRecordAsync(new CreateBillingRecordDto
            {
                UserId = userId.ToString(),
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
            });

            _logger.LogInformation("Created billing record for successful payment: {InvoiceId}", invoice.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling payment succeeded event for invoice {InvoiceId}", invoice.Id);
            throw;
        }
    }

    private async Task HandlePaymentFailed(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        try
        {
            // Validate customer ID format before parsing
            if (!Guid.TryParse(invoice.CustomerId, out Guid userId))
            {
                _logger.LogWarning("Invalid customer ID format in invoice: {CustomerId}", invoice.CustomerId);
                return;
            }

            // Create billing record for failed payment
            await _billingService.CreateBillingRecordAsync(new CreateBillingRecordDto
            {
                UserId = userId.ToString(),
                Amount = invoice.AmountDue / 100m, // Convert from cents
                Currency = invoice.Currency,
                PaymentMethod = "stripe",
                StripeInvoiceId = invoice.Id,
                Status = BillingRecord.BillingStatus.Failed.ToString(),
                Description = $"Failed payment for invoice {invoice.Number}",
                BillingDate = DateTime.UtcNow
            });

            _logger.LogInformation("Created billing record for failed payment: {InvoiceId}", invoice.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling payment failed event for invoice {InvoiceId}", invoice.Id);
            throw;
        }
    }

    private async Task HandlePaymentIntentSucceeded(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
        if (paymentIntent == null) return;

        try
        {
            // Handle successful payment intent
            _logger.LogInformation("Payment intent succeeded: {PaymentIntentId}", paymentIntent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling payment intent succeeded event for {PaymentIntentId}", paymentIntent.Id);
            throw;
        }
    }

    private async Task HandlePaymentIntentFailed(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
        if (paymentIntent == null) return;

        try
        {
            // Handle failed payment intent
            _logger.LogWarning("Payment intent failed: {PaymentIntentId}", paymentIntent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling payment intent failed event for {PaymentIntentId}", paymentIntent.Id);
            throw;
        }
    }

    private async Task HandleSubscriptionTrialWillEnd(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (subscription == null) return;

        try
        {
            _logger.LogInformation("Subscription trial will end: {StripeSubscriptionId}", subscription.Id);
            // TODO: Send notification to user about trial ending
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling subscription trial will end event for {StripeSubscriptionId}", subscription.Id);
            throw;
        }
    }

    private async Task HandlePaymentActionRequired(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        try
        {
            _logger.LogWarning("Payment action required for invoice: {InvoiceId}", invoice.Id);
            // TODO: Send notification to user about payment action required
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling payment action required event for invoice {InvoiceId}", invoice.Id);
            throw;
        }
    }
} 