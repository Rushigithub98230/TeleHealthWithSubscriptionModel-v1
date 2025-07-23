using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace SmartTelehealth.Tests.Services
{
    public class PaymentServiceTests
    {
        private readonly Mock<IStripeService> _mockStripe;

        public PaymentServiceTests()
        {
            _mockStripe = new Mock<IStripeService>();
        }

        [Fact(DisplayName = "CreateCustomerAsync returns customer ID successfully")]
        public async Task CreateCustomerAsync_Success()
        {
            // Arrange
            var email = "test@example.com";
            var name = "Test User";
            var customerId = "cus_123";
            _mockStripe.Setup(s => s.CreateCustomerAsync(email, name)).ReturnsAsync(customerId);
            // Act
            var result = await _mockStripe.Object.CreateCustomerAsync(email, name);
            // Assert
            Assert.Equal(customerId, result);
        }

        [Fact(DisplayName = "CreateCustomerAsync throws on invalid input")]
        public async Task CreateCustomerAsync_InvalidInput_Throws()
        {
            // Arrange
            var email = "";
            var name = "Test User";
            _mockStripe.Setup(s => s.CreateCustomerAsync(email, name)).ThrowsAsync(new ArgumentException());
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _mockStripe.Object.CreateCustomerAsync(email, name));
        }

        [Fact(DisplayName = "CreatePaymentMethodAsync returns payment method ID successfully")]
        public async Task CreatePaymentMethodAsync_Success()
        {
            // Arrange
            var customerId = "cus_123";
            var paymentMethodId = "pm_123";
            _mockStripe.Setup(s => s.CreatePaymentMethodAsync(customerId, paymentMethodId)).ReturnsAsync(paymentMethodId);
            // Act
            var result = await _mockStripe.Object.CreatePaymentMethodAsync(customerId, paymentMethodId);
            // Assert
            Assert.Equal(paymentMethodId, result);
        }

        [Fact(DisplayName = "CreatePaymentMethodAsync throws on invalid input")]
        public async Task CreatePaymentMethodAsync_InvalidInput_Throws()
        {
            // Arrange
            var customerId = "";
            var paymentMethodId = "pm_123";
            _mockStripe.Setup(s => s.CreatePaymentMethodAsync(customerId, paymentMethodId)).ThrowsAsync(new ArgumentException());
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _mockStripe.Object.CreatePaymentMethodAsync(customerId, paymentMethodId));
        }

        [Fact(DisplayName = "ProcessPaymentAsync returns payment result successfully")]
        public async Task ProcessPaymentAsync_Success()
        {
            // Arrange
            var paymentMethodId = "pm_123";
            var amount = 100m;
            var currency = "usd";
            var paymentResult = new PaymentResultDto { Status = "succeeded", PaymentIntentId = "pi_123" };
            _mockStripe.Setup(s => s.ProcessPaymentAsync(paymentMethodId, amount, currency)).ReturnsAsync(paymentResult);
            // Act
            var result = await _mockStripe.Object.ProcessPaymentAsync(paymentMethodId, amount, currency);
            // Assert
            Assert.Equal("succeeded", result.Status);
            Assert.Equal("pi_123", result.PaymentIntentId);
        }

        [Fact(DisplayName = "ProcessPaymentAsync throws on Stripe error")]
        public async Task ProcessPaymentAsync_StripeError_Throws()
        {
            // Arrange
            var paymentMethodId = "pm_invalid";
            var amount = 100m;
            var currency = "usd";
            _mockStripe.Setup(s => s.ProcessPaymentAsync(paymentMethodId, amount, currency)).ThrowsAsync(new InvalidOperationException());
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _mockStripe.Object.ProcessPaymentAsync(paymentMethodId, amount, currency));
        }

        [Fact(DisplayName = "ProcessRefundAsync returns true on success")]
        public async Task ProcessRefundAsync_Success()
        {
            // Arrange
            var paymentIntentId = "pi_123";
            var amount = 50m;
            _mockStripe.Setup(s => s.ProcessRefundAsync(paymentIntentId, amount)).ReturnsAsync(true);
            // Act
            var result = await _mockStripe.Object.ProcessRefundAsync(paymentIntentId, amount);
            // Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "ProcessRefundAsync throws on error")]
        public async Task ProcessRefundAsync_Error_Throws()
        {
            // Arrange
            var paymentIntentId = "pi_invalid";
            var amount = 50m;
            _mockStripe.Setup(s => s.ProcessRefundAsync(paymentIntentId, amount)).ThrowsAsync(new InvalidOperationException());
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _mockStripe.Object.ProcessRefundAsync(paymentIntentId, amount));
        }
    }
} 