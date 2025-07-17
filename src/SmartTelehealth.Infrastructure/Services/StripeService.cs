using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Stripe;

namespace SmartTelehealth.Infrastructure.Services;

public class StripeService : IStripeService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeService> _logger;
    private readonly int _maxRetries = 3;
    private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(1);
    
    public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        var secretKey = _configuration["Stripe:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("Stripe secret key is not configured");
        }
        
        StripeConfiguration.ApiKey = secretKey;
        _logger.LogInformation("Stripe service initialized successfully");
    }
    
    // Customer Management
    public async Task<string> CreateCustomerAsync(string email, string name)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException("Email is required", nameof(email));
        
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name is required", nameof(name));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var customerCreateOptions = new CustomerCreateOptions
                {
                    Email = email,
                    Name = name,
                    Metadata = new Dictionary<string, string>
                    {
                        { "created_at", DateTime.UtcNow.ToString("O") },
                        { "source", "smart_telehealth" }
                    }
                };

                var customerService = new CustomerService();
                var customer = await customerService.CreateAsync(customerCreateOptions);

                _logger.LogInformation("Created Stripe customer: {CustomerId} for {Email}", customer.Id, email);
                return customer.Id;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating customer for {Email}: {Message}", email, ex.Message);
                throw new InvalidOperationException($"Failed to create Stripe customer: {ex.Message}", ex);
            }
        });
    }

    public async Task<CustomerDto> GetCustomerAsync(string customerId)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentException("Customer ID is required", nameof(customerId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var customerService = new CustomerService();
                var customer = await customerService.GetAsync(customerId);

                return new CustomerDto
                {
                    Id = customer.Id,
                    Email = customer.Email,
                    Name = customer.Name,
                    CreatedAt = customer.Created,
                    IsActive = customer.Deleted == null
                };
            }
            catch (StripeException ex) when (ex.StripeError?.Type == "invalid_request_error")
            {
                _logger.LogWarning("Customer not found: {CustomerId}", customerId);
                throw new ArgumentException($"Customer not found: {customerId}");
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error getting customer {CustomerId}: {Message}", customerId, ex.Message);
                throw new InvalidOperationException($"Failed to get Stripe customer: {ex.Message}", ex);
            }
        });
    }
    
    // Payment Methods
    public async Task<string> CreatePaymentMethodAsync(string customerId, string paymentMethodId)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentException("Customer ID is required", nameof(customerId));
        
        if (string.IsNullOrEmpty(paymentMethodId))
            throw new ArgumentException("Payment method ID is required", nameof(paymentMethodId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var paymentMethodAttachOptions = new PaymentMethodAttachOptions
                {
                    Customer = customerId
                };

                var paymentMethodService = new PaymentMethodService();
                var paymentMethod = await paymentMethodService.AttachAsync(paymentMethodId, paymentMethodAttachOptions);

                _logger.LogInformation("Attached payment method {PaymentMethodId} to customer {CustomerId}", paymentMethodId, customerId);
                return paymentMethod.Id;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error attaching payment method {PaymentMethodId} to customer {CustomerId}: {Message}", 
                    paymentMethodId, customerId, ex.Message);
                throw new InvalidOperationException($"Failed to attach payment method: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> UpdatePaymentMethodAsync(string customerId, string paymentMethodId)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentException("Customer ID is required", nameof(customerId));
        
        if (string.IsNullOrEmpty(paymentMethodId))
            throw new ArgumentException("Payment method ID is required", nameof(paymentMethodId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var paymentMethodUpdateOptions = new PaymentMethodUpdateOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { "updated_at", DateTime.UtcNow.ToString("O") }
                    }
                };

                var paymentMethodService = new PaymentMethodService();
                await paymentMethodService.UpdateAsync(paymentMethodId, paymentMethodUpdateOptions);

                _logger.LogInformation("Updated payment method {PaymentMethodId} for customer {CustomerId}", paymentMethodId, customerId);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error updating payment method {PaymentMethodId} for customer {CustomerId}: {Message}", 
                    paymentMethodId, customerId, ex.Message);
                throw new InvalidOperationException($"Failed to update payment method: {ex.Message}", ex);
            }
        });
    }
    
    // Product Management
    public async Task<string> CreateProductAsync(string name, string description)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Product name is required", nameof(name));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var productCreateOptions = new ProductCreateOptions
                {
                    Name = name,
                    Description = description ?? "",
                    Metadata = new Dictionary<string, string>
                    {
                        { "created_at", DateTime.UtcNow.ToString("O") },
                        { "source", "smart_telehealth" }
                    }
                };

                var productService = new ProductService();
                var product = await productService.CreateAsync(productCreateOptions);

                _logger.LogInformation("Created Stripe product: {ProductId} - {Name}", product.Id, name);
                return product.Id;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating product {Name}: {Message}", name, ex.Message);
                throw new InvalidOperationException($"Failed to create Stripe product: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> UpdateProductAsync(string productId, string name, string description)
    {
        if (string.IsNullOrEmpty(productId))
            throw new ArgumentException("Product ID is required", nameof(productId));
        
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Product name is required", nameof(name));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var productUpdateOptions = new ProductUpdateOptions
                {
                    Name = name,
                    Description = description ?? "",
                    Metadata = new Dictionary<string, string>
                    {
                        { "updated_at", DateTime.UtcNow.ToString("O") }
                    }
                };

                var productService = new ProductService();
                await productService.UpdateAsync(productId, productUpdateOptions);

                _logger.LogInformation("Updated Stripe product: {ProductId}", productId);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error updating product {ProductId}: {Message}", productId, ex.Message);
                throw new InvalidOperationException($"Failed to update Stripe product: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> DeleteProductAsync(string productId)
    {
        if (string.IsNullOrEmpty(productId))
            throw new ArgumentException("Product ID is required", nameof(productId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var productService = new ProductService();
                await productService.DeleteAsync(productId);

                _logger.LogInformation("Deleted Stripe product: {ProductId}", productId);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error deleting product {ProductId}: {Message}", productId, ex.Message);
                throw new InvalidOperationException($"Failed to delete Stripe product: {ex.Message}", ex);
            }
        });
    }
    
    // Price Management
    public async Task<string> CreatePriceAsync(string productId, decimal amount, string currency, string interval, int intervalCount)
    {
        if (string.IsNullOrEmpty(productId))
            throw new ArgumentException("Product ID is required", nameof(productId));
        
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));
        
        if (string.IsNullOrEmpty(currency))
            throw new ArgumentException("Currency is required", nameof(currency));
        
        if (string.IsNullOrEmpty(interval))
            throw new ArgumentException("Interval is required", nameof(interval));
        
        if (intervalCount <= 0)
            throw new ArgumentException("Interval count must be greater than zero", nameof(intervalCount));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var priceCreateOptions = new PriceCreateOptions
                {
                    Product = productId,
                    UnitAmount = (long)(amount * 100), // Convert to cents
                    Currency = currency.ToLower(),
                    Recurring = new PriceRecurringOptions
                    {
                        Interval = interval.ToLower(),
                        IntervalCount = intervalCount
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "created_at", DateTime.UtcNow.ToString("O") },
                        { "source", "smart_telehealth" }
                    }
                };

                var priceService = new PriceService();
                var price = await priceService.CreateAsync(priceCreateOptions);

                _logger.LogInformation("Created Stripe price: {PriceId} for product {ProductId} - {Amount} {Currency} every {IntervalCount} {Interval}", 
                    price.Id, productId, amount, currency, intervalCount, interval);
                return price.Id;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating price for product {ProductId}: {Message}", productId, ex.Message);
                throw new InvalidOperationException($"Failed to create Stripe price: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> UpdatePriceAsync(string priceId, decimal amount)
    {
        if (string.IsNullOrEmpty(priceId))
            throw new ArgumentException("Price ID is required", nameof(priceId));
        
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                // Note: Stripe doesn't allow updating UnitAmount on existing prices
                // We need to create a new price instead
                _logger.LogWarning("Cannot update UnitAmount on existing Stripe price {PriceId}. Create a new price instead.", priceId);
                return false;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error updating price {PriceId}: {Message}", priceId, ex.Message);
                throw new InvalidOperationException($"Failed to update Stripe price: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> DeactivatePriceAsync(string priceId)
    {
        if (string.IsNullOrEmpty(priceId))
            throw new ArgumentException("Price ID is required", nameof(priceId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var priceUpdateOptions = new PriceUpdateOptions
                {
                    Active = false
                };

                var priceService = new PriceService();
                await priceService.UpdateAsync(priceId, priceUpdateOptions);

                _logger.LogInformation("Deactivated Stripe price: {PriceId}", priceId);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error deactivating price {PriceId}: {Message}", priceId, ex.Message);
                throw new InvalidOperationException($"Failed to deactivate Stripe price: {ex.Message}", ex);
            }
        });
    }
    
    // Subscription Management
    public async Task<string> CreateSubscriptionAsync(string customerId, string priceId, string paymentMethodId)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentException("Customer ID is required", nameof(customerId));
        
        if (string.IsNullOrEmpty(priceId))
            throw new ArgumentException("Price ID is required", nameof(priceId));
        
        if (string.IsNullOrEmpty(paymentMethodId))
            throw new ArgumentException("Payment method ID is required", nameof(paymentMethodId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var subscriptionCreateOptions = new SubscriptionCreateOptions
                {
                    Customer = customerId,
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions
                        {
                            Price = priceId
                        }
                    },
                    DefaultPaymentMethod = paymentMethodId,
                    PaymentSettings = new SubscriptionPaymentSettingsOptions
                    {
                        PaymentMethodTypes = new List<string> { "card" },
                        SaveDefaultPaymentMethod = "on_subscription"
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "created_at", DateTime.UtcNow.ToString("O") },
                        { "source", "smart_telehealth" }
                    }
                };

                var subscriptionService = new SubscriptionService();
                var subscription = await subscriptionService.CreateAsync(subscriptionCreateOptions);

                _logger.LogInformation("Created Stripe subscription: {SubscriptionId} for customer {CustomerId}", subscription.Id, customerId);
                return subscription.Id;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating subscription for customer {CustomerId}: {Message}", customerId, ex.Message);
                throw new InvalidOperationException($"Failed to create Stripe subscription: {ex.Message}", ex);
            }
        });
    }

    public async Task<SubscriptionDto> GetSubscriptionAsync(string subscriptionId)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID is required", nameof(subscriptionId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var subscriptionService = new SubscriptionService();
                var subscription = await subscriptionService.GetAsync(subscriptionId);

                return new SubscriptionDto
                {
                    Id = Guid.NewGuid().ToString(), // Convert Guid to string
                    StripeSubscriptionId = subscription.Id,
                    Status = MapStripeStatusToEnum(subscription.Status).ToString(), // Convert enum to string
                    CreatedAt = subscription.Created
                };
            }
            catch (StripeException ex) when (ex.StripeError?.Type == "invalid_request_error")
            {
                _logger.LogWarning("Subscription not found: {SubscriptionId}", subscriptionId);
                throw new ArgumentException($"Subscription not found: {subscriptionId}");
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error getting subscription {SubscriptionId}: {Message}", subscriptionId, ex.Message);
                throw new InvalidOperationException($"Failed to get Stripe subscription: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> CancelSubscriptionAsync(string subscriptionId)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID is required", nameof(subscriptionId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var subscriptionService = new SubscriptionService();
                await subscriptionService.CancelAsync(subscriptionId);

                _logger.LogInformation("Cancelled Stripe subscription: {SubscriptionId}", subscriptionId);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error cancelling subscription {SubscriptionId}: {Message}", subscriptionId, ex.Message);
                throw new InvalidOperationException($"Failed to cancel Stripe subscription: {ex.Message}", ex);
            }
        });
    }
    
    // Payment Processing
    public async Task<PaymentResultDto> ProcessPaymentAsync(string paymentMethodId, decimal amount, string currency)
    {
        if (string.IsNullOrEmpty(paymentMethodId))
            throw new ArgumentException("Payment method ID is required", nameof(paymentMethodId));
        
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));
        
        if (string.IsNullOrEmpty(currency))
            throw new ArgumentException("Currency is required", nameof(currency));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var paymentIntentCreateOptions = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100), // Convert to cents
                    Currency = currency.ToLower(),
                    PaymentMethod = paymentMethodId,
                    Confirm = true,
                    ReturnUrl = "https://smarttelehealth.com/payment/success",
                    Metadata = new Dictionary<string, string>
                    {
                        { "created_at", DateTime.UtcNow.ToString("O") },
                        { "source", "smart_telehealth" }
                    }
                };

                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = await paymentIntentService.CreateAsync(paymentIntentCreateOptions);

                _logger.LogInformation("Processed payment: {PaymentIntentId} - {Amount} {Currency}", 
                    paymentIntent.Id, amount, currency);
                return new PaymentResultDto
                {
                    PaymentIntentId = paymentIntent.Id,
                    CustomerId = paymentIntent.CustomerId,
                    Amount = amount,
                    Currency = currency,
                    Status = paymentIntent.Status,
                    ErrorMessage = null
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error processing payment: {Message}", ex.Message);
                return new PaymentResultDto
                {
                    PaymentIntentId = string.Empty,
                    CustomerId = string.Empty,
                    Amount = amount,
                    Currency = currency,
                    Status = "failed",
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "General error processing payment: {Message}", ex.Message);
                return new PaymentResultDto
                {
                    PaymentIntentId = string.Empty,
                    CustomerId = string.Empty,
                    Amount = amount,
                    Currency = currency,
                    Status = "failed",
                    ErrorMessage = ex.Message
                };
            }
        });
    }

    public async Task<bool> ProcessRefundAsync(string paymentIntentId, decimal amount)
    {
        if (string.IsNullOrEmpty(paymentIntentId))
            throw new ArgumentException("Payment intent ID is required", nameof(paymentIntentId));
        
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var refundCreateOptions = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId,
                    Amount = (long)(amount * 100), // Convert to cents
                    Metadata = new Dictionary<string, string>
                    {
                        { "created_at", DateTime.UtcNow.ToString("O") },
                        { "source", "smart_telehealth" }
                    }
                };

                var refundService = new RefundService();
                await refundService.CreateAsync(refundCreateOptions);

                _logger.LogInformation("Processed refund for payment intent: {PaymentIntentId} - {Amount}", 
                    paymentIntentId, amount);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error processing refund for payment intent {PaymentIntentId}: {Message}", 
                    paymentIntentId, ex.Message);
                throw new InvalidOperationException($"Failed to process refund: {ex.Message}", ex);
            }
        });
    }

    public Task<string> AddPaymentMethodAsync(string customerId, string paymentMethodId) => throw new NotImplementedException();
    public Task<bool> SetDefaultPaymentMethodAsync(string customerId, string paymentMethodId) => throw new NotImplementedException();
    public Task<bool> RemovePaymentMethodAsync(string customerId, string paymentMethodId) => throw new NotImplementedException();
    public Task<IEnumerable<PaymentMethodDto>> GetCustomerPaymentMethodsAsync(string customerId) => throw new NotImplementedException();
    public Task<bool> UpdateSubscriptionAsync(string subscriptionId, string priceId) => throw new NotImplementedException();
    public Task<bool> PauseSubscriptionAsync(string subscriptionId) => throw new NotImplementedException();
    public Task<bool> ResumeSubscriptionAsync(string subscriptionId) => throw new NotImplementedException();
    public Task<bool> ReactivateSubscriptionAsync(string subscriptionId) => throw new NotImplementedException();
    public Task<bool> UpdateSubscriptionPaymentMethodAsync(string subscriptionId, string paymentMethodId) => throw new NotImplementedException();

    // Helper Methods
    private SmartTelehealth.Core.Entities.Subscription.SubscriptionStatus MapStripeStatusToEnum(string status)
    {
        return status switch
        {
            "active" => SmartTelehealth.Core.Entities.Subscription.SubscriptionStatus.Active,
            "canceled" => SmartTelehealth.Core.Entities.Subscription.SubscriptionStatus.Cancelled,
            "incomplete" => SmartTelehealth.Core.Entities.Subscription.SubscriptionStatus.Pending,
            "incomplete_expired" => SmartTelehealth.Core.Entities.Subscription.SubscriptionStatus.Cancelled,
            "past_due" => SmartTelehealth.Core.Entities.Subscription.SubscriptionStatus.PastDue,
            "trialing" => SmartTelehealth.Core.Entities.Subscription.SubscriptionStatus.Active,
            "unpaid" => SmartTelehealth.Core.Entities.Subscription.SubscriptionStatus.Pending,
            _ => SmartTelehealth.Core.Entities.Subscription.SubscriptionStatus.Pending
        };
    }

    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
    {
        var lastException = (Exception?)null;
        
        for (int attempt = 1; attempt <= _maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (StripeException ex) when (ex.StripeError?.Type == "rate_limit_error" && attempt < _maxRetries)
            {
                lastException = ex;
                _logger.LogWarning("Rate limit hit, retrying in {Delay}ms (attempt {Attempt}/{MaxRetries})", 
                    _retryDelay.TotalMilliseconds, attempt, _maxRetries);
                await Task.Delay(_retryDelay);
            }
            catch (StripeException ex) when (ex.StripeError?.Type == "api_connection_error" && attempt < _maxRetries)
            {
                lastException = ex;
                _logger.LogWarning("API connection error, retrying in {Delay}ms (attempt {Attempt}/{MaxRetries})", 
                    _retryDelay.TotalMilliseconds, attempt, _maxRetries);
                await Task.Delay(_retryDelay);
            }
            catch (Exception ex)
            {
                // Don't retry for other exceptions
                throw;
            }
        }
        
        throw lastException ?? new InvalidOperationException("Operation failed after all retry attempts");
    }
} 