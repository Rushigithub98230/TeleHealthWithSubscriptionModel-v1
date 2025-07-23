using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IStripeService
{
    // Customer Management
    Task<string> CreateCustomerAsync(string email, string name);
    Task<CustomerDto> GetCustomerAsync(string customerId);
    
    // Payment Methods
    Task<string> CreatePaymentMethodAsync(string customerId, string paymentMethodId);
    Task<bool> UpdatePaymentMethodAsync(string customerId, string paymentMethodId);
    Task<string> AddPaymentMethodAsync(string customerId, string paymentMethodId);
    Task<bool> SetDefaultPaymentMethodAsync(string customerId, string paymentMethodId);
    Task<bool> RemovePaymentMethodAsync(string customerId, string paymentMethodId);
    Task<IEnumerable<PaymentMethodDto>> GetCustomerPaymentMethodsAsync(string customerId);
    
    // Product Management
    Task<string> CreateProductAsync(string name, string description);
    Task<bool> UpdateProductAsync(string productId, string name, string description);
    Task<bool> DeleteProductAsync(string productId);
    
    // Price Management
    Task<string> CreatePriceAsync(string productId, decimal amount, string currency, string interval, int intervalCount);
    Task<bool> UpdatePriceAsync(string priceId, decimal amount);
    Task<bool> DeactivatePriceAsync(string priceId);
    
    // Subscription Management
    Task<string> CreateSubscriptionAsync(string customerId, string priceId, string paymentMethodId);
    Task<bool> CancelSubscriptionAsync(string subscriptionId);
    Task<SubscriptionDto> GetSubscriptionAsync(string subscriptionId);
    Task<bool> UpdateSubscriptionAsync(string subscriptionId, string priceId);
    Task<bool> PauseSubscriptionAsync(string subscriptionId);
    Task<bool> ResumeSubscriptionAsync(string subscriptionId);
    Task<bool> ReactivateSubscriptionAsync(string subscriptionId);
    Task<bool> UpdateSubscriptionPaymentMethodAsync(string subscriptionId, string paymentMethodId);
    
    // Payment Processing
    Task<PaymentResultDto> ProcessPaymentAsync(string paymentMethodId, decimal amount, string currency);
    Task<bool> ProcessRefundAsync(string paymentIntentId, decimal amount);
    
    // Stripe Checkout
    Task<string> CreateCheckoutSessionAsync(string priceId, string successUrl, string cancelUrl);
} 