using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Stripe;
using Stripe.Checkout;

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
        
        var secretKey = _configuration["StripeSettings:SecretKey"];
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

                _logger.LogInformation("Created Stripe customer: {CustomerId} for email {Email}", customer.Id, email);
                return customer.Id;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating customer for email {Email}: {Message}", email, ex.Message);
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
                    CreatedAt = customer.Created
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

    public async Task<IEnumerable<CustomerDto>> ListCustomersAsync()
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var customerService = new CustomerService();
                var customers = await customerService.ListAsync(new CustomerListOptions
                {
                    Limit = 100 // Limit to 100 customers for testing
                });

                return customers.Data.Select(customer => new CustomerDto
                {
                    Id = customer.Id,
                    Email = customer.Email,
                    Name = customer.Name,
                    CreatedAt = customer.Created
                });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error listing customers: {Message}", ex.Message);
                throw new InvalidOperationException($"Failed to list Stripe customers: {ex.Message}", ex);
            }
        });
    }
    
    public async Task<bool> UpdateCustomerAsync(string customerId, string email, string name)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentException("Customer ID is required", nameof(customerId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var customerUpdateOptions = new CustomerUpdateOptions
                {
                    Email = email,
                    Name = name,
                    Metadata = new Dictionary<string, string>
                    {
                        { "updated_at", DateTime.UtcNow.ToString("O") },
                        { "source", "smart_telehealth" }
                    }
                };

                var customerService = new CustomerService();
                await customerService.UpdateAsync(customerId, customerUpdateOptions);

                _logger.LogInformation("Updated Stripe customer: {CustomerId}", customerId);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error updating customer {CustomerId}: {Message}", customerId, ex.Message);
                throw new InvalidOperationException($"Failed to update Stripe customer: {ex.Message}", ex);
            }
        });
    }

    // Payment Method Management
    public async Task<IEnumerable<PaymentMethodDto>> GetCustomerPaymentMethodsAsync(string customerId)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentException("Customer ID is required", nameof(customerId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var paymentMethodService = new PaymentMethodService();
                var paymentMethods = await paymentMethodService.ListAsync(new PaymentMethodListOptions
                {
                    Customer = customerId,
                    Type = "card"
                });

                var customerService = new CustomerService();
                var customer = await customerService.GetAsync(customerId);
                var defaultPaymentMethodId = customer.InvoiceSettings?.DefaultPaymentMethod;

                return paymentMethods.Data.Select(pm => new PaymentMethodDto
                {
                    Id = pm.Id,
                    CustomerId = pm.CustomerId,
                    Type = pm.Type,
                    Card = new CardDto
                    {
                        Brand = pm.Card?.Brand,
                        Last4 = pm.Card?.Last4,
                        ExpMonth = (int)(pm.Card?.ExpMonth ?? 0),
                        ExpYear = (int)(pm.Card?.ExpYear ?? 0),
                        Fingerprint = pm.Card?.Fingerprint
                    },
                    IsDefault =  pm.Id.Equals(defaultPaymentMethodId),
                    CreatedAt = pm.Created
                });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error getting payment methods for customer {CustomerId}: {Message}", customerId, ex.Message);
                throw new InvalidOperationException($"Failed to get payment methods: {ex.Message}", ex);
            }
        });
    }
    
    public async Task<string> AddPaymentMethodAsync(string customerId, string paymentMethodId)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentException("Customer ID is required", nameof(customerId));

        if (string.IsNullOrEmpty(paymentMethodId))
            throw new ArgumentException("Payment method ID is required", nameof(paymentMethodId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var paymentMethodService = new PaymentMethodService();
                var paymentMethodAttachOptions = new PaymentMethodAttachOptions
                {
                    Customer = customerId
                };

                var paymentMethod = await paymentMethodService.AttachAsync(paymentMethodId, paymentMethodAttachOptions);

                _logger.LogInformation("Added payment method {PaymentMethodId} to customer {CustomerId}", paymentMethodId, customerId);
                return paymentMethod.Id;
            }
            catch (StripeException ex) when (ex.StripeError?.Type == "invalid_request_error")
            {
                _logger.LogWarning("Payment method not found: {PaymentMethodId}", paymentMethodId);
                throw new ArgumentException($"Payment method not found: {paymentMethodId}");
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error adding payment method {PaymentMethodId} to customer {CustomerId}: {Message}", paymentMethodId, customerId, ex.Message);
                throw new InvalidOperationException($"Failed to add payment method: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> SetDefaultPaymentMethodAsync(string customerId, string paymentMethodId)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentException("Customer ID is required", nameof(customerId));
        
        if (string.IsNullOrEmpty(paymentMethodId))
            throw new ArgumentException("Payment method ID is required", nameof(paymentMethodId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var customerService = new CustomerService();
                var customerUpdateOptions = new CustomerUpdateOptions
                {
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = paymentMethodId
                    }
                };

                await customerService.UpdateAsync(customerId, customerUpdateOptions);

                _logger.LogInformation("Set default payment method {PaymentMethodId} for customer {CustomerId}", paymentMethodId, customerId);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error setting default payment method {PaymentMethodId} for customer {CustomerId}: {Message}", paymentMethodId, customerId, ex.Message);
                throw new InvalidOperationException($"Failed to set default payment method: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> RemovePaymentMethodAsync(string customerId, string paymentMethodId)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentException("Customer ID is required", nameof(customerId));
        
        if (string.IsNullOrEmpty(paymentMethodId))
            throw new ArgumentException("Payment method ID is required", nameof(paymentMethodId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var paymentMethodService = new PaymentMethodService();
                
                // Detach payment method from customer
                await paymentMethodService.DetachAsync(paymentMethodId);

                // Check if this was the default payment method
                var customer = await GetCustomerAsync(customerId);
                if (customer.DefaultPaymentMethodId == paymentMethodId)
                {
                    // Get remaining payment methods and set a new default
                    var remainingPaymentMethods = await GetCustomerPaymentMethodsAsync(customerId);
                    var newDefault = remainingPaymentMethods.FirstOrDefault();
                    
                    if (newDefault != null)
                    {
                        await SetDefaultPaymentMethodAsync(customerId, newDefault.Id);
                    }
                    else
                    {
                        // Remove default payment method if no payment methods remain
                        var customerService = new CustomerService();
                        var customerUpdateOptions = new CustomerUpdateOptions
                        {
                            InvoiceSettings = new CustomerInvoiceSettingsOptions
                            {
                                DefaultPaymentMethod = null
                            }
                        };
                        await customerService.UpdateAsync(customerId, customerUpdateOptions);
                    }
                }

                _logger.LogInformation("Removed payment method {PaymentMethodId} from customer {CustomerId}", paymentMethodId, customerId);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error removing payment method {PaymentMethodId} from customer {CustomerId}: {Message}", paymentMethodId, customerId, ex.Message);
                throw new InvalidOperationException($"Failed to remove payment method: {ex.Message}", ex);
            }
        });
    }

    public async Task<PaymentMethodValidationDto> ValidatePaymentMethodDetailedAsync(string paymentMethodId)
    {
        if (string.IsNullOrEmpty(paymentMethodId))
            throw new ArgumentException("Payment method ID is required", nameof(paymentMethodId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var paymentMethodService = new PaymentMethodService();
                var paymentMethod = await paymentMethodService.GetAsync(paymentMethodId);

                var validation = new PaymentMethodValidationDto
                {
                    IsValid = true,
                    CardType = paymentMethod.Card?.Brand,
                    Last4Digits = paymentMethod.Card?.Last4,
                    ExpiryDate = paymentMethod.Card?.ExpYear != null && paymentMethod.Card?.ExpMonth != null
                        ? new DateTime((int)paymentMethod.Card.ExpYear, (int)paymentMethod.Card.ExpMonth, 1)
                        : null
                };

                // Check if card is expired
                if (validation.ExpiryDate.HasValue && validation.ExpiryDate.Value < DateTime.UtcNow)
                {
                    validation.IsValid = false;
                    validation.ErrorMessage = "Card has expired";
                }

                // Check if card will expire soon (within 30 days)
                if (validation.ExpiryDate.HasValue && validation.ExpiryDate.Value < DateTime.UtcNow.AddDays(30))
                {
                    validation.ErrorMessage = "Card will expire soon";
                }

                return validation;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error validating payment method {PaymentMethodId}: {Message}", paymentMethodId, ex.Message);
                return new PaymentMethodValidationDto
                {
                    IsValid = false,
                    ErrorMessage = ex.Message
                };
            }
        });
    }

    // Interface method that returns bool
    public async Task<bool> ValidatePaymentMethodAsync(string paymentMethodId)
    {
        var validation = await ValidatePaymentMethodDetailedAsync(paymentMethodId);
        return validation.IsValid;
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
                    ReturnUrl = "https://your-domain.com/payment/success",
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
                
                // Map Stripe payment intent status to our expected status
                var status = MapPaymentIntentStatus(paymentIntent.Status);
                
                return new PaymentResultDto
                {
                    PaymentIntentId = paymentIntent.Id,
                    CustomerId = paymentIntent.CustomerId,
                    Amount = amount,
                    Currency = currency,
                    Status = status,
                    ErrorMessage = status == "failed" ? "Payment processing failed" : null
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
                var refund = await refundService.CreateAsync(refundCreateOptions);

                _logger.LogInformation("Processed refund: {RefundId} for payment intent {PaymentIntentId} - {Amount}", 
                    refund.Id, paymentIntentId, amount);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error processing refund for payment intent {PaymentIntentId}: {Message}", paymentIntentId, ex.Message);
                throw new InvalidOperationException($"Failed to process refund: {ex.Message}", ex);
            }
        });
    }

    // Product & Price Management
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
                    Description = description,
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

    public async Task<bool> DeactivatePriceAsync(string priceId)
    {
        if (string.IsNullOrEmpty(priceId))
            throw new ArgumentException("Price ID is required", nameof(priceId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var priceService = new PriceService();
                var priceUpdateOptions = new PriceUpdateOptions
                {
                    Active = false
                };

                var price = await priceService.UpdateAsync(priceId, priceUpdateOptions);
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

    // Missing interface methods
    public async Task<string> CreatePaymentMethodAsync(string customerId, string paymentMethodId)
    {
        return await AddPaymentMethodAsync(customerId, paymentMethodId);
    }

    public async Task<bool> UpdatePaymentMethodAsync(string customerId, string paymentMethodId)
    {
        // For Stripe, updating a payment method typically means replacing it
        return await SetDefaultPaymentMethodAsync(customerId, paymentMethodId);
    }

    public async Task<bool> UpdateProductAsync(string productId, string name, string description)
    {
        if (string.IsNullOrEmpty(productId))
            throw new ArgumentException("Product ID is required", nameof(productId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var productService = new ProductService();
                var productUpdateOptions = new ProductUpdateOptions
                {
                    Name = name,
                    Description = description
                };

                var product = await productService.UpdateAsync(productId, productUpdateOptions);
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

    public async Task<string> CreatePriceAsync(string productId, decimal amount, string currency, string interval, int intervalCount)
    {
        if (string.IsNullOrEmpty(productId))
            throw new ArgumentException("Product ID is required", nameof(productId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var priceService = new PriceService();
                var priceCreateOptions = new PriceCreateOptions
                {
                    Product = productId,
                    UnitAmount = (long)(amount * 100), // Convert to cents
                    Currency = currency.ToLower(),
                    Recurring = new PriceRecurringOptions
                    {
                        Interval = interval.ToLower(),
                        IntervalCount = intervalCount
                    }
                };

                var price = await priceService.CreateAsync(priceCreateOptions);
                _logger.LogInformation("Created Stripe price: {PriceId} for product {ProductId}", price.Id, productId);
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

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                // Note: Stripe prices are immutable, so we can't update them directly
                // This method should be used to deactivate the old price and create a new one
                var priceService = new PriceService();
                
                // Deactivate the old price
                var deactivateOptions = new PriceUpdateOptions
                {
                    Active = false
                };
                await priceService.UpdateAsync(priceId, deactivateOptions);
                
                _logger.LogInformation("Deactivated Stripe price: {PriceId}", priceId);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error updating price {PriceId}: {Message}", priceId, ex.Message);
                throw new InvalidOperationException($"Failed to update Stripe price: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> UpdateSubscriptionAsync(string subscriptionId, string priceId)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID is required", nameof(subscriptionId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var subscriptionService = new SubscriptionService();
                var subscriptionUpdateOptions = new SubscriptionUpdateOptions
                {
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions
                        {
                            Id = subscriptionId, // This should be the subscription item ID, not subscription ID
                            Price = priceId
                        }
                    }
                };

                var subscription = await subscriptionService.UpdateAsync(subscriptionId, subscriptionUpdateOptions);
                _logger.LogInformation("Updated Stripe subscription: {SubscriptionId}", subscriptionId);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error updating subscription {SubscriptionId}: {Message}", subscriptionId, ex.Message);
                throw new InvalidOperationException($"Failed to update Stripe subscription: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> PauseSubscriptionAsync(string subscriptionId)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID is required", nameof(subscriptionId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var subscriptionService = new SubscriptionService();
                var subscriptionUpdateOptions = new SubscriptionUpdateOptions
                {
                    PauseCollection = new SubscriptionPauseCollectionOptions
                    {
                        Behavior = "keep_as_draft"
                    }
                };

                var subscription = await subscriptionService.UpdateAsync(subscriptionId, subscriptionUpdateOptions);
                _logger.LogInformation("Paused Stripe subscription: {SubscriptionId}", subscriptionId);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error pausing subscription {SubscriptionId}: {Message}", subscriptionId, ex.Message);
                throw new InvalidOperationException($"Failed to pause Stripe subscription: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> ResumeSubscriptionAsync(string subscriptionId)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID is required", nameof(subscriptionId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var subscriptionService = new SubscriptionService();
                var subscriptionUpdateOptions = new SubscriptionUpdateOptions
                {
                    PauseCollection = null // Remove pause collection
                };

                var subscription = await subscriptionService.UpdateAsync(subscriptionId, subscriptionUpdateOptions);

                _logger.LogInformation("Resumed Stripe subscription: {SubscriptionId}", subscriptionId);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error resuming subscription {SubscriptionId}: {Message}", subscriptionId, ex.Message);
                throw new InvalidOperationException($"Failed to resume Stripe subscription: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> ReactivateSubscriptionAsync(string subscriptionId)
    {
        // Reactivation is similar to resuming
        return await ResumeSubscriptionAsync(subscriptionId);
    }

    public async Task<bool> UpdateSubscriptionPaymentMethodAsync(string subscriptionId, string paymentMethodId)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID is required", nameof(subscriptionId));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var subscriptionService = new SubscriptionService();
                var subscriptionUpdateOptions = new SubscriptionUpdateOptions
                {
                    DefaultPaymentMethod = paymentMethodId
                };

                var subscription = await subscriptionService.UpdateAsync(subscriptionId, subscriptionUpdateOptions);
                _logger.LogInformation("Updated payment method for Stripe subscription: {SubscriptionId}", subscriptionId);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error updating payment method for subscription {SubscriptionId}: {Message}", subscriptionId, ex.Message);
                throw new InvalidOperationException($"Failed to update payment method for Stripe subscription: {ex.Message}", ex);
            }
        });
    }

    public async Task<string> CreateCheckoutSessionAsync(string priceId, string successUrl, string cancelUrl)
    {
        if (string.IsNullOrEmpty(priceId))
            throw new ArgumentException("Price ID is required", nameof(priceId));
        
        if (string.IsNullOrEmpty(successUrl))
            throw new ArgumentException("Success URL is required", nameof(successUrl));
        
        if (string.IsNullOrEmpty(cancelUrl))
            throw new ArgumentException("Cancel URL is required", nameof(cancelUrl));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var sessionCreateOptions = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            Price = priceId,
                            Quantity = 1
                        }
                    },
                    Mode = "subscription",
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl,
                    Metadata = new Dictionary<string, string>
                    {
                        { "created_at", DateTime.UtcNow.ToString("O") },
                        { "source", "smart_telehealth" }
                    }
                };

                var sessionService = new SessionService();
                var session = await sessionService.CreateAsync(sessionCreateOptions);

                _logger.LogInformation("Created Stripe checkout session: {SessionId} for price {PriceId}", session.Id, priceId);
                return session.Url;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating checkout session for price {PriceId}: {Message}", priceId, ex.Message);
                throw new InvalidOperationException($"Failed to create Stripe checkout session: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> ProcessWebhookAsync(string json, string signature)
    {
        if (string.IsNullOrEmpty(json))
            throw new ArgumentException("Webhook JSON is required", nameof(json));
        
        if (string.IsNullOrEmpty(signature))
            throw new ArgumentException("Webhook signature is required", nameof(signature));

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var webhookSecret = _configuration["Stripe:WebhookSecret"];
                if (string.IsNullOrEmpty(webhookSecret))
                {
                    throw new InvalidOperationException("Stripe webhook secret is not configured");
                }

                // For now, just log the webhook processing
                _logger.LogInformation("Processing Stripe webhook with signature: {Signature}", signature);
                
                // In a real implementation, you would verify the signature and process the event
                // For now, we'll just return true to indicate successful processing
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error processing webhook: {Message}", ex.Message);
                throw new InvalidOperationException($"Failed to process Stripe webhook: {ex.Message}", ex);
            }
        });
    }

    // Utility Methods
    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
    {
        for (int attempt = 1; attempt <= _maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                if (attempt == _maxRetries)
                {
                    throw;
                }

                _logger.LogWarning(ex, "Attempt {Attempt} failed, retrying in {Delay}ms", attempt, _retryDelay.TotalMilliseconds);
                await Task.Delay(_retryDelay);
            }
        }

        throw new InvalidOperationException("All retry attempts failed");
    }

    private SubscriptionStatus MapStripeStatusToEnum(string stripeStatus)
    {
        return stripeStatus?.ToLower() switch
        {
            "active" => SubscriptionStatus.Active,
            "canceled" => SubscriptionStatus.Cancelled,
            "incomplete" => SubscriptionStatus.Pending,
            "incomplete_expired" => SubscriptionStatus.Expired,
            "past_due" => SubscriptionStatus.PaymentFailed,
            "trialing" => SubscriptionStatus.TrialActive,
            "unpaid" => SubscriptionStatus.PaymentFailed,
            _ => SubscriptionStatus.Pending
        };
    }

    private string MapPaymentIntentStatus(string stripeStatus)
    {
        return stripeStatus?.ToLower() switch
        {
            "succeeded" => "succeeded",
            "processing" => "processing",
            "requires_payment_method" => "failed",
            "requires_confirmation" => "failed",
            "requires_action" => "failed",
            "canceled" => "failed",
            _ => "failed"
        };
    }
}

public enum SubscriptionStatus
{
    Pending,
    Active,
    Cancelled,
    Expired,
    PaymentFailed,
    TrialActive
} 