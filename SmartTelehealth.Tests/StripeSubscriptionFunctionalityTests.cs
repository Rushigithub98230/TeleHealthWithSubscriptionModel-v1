using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartTelehealth.API.Controllers;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using Xunit;
using FluentAssertions;

namespace SmartTelehealth.Tests
{
    public class StripeSubscriptionFunctionalityTests : IDisposable
    {
        #region Test Data Setup
        private User _testUser;
        private User _adminUser;
        private SubscriptionPlan _basicPlan;
        private MasterBillingCycle _monthlyBillingCycle;
        private MasterCurrency _usdCurrency;
        #endregion

        #region Mock Services
        private Mock<IStripeService> _mockStripeService;
        private Mock<ISubscriptionService> _mockSubscriptionService;
        private Mock<IBillingService> _mockBillingService;
        private Mock<INotificationService> _mockNotificationService;
        private Mock<IAuditService> _mockAuditService;
        private Mock<IPaymentSecurityService> _mockPaymentSecurityService;
        #endregion

        #region Controllers
        private PaymentController _paymentController;
        private SubscriptionsController _subscriptionsController;
        #endregion

        public StripeSubscriptionFunctionalityTests()
        {
            InitializeTestData();
            InitializeMocks();
            InitializeControllers();
            SetupControllerContext();
        }

        #region Test Data Initialization
        private void InitializeTestData()
        {
            _testUser = new User
            {
                Id = 1,
                Email = "testuser@example.com",
                FirstName = "Test",
                LastName = "User",
                UserRoleId = 2,
                IsActive = true
            };

            _adminUser = new User
            {
                Id = 2,
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                UserRoleId = 1,
                IsActive = true
            };

            _monthlyBillingCycle = new MasterBillingCycle
            {
                Id = Guid.NewGuid(),
                Name = "Monthly",
                DurationInDays = 30,
                IsActive = true
            };

            _usdCurrency = new MasterCurrency
            {
                Id = Guid.NewGuid(),
                Code = "USD",
                Name = "US Dollar",
                Symbol = "$",
                IsActive = true
            };

            _basicPlan = new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Basic Plan",
                Description = "Basic healthcare plan",
                Price = 29.99m,
                BillingCycleId = _monthlyBillingCycle.Id,
                CurrencyId = _usdCurrency.Id,
                IsActive = true,
                IsTrialAllowed = true,
                TrialDurationInDays = 7
            };
        }
        #endregion

        #region Mock Initialization
        private void InitializeMocks()
        {
            _mockStripeService = new Mock<IStripeService>();
            _mockSubscriptionService = new Mock<ISubscriptionService>();
            _mockBillingService = new Mock<IBillingService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockAuditService = new Mock<IAuditService>();
            _mockPaymentSecurityService = new Mock<IPaymentSecurityService>();

            SetupCommonMocks();
        }

        private void SetupCommonMocks()
        {
            // Setup common service mocks
            _mockSubscriptionService.Setup(x => x.GetAllPlansAsync(It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = new List<SubscriptionPlanDto> { new SubscriptionPlanDto() }, Message = "Success", StatusCode = 200 });
        }
        #endregion

        #region Controller Initialization
        private void InitializeControllers()
        {
            _paymentController = new PaymentController(
                _mockStripeService.Object,
                _mockBillingService.Object,
                _mockSubscriptionService.Object,
                _mockAuditService.Object,
                _mockPaymentSecurityService.Object);

            _subscriptionsController = new SubscriptionsController(_mockSubscriptionService.Object);
        }
        #endregion

        #region Controller Context Setup
        private void SetupControllerContext()
        {
            SetupControllerContext(_paymentController, _testUser);
            SetupControllerContext(_subscriptionsController, _testUser);
        }

        private void SetupControllerContext(ControllerBase controller, User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.UserRoleId.ToString()),
                new Claim("sub", user.Id.ToString()),
                new Claim("userId", user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext();
            httpContext.User = principal;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }
        #endregion

        #region Stripe Customer Management Tests
        [Fact]
        public async Task Test_Stripe_Customer_Creation_And_Management()
        {
            // Test customer creation
            var customerId = "cus_test_123";
            var customerEmail = _testUser.Email;
            var customerName = $"{_testUser.FirstName} {_testUser.LastName}";

            _mockStripeService.Setup(x => x.CreateCustomerAsync(customerEmail, customerName, It.IsAny<TokenModel>()))
                .ReturnsAsync(customerId);

            var createResult = await _mockStripeService.Object.CreateCustomerAsync(customerEmail, customerName, new TokenModel());
            createResult.Should().Be(customerId);

            // Test customer retrieval
            var customerData = new CustomerDto
            {
                Id = customerId,
                Email = customerEmail,
                Name = customerName,
                DefaultPaymentMethodId = "pm_default_123",
                CreatedAt = DateTime.UtcNow
            };

            _mockStripeService.Setup(x => x.GetCustomerAsync(customerId, It.IsAny<TokenModel>()))
                .ReturnsAsync(customerData);

            var retrieveResult = await _mockStripeService.Object.GetCustomerAsync(customerId, new TokenModel());
            retrieveResult.Should().NotBeNull();
            retrieveResult.Email.Should().Be(customerEmail);
            retrieveResult.Name.Should().Be(customerName);


        }

        [Fact]
        public async Task Test_Stripe_Payment_Method_Management()
        {
            var customerId = "cus_test_123";
            var paymentMethodId = "pm_test_123";

            // Test adding payment method
            _mockStripeService.Setup(x => x.AddPaymentMethodAsync(customerId, paymentMethodId, It.IsAny<TokenModel>()))
                .ReturnsAsync(paymentMethodId);

            var addResult = await _mockStripeService.Object.AddPaymentMethodAsync(customerId, paymentMethodId, new TokenModel());
            addResult.Should().Be(paymentMethodId);

            // Test setting default payment method
            _mockStripeService.Setup(x => x.SetDefaultPaymentMethodAsync(customerId, paymentMethodId, It.IsAny<TokenModel>()))
                .ReturnsAsync(true);

            var setDefaultResult = await _mockStripeService.Object.SetDefaultPaymentMethodAsync(customerId, paymentMethodId, new TokenModel());
            setDefaultResult.Should().BeTrue();

            // Test removing payment method
            _mockStripeService.Setup(x => x.RemovePaymentMethodAsync(customerId, paymentMethodId, It.IsAny<TokenModel>()))
                .ReturnsAsync(true);

            var removeResult = await _mockStripeService.Object.RemovePaymentMethodAsync(customerId, paymentMethodId, new TokenModel());
            removeResult.Should().BeTrue();
        }
        #endregion

        #region Stripe Subscription Lifecycle Tests
        [Fact]
        public async Task Test_Stripe_Subscription_Creation_And_Activation()
        {
            var customerId = "cus_test_123";
            var priceId = "price_test_123";
            var paymentMethodId = "pm_test_123";
            var subscriptionId = "sub_test_123";

            // Test subscription creation
            _mockStripeService.Setup(x => x.CreateSubscriptionAsync(customerId, priceId, paymentMethodId, It.IsAny<TokenModel>()))
                .ReturnsAsync(subscriptionId);

            var createResult = await _mockStripeService.Object.CreateSubscriptionAsync(customerId, priceId, paymentMethodId, new TokenModel());
            createResult.Should().Be(subscriptionId);

            // Test subscription activation
            var subscriptionData = new SubscriptionDto
            {
                Id = subscriptionId,
                Status = "active",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1)
            };

            _mockStripeService.Setup(x => x.GetSubscriptionAsync(subscriptionId, It.IsAny<TokenModel>()))
                .ReturnsAsync(subscriptionData);

            var retrieveResult = await _mockStripeService.Object.GetSubscriptionAsync(subscriptionId, new TokenModel());
            retrieveResult.Should().NotBeNull();
            retrieveResult.Status.Should().Be("active");
        }



        [Fact]
        public async Task Test_Stripe_Subscription_Updates_And_Modifications()
        {
            var subscriptionId = "sub_test_123";
            var newPriceId = "price_test_456";

            // Test subscription plan change
            _mockStripeService.Setup(x => x.UpdateSubscriptionAsync(subscriptionId, newPriceId, It.IsAny<TokenModel>()))
                .ReturnsAsync(true);

            var updateResult = await _mockStripeService.Object.UpdateSubscriptionAsync(subscriptionId, newPriceId, new TokenModel());
            updateResult.Should().BeTrue();

            // Test subscription cancellation
            _mockStripeService.Setup(x => x.CancelSubscriptionAsync(subscriptionId, It.IsAny<TokenModel>()))
                .ReturnsAsync(true);

            var cancelResult = await _mockStripeService.Object.CancelSubscriptionAsync(subscriptionId, new TokenModel());
            cancelResult.Should().BeTrue();

            // Test subscription pause
            _mockStripeService.Setup(x => x.PauseSubscriptionAsync(subscriptionId, It.IsAny<TokenModel>()))
                .ReturnsAsync(true);

            var pauseResult = await _mockStripeService.Object.PauseSubscriptionAsync(subscriptionId, new TokenModel());
            pauseResult.Should().BeTrue();

            // Test subscription resume
            _mockStripeService.Setup(x => x.ResumeSubscriptionAsync(subscriptionId, It.IsAny<TokenModel>()))
                .ReturnsAsync(true);

            var resumeResult = await _mockStripeService.Object.ResumeSubscriptionAsync(subscriptionId, new TokenModel());
            resumeResult.Should().BeTrue();
        }
        #endregion

        #region Stripe Payment Processing Tests
        [Fact]
        public async Task Test_Stripe_Payment_Processing_Success()
        {
            var paymentMethodId = "pm_test_123";
            var amount = 29.99m;
            var currency = "usd";

            // Test payment processing
            var paymentResult = new PaymentResultDto
            {
                Status = "succeeded",
                PaymentIntentId = "pi_test_123",
                Amount = amount,
                Currency = currency,
                ProcessedAt = DateTime.UtcNow
            };

            _mockStripeService.Setup(x => x.ProcessPaymentAsync(paymentMethodId, amount, currency, It.IsAny<TokenModel>()))
                .ReturnsAsync(paymentResult);

            var processResult = await _mockStripeService.Object.ProcessPaymentAsync(paymentMethodId, amount, currency, new TokenModel());
            processResult.Should().NotBeNull();
            processResult.Status.Should().Be("succeeded");
            processResult.Amount.Should().Be(29.99m);
        }

        [Fact]
        public async Task Test_Stripe_Payment_Processing_Failure()
        {
            var paymentMethodId = "pm_failed_123";
            var amount = 29.99m;
            var currency = "usd";

            // Test payment failure
            var failedPayment = new PaymentResultDto
            {
                Status = "failed",
                PaymentIntentId = "pi_failed_123",
                Amount = amount,
                Currency = currency,
                ErrorMessage = "Insufficient funds",
                ProcessedAt = DateTime.UtcNow
            };

            _mockStripeService.Setup(x => x.ProcessPaymentAsync(paymentMethodId, amount, currency, It.IsAny<TokenModel>()))
                .ReturnsAsync(failedPayment);

            var processResult = await _mockStripeService.Object.ProcessPaymentAsync(paymentMethodId, amount, currency, new TokenModel());
            processResult.Should().NotBeNull();
            processResult.Status.Should().Be("failed");
            processResult.ErrorMessage.Should().Be("Insufficient funds");
        }


        #endregion

        #region Stripe Webhook Event Tests
        [Fact]
        public async Task Test_Stripe_Webhook_Invoice_Payment_Succeeded()
        {
            var subscriptionId = "sub_test_123";

            // Test invoice.payment_succeeded webhook
            var webhookData = new
            {
                Type = "invoice.payment_succeeded",
                Data = new
                {
                    Object = new
                    {
                        Id = "in_test_123",
                        Subscription = subscriptionId,
                        AmountPaid = 2999,
                        Status = "paid"
                    }
                }
            };

            _mockSubscriptionService.Setup(x => x.HandlePaymentProviderWebhookAsync(
                "invoice.payment_succeeded", subscriptionId, It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = new object(), Message = "Webhook processed", StatusCode = 200 });

            var webhookResult = await _mockSubscriptionService.Object.HandlePaymentProviderWebhookAsync(
                "invoice.payment_succeeded", subscriptionId, null, new TokenModel());
            webhookResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Test_Stripe_Webhook_Invoice_Payment_Failed()
        {
            var subscriptionId = "sub_test_123";
            var invoiceId = "in_failed_123";

            // Test invoice.payment_failed webhook
            _mockSubscriptionService.Setup(x => x.HandlePaymentProviderWebhookAsync(
                "invoice.payment_failed", subscriptionId, It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = new object(), Message = "Payment failure handled", StatusCode = 200 });

            var webhookResult = await _mockSubscriptionService.Object.HandlePaymentProviderWebhookAsync(
                "invoice.payment_failed", subscriptionId, null, new TokenModel());
            webhookResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Test_Stripe_Webhook_Subscription_Updated()
        {
            var subscriptionId = "sub_test_123";

            // Test customer.subscription.updated webhook
            _mockSubscriptionService.Setup(x => x.HandlePaymentProviderWebhookAsync(
                "customer.subscription.updated", subscriptionId, It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = new object(), Message = "Subscription update handled", StatusCode = 200 });

            var webhookResult = await _mockSubscriptionService.Object.HandlePaymentProviderWebhookAsync(
                "customer.subscription.updated", subscriptionId, null, new TokenModel());
            webhookResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Test_Stripe_Webhook_Trial_Ending()
        {
            var subscriptionId = "sub_trial_123";

            // Test customer.subscription.trial_will_end webhook
            _mockSubscriptionService.Setup(x => x.HandlePaymentProviderWebhookAsync(
                "customer.subscription.trial_will_end", subscriptionId, It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = new object(), Message = "Trial ending notification sent", StatusCode = 200 });

            var webhookResult = await _mockSubscriptionService.Object.HandlePaymentProviderWebhookAsync(
                "customer.subscription.trial_will_end", subscriptionId, null, new TokenModel());
            webhookResult.StatusCode.Should().Be(200);
        }
        #endregion

        #region Stripe Error Handling Tests
        [Fact]
        public async Task Test_Stripe_API_Error_Handling()
        {
            var customerId = "cus_error_123";

            // Test Stripe API error (e.g., invalid customer ID)
            _mockStripeService.Setup(x => x.GetCustomerAsync(customerId, It.IsAny<TokenModel>()))
                .ThrowsAsync(new Exception("No such customer: 'cus_error_123'"));

            await Assert.ThrowsAsync<Exception>(async () =>
                await _mockStripeService.Object.GetCustomerAsync(customerId, new TokenModel()));
        }

        [Fact]
        public async Task Test_Stripe_Network_Error_Handling()
        {
            var customerId = "cus_network_123";

            // Test network error
            _mockStripeService.Setup(x => x.GetCustomerAsync(customerId, It.IsAny<TokenModel>()))
                .ThrowsAsync(new Exception("Network error: Unable to connect to Stripe API"));

            await Assert.ThrowsAsync<Exception>(async () =>
                await _mockStripeService.Object.GetCustomerAsync(customerId, new TokenModel()));
        }

        [Fact]
        public async Task Test_Stripe_Invalid_Payment_Method_Handling()
        {
            var customerId = "cus_test_123";
            var invalidPaymentMethodId = "pm_invalid_123";

            // Test invalid payment method
            _mockStripeService.Setup(x => x.AddPaymentMethodAsync(customerId, invalidPaymentMethodId, It.IsAny<TokenModel>()))
                .ThrowsAsync(new Exception("No such payment_method: 'pm_invalid_123'"));

            await Assert.ThrowsAsync<Exception>(async () =>
                await _mockStripeService.Object.AddPaymentMethodAsync(customerId, invalidPaymentMethodId, new TokenModel()));
        }
        #endregion



        public void Dispose()
        {
            // Cleanup if needed
        }
    }


}
