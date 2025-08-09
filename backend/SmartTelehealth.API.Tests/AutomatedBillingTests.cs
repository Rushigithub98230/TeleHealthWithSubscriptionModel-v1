using Microsoft.Extensions.Logging;
using Moq;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.Services;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace SmartTelehealth.API.Tests
{
    public class AutomatedBillingTests
    {
        private readonly Mock<ISubscriptionRepository> _mockSubscriptionRepository;
        private readonly Mock<IStripeService> _mockStripeService;
        private readonly Mock<IBillingService> _mockBillingService;
        private readonly Mock<ILogger<AutomatedBillingService>> _mockLogger;
        private readonly Mock<ISubscriptionStatusHistoryRepository> _mockStatusHistoryRepository;
        private readonly AutomatedBillingService _automatedBillingService;
        private readonly Mock<ISubscriptionLifecycleService> _mockLifecycleService;
        private readonly Mock<ILogger<SubscriptionLifecycleService>> _mockLifecycleLogger;

        public AutomatedBillingTests()
        {
            _mockSubscriptionRepository = new Mock<ISubscriptionRepository>();
            _mockStripeService = new Mock<IStripeService>();
            _mockBillingService = new Mock<IBillingService>();
            _mockLogger = new Mock<ILogger<AutomatedBillingService>>();
            _mockStatusHistoryRepository = new Mock<ISubscriptionStatusHistoryRepository>();
            _mockLifecycleService = new Mock<ISubscriptionLifecycleService>();
            _mockLifecycleLogger = new Mock<ILogger<SubscriptionLifecycleService>>();

            _automatedBillingService = new AutomatedBillingService(
                _mockSubscriptionRepository.Object,
                _mockBillingService.Object,
                _mockStripeService.Object,
                new Mock<IAuditService>().Object,
                _mockLogger.Object);
        }

        #region Automated Billing Tests

        [Fact]
        public async Task ProcessRecurringBillingAsync_WithDueSubscriptions_ProcessesAllBilling()
        {
            // Arrange
            var planId1 = Guid.NewGuid();
            var planId2 = Guid.NewGuid();
            var billingCycleId = Guid.NewGuid();
            
            var billingCycle = new MasterBillingCycle
            {
                Id = billingCycleId,
                Name = "Monthly",
                DurationInDays = 30,
                DurationInMonths = 1,
                IsActive = true
            };
            
            var plan1 = new SubscriptionPlan
            {
                Id = planId1,
                Name = "Basic Plan",
                Price = 99.99m,
                BillingCycleId = billingCycleId,
                BillingCycle = billingCycle,
                IsActive = true
            };
            
            var plan2 = new SubscriptionPlan
            {
                Id = planId2,
                Name = "Premium Plan",
                Price = 149.99m,
                BillingCycleId = billingCycleId,
                BillingCycle = billingCycle,
                IsActive = true
            };
            
            var dueSubscriptions = new List<Subscription>
            {
                new Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    SubscriptionPlanId = planId1,
                    BillingCycleId = billingCycleId,
                    SubscriptionPlan = plan1,
                    BillingCycle = billingCycle,
                    Status = Subscription.SubscriptionStatuses.Active,
                    NextBillingDate = DateTime.UtcNow.AddDays(-1),
                    CurrentPrice = 99.99m,
                    StripeCustomerId = "cus_test_1"
                },
                new Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    SubscriptionPlanId = planId2,
                    BillingCycleId = billingCycleId,
                    SubscriptionPlan = plan2,
                    BillingCycle = billingCycle,
                    Status = Subscription.SubscriptionStatuses.Active,
                    NextBillingDate = DateTime.UtcNow.AddDays(-2),
                    CurrentPrice = 149.99m,
                    StripeCustomerId = "cus_test_2"
                }
            };

            var paymentResult = new PaymentResultDto
            {
                Status = "succeeded",
                PaymentIntentId = "pi_test_123",
                Amount = 99.99m
            };

            _mockSubscriptionRepository.Setup(x => x.GetSubscriptionsDueForBillingAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(dueSubscriptions);

            _mockStripeService.Setup(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()))
                .ReturnsAsync(paymentResult);

            _mockSubscriptionRepository.Setup(x => x.UpdateAsync(It.IsAny<Subscription>()))
                .ReturnsAsync((Subscription s) => s);

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid id) => dueSubscriptions.FirstOrDefault(s => s.Id == id));

            // Act
            await _automatedBillingService.ProcessRecurringBillingAsync();

            // Assert
            _mockStripeService.Verify(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Exactly(2));
            _mockSubscriptionRepository.Verify(x => x.UpdateAsync(It.IsAny<Subscription>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ProcessRecurringBillingAsync_WithFailedPayments_UpdatesSubscriptionStatus()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var billingCycleId = Guid.NewGuid();
            
            var billingCycle = new MasterBillingCycle
            {
                Id = billingCycleId,
                Name = "Monthly",
                DurationInDays = 30,
                DurationInMonths = 1,
                IsActive = true
            };
            
            var plan = new SubscriptionPlan
            {
                Id = planId,
                Name = "Basic Plan",
                Price = 99.99m,
                BillingCycleId = billingCycleId,
                BillingCycle = billingCycle,
                IsActive = true
            };
            
            var dueSubscriptions = new List<Subscription>
            {
                new Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    SubscriptionPlanId = planId,
                    BillingCycleId = billingCycleId,
                    SubscriptionPlan = plan,
                    BillingCycle = billingCycle,
                    Status = Subscription.SubscriptionStatuses.Active,
                    NextBillingDate = DateTime.UtcNow.AddDays(-1),
                    CurrentPrice = 99.99m,
                    StripeCustomerId = "cus_test_1"
                }
            };

            var failedPaymentResult = new PaymentResultDto
            {
                Status = "failed",
                ErrorMessage = "Payment method declined"
            };

            _mockSubscriptionRepository.Setup(x => x.GetSubscriptionsDueForBillingAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(dueSubscriptions);

            _mockStripeService.Setup(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()))
                .ReturnsAsync(failedPaymentResult);

            _mockSubscriptionRepository.Setup(x => x.UpdateAsync(It.IsAny<Subscription>()))
                .ReturnsAsync((Subscription s) => s);

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid id) => dueSubscriptions.FirstOrDefault(s => s.Id == id));

            // Act
            await _automatedBillingService.ProcessRecurringBillingAsync();

            // Assert
            _mockSubscriptionRepository.Verify(x => x.UpdateAsync(It.Is<Subscription>(s => s.Status == Subscription.SubscriptionStatuses.PaymentFailed)), Times.Once);
        }

        [Fact]
        public async Task ProcessSubscriptionRenewalAsync_WithRenewableSubscriptions_ProcessesRenewals()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var billingCycleId = Guid.NewGuid();
            
            var billingCycle = new MasterBillingCycle
            {
                Id = billingCycleId,
                Name = "Monthly",
                DurationInDays = 30,
                DurationInMonths = 1,
                IsActive = true
            };
            
            var plan = new SubscriptionPlan
            {
                Id = planId,
                Name = "Basic Plan",
                Price = 99.99m,
                BillingCycleId = billingCycleId,
                BillingCycle = billingCycle,
                IsActive = true
            };
            
            var renewableSubscriptions = new List<Subscription>
            {
                new Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    SubscriptionPlanId = planId,
                    BillingCycleId = billingCycleId,
                    SubscriptionPlan = plan,
                    BillingCycle = billingCycle,
                    Status = Subscription.SubscriptionStatuses.Active,
                    AutoRenew = true,
                    NextBillingDate = DateTime.UtcNow.AddDays(-1),
                    CurrentPrice = 99.99m,
                    EndDate = DateTime.UtcNow.AddDays(-1)
                }
            };

            _mockSubscriptionRepository.Setup(x => x.GetAllSubscriptionsAsync())
                .ReturnsAsync(renewableSubscriptions);

            _mockSubscriptionRepository.Setup(x => x.UpdateAsync(It.IsAny<Subscription>()))
                .ReturnsAsync((Subscription s) => s);

            // Act
            await _automatedBillingService.ProcessSubscriptionRenewalAsync();

            // Assert
            _mockSubscriptionRepository.Verify(x => x.UpdateAsync(It.IsAny<Subscription>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPlanChangeAsync_WithValidData_ProcessesProration()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var newPlanId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var billingCycleId = Guid.NewGuid();
            
            var billingCycle = new MasterBillingCycle
            {
                Id = billingCycleId,
                Name = "Monthly",
                DurationInDays = 30,
                DurationInMonths = 1,
                IsActive = true
            };
            
            var currentPlan = new SubscriptionPlan
            {
                Id = planId,
                Name = "Basic Plan",
                Price = 99.99m,
                BillingCycleId = billingCycleId,
                BillingCycle = billingCycle,
                IsActive = true
            };
            
            var subscription = new Subscription
            {
                Id = subscriptionId,
                UserId = Guid.NewGuid(),
                SubscriptionPlanId = planId,
                BillingCycleId = billingCycleId,
                SubscriptionPlan = currentPlan,
                BillingCycle = billingCycle,
                Status = Subscription.SubscriptionStatuses.Active,
                CurrentPrice = 99.99m,
                NextBillingDate = DateTime.UtcNow.AddDays(15)
            };

            var newPlan = new SubscriptionPlan
            {
                Id = newPlanId,
                Name = "Premium Plan",
                Price = 149.99m,
                BillingCycleId = billingCycleId,
                BillingCycle = billingCycle,
                IsActive = true
            };

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(subscriptionId))
                .ReturnsAsync(subscription);

            _mockSubscriptionRepository.Setup(x => x.GetSubscriptionPlanByIdAsync(newPlanId))
                .ReturnsAsync(newPlan);

            _mockSubscriptionRepository.Setup(x => x.UpdateAsync(It.IsAny<Subscription>()))
                .ReturnsAsync((Subscription s) => s);

            // Act
            await _automatedBillingService.ProcessPlanChangeAsync(subscriptionId, newPlanId);

            // Assert
            _mockSubscriptionRepository.Verify(x => x.UpdatePlanAsync(It.IsAny<SubscriptionPlan>()), Times.Once);
            _mockBillingService.Verify(x => x.CreateBillingRecordAsync(It.IsAny<CreateBillingRecordDto>()), Times.Once);
        }

        [Fact]
        public async Task CalculateProratedAmountAsync_WithValidData_ReturnsCorrectAmount()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var billingCycleId = Guid.NewGuid();
            
            var billingCycle = new MasterBillingCycle
            {
                Id = billingCycleId,
                Name = "Monthly",
                DurationInDays = 30,
                DurationInMonths = 1,
                IsActive = true
            };
            
            var subscriptionPlan = new SubscriptionPlan
            {
                Id = planId,
                Name = "Basic Plan",
                Price = 100.00m,
                BillingCycleId = billingCycleId,
                BillingCycle = billingCycle,
                IsActive = true
            };
            
            var subscription = new Subscription
            {
                Id = subscriptionId,
                SubscriptionPlanId = planId,
                BillingCycleId = billingCycleId,
                SubscriptionPlan = subscriptionPlan,
                BillingCycle = billingCycle,
                CurrentPrice = 100.00m,
                NextBillingDate = DateTime.UtcNow.AddDays(15),
                Status = Subscription.SubscriptionStatuses.Active
            };

            var effectiveDate = DateTime.UtcNow;

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(subscriptionId))
                .ReturnsAsync(subscription);

            // Act
            var result = await _automatedBillingService.CalculateProratedAmountAsync(subscriptionId, effectiveDate);

            // Assert
            Assert.True(result > 0);
            Assert.True(result <= subscription.CurrentPrice);
        }

        #endregion

        #region Lifecycle Management Tests

        [Fact]
        public async Task ActivateSubscriptionAsync_WithValidSubscription_ReturnsTrue()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var subscription = new Subscription
            {
                Id = subscriptionId,
                Status = Subscription.SubscriptionStatuses.Pending,
                UserId = Guid.NewGuid()
            };

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(subscriptionId))
                .ReturnsAsync(subscription);

            _mockSubscriptionRepository.Setup(x => x.UpdateAsync(It.IsAny<Subscription>()))
                .ReturnsAsync((Subscription s) => s);

            _mockStatusHistoryRepository.Setup(x => x.CreateAsync(It.IsAny<SubscriptionStatusHistory>()))
                .ReturnsAsync(new SubscriptionStatusHistory());

            var lifecycleService = new SubscriptionLifecycleService(
                _mockSubscriptionRepository.Object,
                _mockStatusHistoryRepository.Object,
                new Mock<IAuditService>().Object,
                _mockLifecycleLogger.Object);

            // Act
            var result = await lifecycleService.ActivateSubscriptionAsync(subscriptionId);

            // Assert
            Assert.True(result);
            _mockSubscriptionRepository.Verify(x => x.UpdateAsync(It.Is<Subscription>(s => s.Status == Subscription.SubscriptionStatuses.Active)), Times.Once);
        }

        [Fact]
        public async Task PauseSubscriptionAsync_WithActiveSubscription_ReturnsTrue()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var subscription = new Subscription
            {
                Id = subscriptionId,
                Status = Subscription.SubscriptionStatuses.Active,
                UserId = Guid.NewGuid()
            };

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(subscriptionId))
                .ReturnsAsync(subscription);

            _mockSubscriptionRepository.Setup(x => x.UpdateAsync(It.IsAny<Subscription>()))
                .ReturnsAsync((Subscription s) => s);

            _mockStatusHistoryRepository.Setup(x => x.CreateAsync(It.IsAny<SubscriptionStatusHistory>()))
                .ReturnsAsync(new SubscriptionStatusHistory());

            var lifecycleService = new SubscriptionLifecycleService(
                _mockSubscriptionRepository.Object,
                _mockStatusHistoryRepository.Object,
                new Mock<IAuditService>().Object,
                _mockLifecycleLogger.Object);

            // Act
            var result = await lifecycleService.PauseSubscriptionAsync(subscriptionId, "User request");

            // Assert
            Assert.True(result);
            _mockSubscriptionRepository.Verify(x => x.UpdateAsync(It.Is<Subscription>(s => s.Status == Subscription.SubscriptionStatuses.Paused)), Times.Once);
        }

        [Fact]
        public async Task CancelSubscriptionAsync_WithActiveSubscription_ReturnsTrue()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var subscription = new Subscription
            {
                Id = subscriptionId,
                Status = Subscription.SubscriptionStatuses.Active,
                UserId = Guid.NewGuid()
            };

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(subscriptionId))
                .ReturnsAsync(subscription);

            _mockSubscriptionRepository.Setup(x => x.UpdateAsync(It.IsAny<Subscription>()))
                .ReturnsAsync((Subscription s) => s);

            _mockStatusHistoryRepository.Setup(x => x.CreateAsync(It.IsAny<SubscriptionStatusHistory>()))
                .ReturnsAsync(new SubscriptionStatusHistory());

            var lifecycleService = new SubscriptionLifecycleService(
                _mockSubscriptionRepository.Object,
                _mockStatusHistoryRepository.Object,
                new Mock<IAuditService>().Object,
                _mockLifecycleLogger.Object);

            // Act
            var result = await lifecycleService.CancelSubscriptionAsync(subscriptionId, "User request");

            // Assert
            Assert.True(result);
            _mockSubscriptionRepository.Verify(x => x.UpdateAsync(It.Is<Subscription>(s => s.Status == Subscription.SubscriptionStatuses.Cancelled)), Times.Once);
        }

        [Fact]
        public async Task ExpireSubscriptionAsync_WithActiveSubscription_ReturnsTrue()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var subscription = new Subscription
            {
                Id = subscriptionId,
                Status = Subscription.SubscriptionStatuses.Active,
                UserId = Guid.NewGuid(),
                EndDate = DateTime.UtcNow.AddDays(-1)
            };

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(subscriptionId))
                .ReturnsAsync(subscription);

            _mockSubscriptionRepository.Setup(x => x.UpdateAsync(It.IsAny<Subscription>()))
                .ReturnsAsync((Subscription s) => s);

            _mockStatusHistoryRepository.Setup(x => x.CreateAsync(It.IsAny<SubscriptionStatusHistory>()))
                .ReturnsAsync(new SubscriptionStatusHistory());

            var lifecycleService = new SubscriptionLifecycleService(
                _mockSubscriptionRepository.Object,
                _mockStatusHistoryRepository.Object,
                new Mock<IAuditService>().Object,
                _mockLifecycleLogger.Object);

            // Act
            var result = await lifecycleService.ExpireSubscriptionAsync(subscriptionId);

            // Assert
            Assert.True(result);
            _mockSubscriptionRepository.Verify(x => x.UpdateAsync(It.Is<Subscription>(s => s.Status == Subscription.SubscriptionStatuses.Expired)), Times.Once);
        }

        [Fact]
        public async Task ValidateStatusTransitionAsync_WithValidTransition_ReturnsTrue()
        {
            // Arrange
            var lifecycleService = new SubscriptionLifecycleService(
                _mockSubscriptionRepository.Object,
                _mockStatusHistoryRepository.Object,
                new Mock<IAuditService>().Object,
                _mockLifecycleLogger.Object);

            // Act
            var result = await lifecycleService.ValidateStatusTransitionAsync(Subscription.SubscriptionStatuses.Active, Subscription.SubscriptionStatuses.Paused);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateStatusTransitionAsync_WithInvalidTransition_ReturnsFalse()
        {
            // Arrange
            var lifecycleService = new SubscriptionLifecycleService(
                _mockSubscriptionRepository.Object,
                _mockStatusHistoryRepository.Object,
                new Mock<IAuditService>().Object,
                _mockLifecycleLogger.Object);

            // Act
            var result = await lifecycleService.ValidateStatusTransitionAsync(Subscription.SubscriptionStatuses.Cancelled, Subscription.SubscriptionStatuses.Active);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Payment Processing Tests

        [Fact]
        public async Task ProcessPaymentAsync_WithValidData_ReturnsSuccessResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var billingCycleId = Guid.NewGuid();
            var amount = 99.99m;
            
            var billingCycle = new MasterBillingCycle
            {
                Id = billingCycleId,
                Name = "Monthly",
                DurationInDays = 30,
                DurationInMonths = 1,
                IsActive = true
            };
            
            var plan = new SubscriptionPlan
            {
                Id = planId,
                Name = "Basic Plan",
                Price = 99.99m,
                BillingCycleId = billingCycleId,
                BillingCycle = billingCycle,
                IsActive = true
            };
            
            var subscription = new Subscription
            {
                Id = subscriptionId,
                UserId = Guid.NewGuid(),
                SubscriptionPlanId = planId,
                BillingCycleId = billingCycleId,
                SubscriptionPlan = plan,
                BillingCycle = billingCycle,
                Status = Subscription.SubscriptionStatuses.Active,
                StripeCustomerId = "cus_test_123"
            };

            var paymentResult = new PaymentResultDto
            {
                Status = "succeeded",
                PaymentIntentId = "pi_test_123",
                Amount = amount
            };

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(subscriptionId))
                .ReturnsAsync(subscription);

            _mockStripeService.Setup(x => x.ProcessPaymentAsync(It.IsAny<string>(), amount, It.IsAny<string>()))
                .ReturnsAsync(paymentResult);

            _mockSubscriptionRepository.Setup(x => x.UpdateAsync(It.IsAny<Subscription>()))
                .ReturnsAsync((Subscription s) => s);

            _mockBillingService.Setup(x => x.CreateBillingRecordAsync(It.IsAny<CreateBillingRecordDto>()))
                .ReturnsAsync(ApiResponse<BillingRecordDto>.SuccessResponse(new BillingRecordDto()));

            // Act
            var result = await _automatedBillingService.ProcessPaymentAsync(subscriptionId, amount);

            // Assert
            Assert.Equal("succeeded", result.Status);
            Assert.Equal("pi_test_123", result.PaymentIntentId);
        }

        [Fact]
        public async Task ProcessPaymentAsync_WithFailedPayment_ReturnsFailedResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var amount = 99.99m;
            var subscription = new Subscription
            {
                Id = subscriptionId,
                UserId = Guid.NewGuid(),
                Status = Subscription.SubscriptionStatuses.Active,
                StripeCustomerId = "cus_test_123"
            };

            var paymentResult = new PaymentResultDto
            {
                Status = "failed",
                ErrorMessage = "Payment method declined"
            };

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(subscriptionId))
                .ReturnsAsync(subscription);

            _mockStripeService.Setup(x => x.ProcessPaymentAsync(It.IsAny<string>(), amount, It.IsAny<string>()))
                .ReturnsAsync(paymentResult);

            // Act
            var result = await _automatedBillingService.ProcessPaymentAsync(subscriptionId, amount);

            // Assert
            Assert.Equal("failed", result.Status);
            Assert.Equal("Payment method declined", result.ErrorMessage);
        }

        #endregion

        #region Billing Cycle Tests

        [Fact]
        public async Task ValidateBillingCycleAsync_WithDueSubscription_ReturnsTrue()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var subscription = new Subscription
            {
                Id = subscriptionId,
                NextBillingDate = DateTime.UtcNow.AddDays(-1)
            };

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(subscriptionId))
                .ReturnsAsync(subscription);

            // Act
            var result = await _automatedBillingService.ValidateBillingCycleAsync(subscriptionId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateBillingCycleAsync_WithNotDueSubscription_ReturnsFalse()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var subscription = new Subscription
            {
                Id = subscriptionId,
                NextBillingDate = DateTime.UtcNow.AddDays(5)
            };

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(subscriptionId))
                .ReturnsAsync(subscription);

            // Act
            var result = await _automatedBillingService.ValidateBillingCycleAsync(subscriptionId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CalculateNextBillingDateAsync_WithMonthlyCycle_ReturnsCorrectDate()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var currentDate = DateTime.UtcNow;
            var subscription = new Subscription
            {
                Id = subscriptionId,
                NextBillingDate = currentDate,
                BillingCycle = new MasterBillingCycle
                {
                    DurationInDays = 30
                }
            };

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(subscriptionId))
                .ReturnsAsync(subscription);

            // Act
            var result = await _automatedBillingService.CalculateNextBillingDateAsync(subscriptionId);

            // Assert
            Assert.Equal(currentDate.AddMonths(1), result);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task ProcessRecurringBillingAsync_WhenServiceThrowsException_ContinuesProcessing()
        {
            // Arrange
            var dueSubscriptions = new List<Subscription>
            {
                new Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    Status = Subscription.SubscriptionStatuses.Active,
                    NextBillingDate = DateTime.UtcNow.AddDays(-1),
                    CurrentPrice = 99.99m,
                    StripeCustomerId = "cus_test_1",
                    SubscriptionPlan = new SubscriptionPlan { Price = 99.99m }
                },
                new Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    Status = Subscription.SubscriptionStatuses.Active,
                    NextBillingDate = DateTime.UtcNow.AddDays(-2),
                    CurrentPrice = 149.99m,
                    StripeCustomerId = "cus_test_2",
                    SubscriptionPlan = new SubscriptionPlan { Price = 149.99m }
                }
            };

            _mockSubscriptionRepository.Setup(x => x.GetSubscriptionsDueForBillingAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(dueSubscriptions);

            // Setup GetByIdAsync for ValidateBillingCycleAsync calls
            foreach (var subscription in dueSubscriptions)
            {
                _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(subscription.Id))
                    .ReturnsAsync(subscription);
            }

            var failedPaymentResult = new PaymentResultDto
            {
                Status = "failed",
                ErrorMessage = "Payment service unavailable"
            };

            _mockStripeService.Setup(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()))
                .ReturnsAsync(failedPaymentResult);

            _mockSubscriptionRepository.Setup(x => x.UpdateAsync(It.IsAny<Subscription>()))
                .ReturnsAsync((Subscription s) => s);

            // Act & Assert
            // Should not throw exception, should continue processing other subscriptions
            await _automatedBillingService.ProcessRecurringBillingAsync();

            // Verify that the service attempted to process both subscriptions
            _mockStripeService.Verify(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Exactly(2));
            _mockSubscriptionRepository.Verify(x => x.UpdateAsync(It.IsAny<Subscription>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ProcessPaymentAsync_WithNonExistentSubscription_ReturnsErrorResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var amount = 99.99m;

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(subscriptionId))
                .ReturnsAsync((Subscription?)null);

            // Act
            var result = await _automatedBillingService.ProcessPaymentAsync(subscriptionId, amount);

            // Assert
            Assert.Equal("failed", result.Status);
            Assert.Equal("Subscription not found", result.ErrorMessage);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task CompleteBillingWorkflow_FromDueToProcessed_WorksEndToEnd()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var billingCycleId = Guid.NewGuid();
            
            var billingCycle = new MasterBillingCycle
            {
                Id = billingCycleId,
                Name = "Monthly",
                DurationInDays = 30,
                DurationInMonths = 1,
                IsActive = true
            };
            
            var plan = new SubscriptionPlan
            {
                Id = planId,
                Name = "Basic Plan",
                Price = 99.99m,
                BillingCycleId = billingCycleId,
                BillingCycle = billingCycle,
                IsActive = true
            };
            
            var dueSubscriptions = new List<Subscription>
            {
                new Subscription
                {
                    Id = subscriptionId,
                    UserId = Guid.NewGuid(),
                    SubscriptionPlanId = planId,
                    BillingCycleId = billingCycleId,
                    SubscriptionPlan = plan,
                    BillingCycle = billingCycle,
                    Status = Subscription.SubscriptionStatuses.Active,
                    NextBillingDate = DateTime.UtcNow.AddDays(-1),
                    CurrentPrice = 99.99m,
                    StripeCustomerId = "cus_test_1"
                }
            };

            var paymentResult = new PaymentResultDto
            {
                Status = "succeeded",
                PaymentIntentId = "pi_test_123",
                Amount = 99.99m
            };

            _mockSubscriptionRepository.Setup(x => x.GetSubscriptionsDueForBillingAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(dueSubscriptions);

            // Setup GetByIdAsync for ValidateBillingCycleAsync calls
            foreach (var subscription in dueSubscriptions)
            {
                _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(subscription.Id))
                    .ReturnsAsync(subscription);
            }

            _mockStripeService.Setup(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()))
                .ReturnsAsync(paymentResult);

            _mockSubscriptionRepository.Setup(x => x.UpdateAsync(It.IsAny<Subscription>()))
                .ReturnsAsync((Subscription s) => s);

            _mockBillingService.Setup(x => x.CreateBillingRecordAsync(It.IsAny<CreateBillingRecordDto>()))
                .ReturnsAsync(ApiResponse<BillingRecordDto>.SuccessResponse(new BillingRecordDto()));

            // Act
            await _automatedBillingService.ProcessRecurringBillingAsync();

            // Assert
            _mockStripeService.Verify(x => x.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Once);
            _mockSubscriptionRepository.Verify(x => x.UpdateAsync(It.IsAny<Subscription>()), Times.Exactly(1));
            _mockBillingService.Verify(x => x.CreateBillingRecordAsync(It.IsAny<CreateBillingRecordDto>()), Times.Once);
        }

        #endregion
    }
}
