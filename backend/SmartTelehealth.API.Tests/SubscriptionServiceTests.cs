using Microsoft.Extensions.Logging;
using Moq;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.Services;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using Xunit;
using AutoMapper;

namespace SmartTelehealth.API.Tests
{
    public class SubscriptionServiceTests
    {
        private readonly Mock<ISubscriptionRepository> _mockSubscriptionRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<SubscriptionService>> _mockLogger;
        private readonly Mock<IStripeService> _mockStripeService;
        private readonly Mock<PrivilegeService> _mockPrivilegeService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ISubscriptionPlanPrivilegeRepository> _mockPlanPrivilegeRepo;
        private readonly Mock<IUserSubscriptionPrivilegeUsageRepository> _mockUsageRepo;
        private readonly Mock<IBillingService> _mockBillingService;
        private readonly SubscriptionService _subscriptionService;

        public SubscriptionServiceTests()
        {
            _mockSubscriptionRepository = new Mock<ISubscriptionRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<SubscriptionService>>();
            _mockStripeService = new Mock<IStripeService>();
            _mockPrivilegeService = new Mock<PrivilegeService>(
                new Mock<IPrivilegeRepository>().Object,
                new Mock<ISubscriptionPlanPrivilegeRepository>().Object,
                new Mock<IUserSubscriptionPrivilegeUsageRepository>().Object,
                new Mock<ISubscriptionRepository>().Object,
                new Mock<ILogger<PrivilegeService>>().Object);
            _mockNotificationService = new Mock<INotificationService>();
            _mockAuditService = new Mock<IAuditService>();
            _mockUserService = new Mock<IUserService>();
            _mockPlanPrivilegeRepo = new Mock<ISubscriptionPlanPrivilegeRepository>();
            _mockUsageRepo = new Mock<IUserSubscriptionPrivilegeUsageRepository>();
            _mockBillingService = new Mock<IBillingService>();

            _subscriptionService = new SubscriptionService(
                _mockSubscriptionRepository.Object,
                _mockMapper.Object,
                _mockLogger.Object,
                _mockStripeService.Object,
                _mockPrivilegeService.Object,
                _mockNotificationService.Object,
                _mockAuditService.Object,
                _mockUserService.Object,
                _mockPlanPrivilegeRepo.Object,
                _mockUsageRepo.Object,
                _mockBillingService.Object);
        }

        #region Subscription Creation Tests

        [Fact]
        public async Task CreateSubscriptionAsync_WithValidData_ReturnsSuccessResponse()
        {
            // Arrange
            var createDto = new CreateSubscriptionDto
            {
                UserId = Guid.NewGuid().ToString(),
                PlanId = Guid.NewGuid().ToString(),
                Price = 99.99m,
                BillingCycleId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                AutoRenew = true
            };

            var subscriptionPlan = new SubscriptionPlan
            {
                Id = Guid.Parse(createDto.PlanId),
                Name = "Premium Plan",
                Price = 99.99m,
                IsActive = true,
                IsTrialAllowed = false,
                TrialDurationInDays = 0
            };

            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse(createDto.UserId),
                SubscriptionPlanId = Guid.Parse(createDto.PlanId),
                Status = Subscription.SubscriptionStatuses.Active,
                StartDate = DateTime.UtcNow,
                NextBillingDate = DateTime.UtcNow.AddMonths(1),
                CurrentPrice = createDto.Price,
                AutoRenew = createDto.AutoRenew
            };

            var subscriptionDto = new SubscriptionDto
            {
                Id = subscription.Id.ToString(),
                UserId = subscription.UserId.ToString(),
                PlanId = subscription.SubscriptionPlanId.ToString(),
                Status = subscription.Status,
                CurrentPrice = subscription.CurrentPrice,
                StartDate = subscription.StartDate,
                NextBillingDate = subscription.NextBillingDate
            };

            var user = new User
            {
                Id = Guid.Parse(createDto.UserId),
                Email = "test@example.com"
            };

            _mockSubscriptionRepository.Setup(x => x.GetSubscriptionPlanByIdAsync(Guid.Parse(createDto.PlanId)))
                .ReturnsAsync(subscriptionPlan);

            _mockSubscriptionRepository.Setup(x => x.GetByUserIdAsync(Guid.Parse(createDto.UserId)))
                .ReturnsAsync(new List<Subscription>());

            _mockMapper.Setup(x => x.Map<Subscription>(createDto))
                .Returns(subscription);

            _mockSubscriptionRepository.Setup(x => x.CreateAsync(subscription))
                .ReturnsAsync(subscription);

            _mockMapper.Setup(x => x.Map<SubscriptionDto>(subscription))
                .Returns(subscriptionDto);

            _mockUserService.Setup(x => x.GetUserByIdAsync(createDto.UserId))
                .ReturnsAsync(ApiResponse<UserDto>.SuccessResponse(new UserDto { Id = createDto.UserId, Email = "test@example.com" }));

            _mockAuditService.Setup(x => x.LogUserActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _mockSubscriptionRepository.Setup(x => x.AddStatusHistoryAsync(It.IsAny<SubscriptionStatusHistory>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _subscriptionService.CreateSubscriptionAsync(createDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Subscription created", result.Message);
            Assert.Equal(subscriptionDto.Id, result.Data.Id);
            Assert.Equal(subscriptionDto.Status, result.Data.Status);
        }

        [Fact]
        public async Task CreateSubscriptionAsync_WithTrialPlan_ReturnsTrialSubscription()
        {
            // Arrange
            var createDto = new CreateSubscriptionDto
            {
                UserId = Guid.NewGuid().ToString(),
                PlanId = Guid.NewGuid().ToString(),
                Price = 99.99m
            };

            var subscriptionPlan = new SubscriptionPlan
            {
                Id = Guid.Parse(createDto.PlanId),
                Name = "Trial Plan",
                Price = 99.99m,
                IsActive = true,
                IsTrialAllowed = true,
                TrialDurationInDays = 7
            };

            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse(createDto.UserId),
                SubscriptionPlanId = Guid.Parse(createDto.PlanId),
                Status = Subscription.SubscriptionStatuses.TrialActive,
                IsTrialSubscription = true,
                TrialStartDate = DateTime.UtcNow,
                TrialEndDate = DateTime.UtcNow.AddDays(7),
                TrialDurationInDays = 7
            };

            var subscriptionDto = new SubscriptionDto
            {
                Id = subscription.Id.ToString(),
                Status = subscription.Status,
                IsTrialSubscription = true,
                TrialStartDate = subscription.TrialStartDate,
                TrialEndDate = subscription.TrialEndDate
            };

            var user = new User
            {
                Id = Guid.Parse(createDto.UserId),
                Email = "test@example.com"
            };

            _mockSubscriptionRepository.Setup(x => x.GetSubscriptionPlanByIdAsync(Guid.Parse(createDto.PlanId)))
                .ReturnsAsync(subscriptionPlan);

            _mockSubscriptionRepository.Setup(x => x.GetByUserIdAsync(Guid.Parse(createDto.UserId)))
                .ReturnsAsync(new List<Subscription>());

            _mockMapper.Setup(x => x.Map<Subscription>(createDto))
                .Returns(subscription);

            _mockSubscriptionRepository.Setup(x => x.CreateAsync(subscription))
                .ReturnsAsync(subscription);

            _mockMapper.Setup(x => x.Map<SubscriptionDto>(subscription))
                .Returns(subscriptionDto);

            _mockUserService.Setup(x => x.GetUserByIdAsync(createDto.UserId))
                .ReturnsAsync(ApiResponse<UserDto>.SuccessResponse(new UserDto { Id = createDto.UserId, Email = "test@example.com" }));

            _mockAuditService.Setup(x => x.LogUserActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _mockSubscriptionRepository.Setup(x => x.AddStatusHistoryAsync(It.IsAny<SubscriptionStatusHistory>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _subscriptionService.CreateSubscriptionAsync(createDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(Subscription.SubscriptionStatuses.TrialActive, result.Data.Status);
            Assert.True(result.Data.IsTrialSubscription);
        }

        [Fact]
        public async Task CreateSubscriptionAsync_WithInactivePlan_ReturnsErrorResponse()
        {
            // Arrange
            var createDto = new CreateSubscriptionDto
            {
                UserId = Guid.NewGuid().ToString(),
                PlanId = Guid.NewGuid().ToString(),
                Price = 99.99m
            };

            var subscriptionPlan = new SubscriptionPlan
            {
                Id = Guid.Parse(createDto.PlanId),
                Name = "Inactive Plan",
                Price = 99.99m,
                IsActive = false
            };

            _mockSubscriptionRepository.Setup(x => x.GetSubscriptionPlanByIdAsync(Guid.Parse(createDto.PlanId)))
                .ReturnsAsync(subscriptionPlan);

            // Act
            var result = await _subscriptionService.CreateSubscriptionAsync(createDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Subscription plan is not active", result.Message);
        }

        [Fact]
        public async Task CreateSubscriptionAsync_WithDuplicateSubscription_ReturnsErrorResponse()
        {
            // Arrange
            var createDto = new CreateSubscriptionDto
            {
                UserId = Guid.NewGuid().ToString(),
                PlanId = Guid.NewGuid().ToString(),
                Price = 99.99m
            };

            var subscriptionPlan = new SubscriptionPlan
            {
                Id = Guid.Parse(createDto.PlanId),
                Name = "Premium Plan",
                Price = 99.99m,
                IsActive = true
            };

            var existingSubscription = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse(createDto.UserId),
                SubscriptionPlanId = Guid.Parse(createDto.PlanId),
                Status = Subscription.SubscriptionStatuses.Active
            };

            _mockSubscriptionRepository.Setup(x => x.GetSubscriptionPlanByIdAsync(Guid.Parse(createDto.PlanId)))
                .ReturnsAsync(subscriptionPlan);

            _mockSubscriptionRepository.Setup(x => x.GetByUserIdAsync(Guid.Parse(createDto.UserId)))
                .ReturnsAsync(new List<Subscription> { existingSubscription });

            // Act
            var result = await _subscriptionService.CreateSubscriptionAsync(createDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User already has an active or paused subscription for this plan", result.Message);
        }

        #endregion

        #region Subscription Lifecycle Tests

        [Fact]
        public async Task PauseSubscriptionAsync_WithActiveSubscription_ReturnsSuccessResponse()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var subscription = new Subscription
            {
                Id = Guid.Parse(subscriptionId),
                Status = Subscription.SubscriptionStatuses.Active,
                UserId = Guid.NewGuid()
            };

            var pausedSubscription = new Subscription
            {
                Id = Guid.Parse(subscriptionId),
                Status = Subscription.SubscriptionStatuses.Paused,
                PausedDate = DateTime.UtcNow
            };

            var subscriptionDto = new SubscriptionDto
            {
                Id = subscriptionId,
                Status = Subscription.SubscriptionStatuses.Paused,
                PausedDate = DateTime.UtcNow
            };

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(Guid.Parse(subscriptionId)))
                .ReturnsAsync(subscription);

            _mockSubscriptionRepository.Setup(x => x.UpdateAsync(It.IsAny<Subscription>()))
                .ReturnsAsync(pausedSubscription);

            _mockMapper.Setup(x => x.Map<SubscriptionDto>(pausedSubscription))
                .Returns(subscriptionDto);

            _mockAuditService.Setup(x => x.LogUserActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _mockSubscriptionRepository.Setup(x => x.AddStatusHistoryAsync(It.IsAny<SubscriptionStatusHistory>()))
                .Returns(Task.CompletedTask);

            _mockUserService.Setup(x => x.GetUserByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<UserDto>.SuccessResponse(new UserDto { Email = "testuser@test.com", FullName = "Test User" }));

            // Act
            var result = await _subscriptionService.PauseSubscriptionAsync(subscriptionId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(Subscription.SubscriptionStatuses.Paused, result.Data.Status);
        }

        [Fact]
        public async Task ResumeSubscriptionAsync_WithPausedSubscription_ReturnsSuccessResponse()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var subscription = new Subscription
            {
                Id = Guid.Parse(subscriptionId),
                Status = Subscription.SubscriptionStatuses.Paused,
                UserId = Guid.NewGuid()
            };

            var resumedSubscription = new Subscription
            {
                Id = Guid.Parse(subscriptionId),
                Status = Subscription.SubscriptionStatuses.Active,
                ResumedDate = DateTime.UtcNow
            };

            var subscriptionDto = new SubscriptionDto
            {
                Id = subscriptionId,
                Status = Subscription.SubscriptionStatuses.Active,
                ResumedDate = DateTime.UtcNow
            };

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(Guid.Parse(subscriptionId)))
                .ReturnsAsync(subscription);

            _mockSubscriptionRepository.Setup(x => x.UpdateAsync(It.IsAny<Subscription>()))
                .ReturnsAsync(resumedSubscription);

            _mockMapper.Setup(x => x.Map<SubscriptionDto>(resumedSubscription))
                .Returns(subscriptionDto);

            _mockAuditService.Setup(x => x.LogUserActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _mockSubscriptionRepository.Setup(x => x.AddStatusHistoryAsync(It.IsAny<SubscriptionStatusHistory>()))
                .Returns(Task.CompletedTask);

            _mockUserService.Setup(x => x.GetUserByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<UserDto>.SuccessResponse(new UserDto { Email = "testuser@test.com", FullName = "Test User" }));

            // Act
            var result = await _subscriptionService.ResumeSubscriptionAsync(subscriptionId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(Subscription.SubscriptionStatuses.Active, result.Data.Status);
        }

        [Fact]
        public async Task CancelSubscriptionAsync_WithActiveSubscription_ReturnsSuccessResponse()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var subscription = new Subscription
            {
                Id = Guid.Parse(subscriptionId),
                Status = Subscription.SubscriptionStatuses.Active,
                UserId = Guid.NewGuid()
            };

            var cancelledSubscription = new Subscription
            {
                Id = Guid.Parse(subscriptionId),
                Status = Subscription.SubscriptionStatuses.Cancelled,
                CancelledDate = DateTime.UtcNow,
                CancellationReason = "User request"
            };

            var subscriptionDto = new SubscriptionDto
            {
                Id = subscriptionId,
                Status = Subscription.SubscriptionStatuses.Cancelled,
                CancelledDate = DateTime.UtcNow,
                CancellationReason = "User request"
            };

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(Guid.Parse(subscriptionId)))
                .ReturnsAsync(subscription);

            _mockSubscriptionRepository.Setup(x => x.UpdateAsync(It.IsAny<Subscription>()))
                .ReturnsAsync(cancelledSubscription);

            _mockMapper.Setup(x => x.Map<SubscriptionDto>(cancelledSubscription))
                .Returns(subscriptionDto);

            _mockAuditService.Setup(x => x.LogUserActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _mockSubscriptionRepository.Setup(x => x.AddStatusHistoryAsync(It.IsAny<SubscriptionStatusHistory>()))
                .Returns(Task.CompletedTask);

            _mockUserService.Setup(x => x.GetUserByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<UserDto>.SuccessResponse(new UserDto { Email = "testuser@test.com", FullName = "Test User" }));

            // Act
            var result = await _subscriptionService.CancelSubscriptionAsync(subscriptionId, "User request");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(Subscription.SubscriptionStatuses.Cancelled, result.Data.Status);
        }

        #endregion

        #region Usage Tracking Tests

        [Fact]
        public async Task GetUsageStatisticsAsync_WithValidSubscription_ReturnsUsageData()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var subscription = new Subscription
            {
                Id = Guid.Parse(subscriptionId),
                SubscriptionPlan = new SubscriptionPlan { Name = "Premium Plan" },
                StartDate = DateTime.UtcNow.AddMonths(-1),
                NextBillingDate = DateTime.UtcNow.AddMonths(1)
            };

            var usages = new List<UserSubscriptionPrivilegeUsage>
            {
                new UserSubscriptionPrivilegeUsage
                {
                    UsedValue = 3,
                    SubscriptionPlanPrivilege = new SubscriptionPlanPrivilege
                    {
                        Privilege = new Privilege { Name = "Consultations" },
                        Value = 10
                    }
                }
            };

            var planPrivileges = new List<SubscriptionPlanPrivilege>
            {
                new SubscriptionPlanPrivilege
                {
                    Privilege = new Privilege { Name = "Consultations" },
                    Value = 10
                }
            };

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(Guid.Parse(subscriptionId)))
                .ReturnsAsync(subscription);

            _mockUsageRepo.Setup(x => x.GetBySubscriptionIdAsync(subscription.Id))
                .ReturnsAsync(usages);

            _mockPlanPrivilegeRepo.Setup(x => x.GetByPlanIdAsync(subscription.SubscriptionPlanId))
                .ReturnsAsync(planPrivileges);

            // Act
            var result = await _subscriptionService.GetUsageStatisticsAsync(subscriptionId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Premium Plan", result.Data.PlanName);
            Assert.Equal(1, result.Data.TotalPrivileges);
            Assert.Equal(1, result.Data.UsedPrivileges);
        }

        [Fact]
        public async Task CanUsePrivilegeAsync_WithAvailablePrivilege_ReturnsTrue()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var privilegeName = "Consultations";
            var subscription = new Subscription
            {
                Id = Guid.Parse(subscriptionId),
                Status = Subscription.SubscriptionStatuses.Active,
                SubscriptionPlanId = Guid.NewGuid()
            };

            var planPrivileges = new List<SubscriptionPlanPrivilege>
            {
                new SubscriptionPlanPrivilege
                {
                    Id = Guid.NewGuid(),
                    Privilege = new Privilege { Name = privilegeName },
                    Value = 10
                }
            };

            var usages = new List<UserSubscriptionPrivilegeUsage>
            {
                new UserSubscriptionPrivilegeUsage
                {
                    UsedValue = 3,
                    SubscriptionPlanPrivilegeId = planPrivileges[0].Id
                }
            };

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(Guid.Parse(subscriptionId)))
                .ReturnsAsync(subscription);

            _mockPlanPrivilegeRepo.Setup(x => x.GetByPlanIdAsync(subscription.SubscriptionPlanId))
                .ReturnsAsync(planPrivileges);

            _mockUsageRepo.Setup(x => x.GetBySubscriptionIdAsync(subscription.Id))
                .ReturnsAsync(usages);

            // Act
            var result = await _subscriptionService.CanUsePrivilegeAsync(subscriptionId, privilegeName);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task CanUsePrivilegeAsync_WithExhaustedPrivilege_ReturnsFalse()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var privilegeName = "Consultations";
            var subscription = new Subscription
            {
                Id = Guid.Parse(subscriptionId),
                Status = Subscription.SubscriptionStatuses.Active,
                SubscriptionPlanId = Guid.NewGuid()
            };

            var planPrivileges = new List<SubscriptionPlanPrivilege>
            {
                new SubscriptionPlanPrivilege
                {
                    Id = Guid.NewGuid(),
                    Privilege = new Privilege { Name = privilegeName },
                    Value = 5
                }
            };

            var usages = new List<UserSubscriptionPrivilegeUsage>
            {
                new UserSubscriptionPrivilegeUsage
                {
                    UsedValue = 5,
                    SubscriptionPlanPrivilegeId = planPrivileges[0].Id
                }
            };

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(Guid.Parse(subscriptionId)))
                .ReturnsAsync(subscription);

            _mockPlanPrivilegeRepo.Setup(x => x.GetByPlanIdAsync(subscription.SubscriptionPlanId))
                .ReturnsAsync(planPrivileges);

            _mockUsageRepo.Setup(x => x.GetBySubscriptionIdAsync(subscription.Id))
                .ReturnsAsync(usages);

            // Act
            var result = await _subscriptionService.CanUsePrivilegeAsync(subscriptionId, privilegeName);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Usage limit reached", result.Message);
        }

        [Fact]
        public async Task CanUsePrivilegeAsync_WithInactiveSubscription_ReturnsError()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var privilegeName = "Consultations";
            var subscription = new Subscription
            {
                Id = Guid.Parse(subscriptionId),
                Status = Subscription.SubscriptionStatuses.Cancelled
            };

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(Guid.Parse(subscriptionId)))
                .ReturnsAsync(subscription);

            // Act
            var result = await _subscriptionService.CanUsePrivilegeAsync(subscriptionId, privilegeName);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Subscription not active", result.Message);
        }

        #endregion

        #region Payment Processing Tests

        [Fact]
        public async Task ProcessPaymentAsync_WithValidPayment_ReturnsSuccessResponse()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var paymentRequest = new PaymentRequestDto
            {
                PaymentMethodId = "pm_test_123",
                Amount = 99.99m,
                Currency = "usd"
            };

            var subscription = new Subscription
            {
                Id = Guid.Parse(subscriptionId),
                Status = Subscription.SubscriptionStatuses.PaymentFailed,
                StripeCustomerId = "cus_test_123"
            };

            var paymentResult = new PaymentResultDto
            {
                Status = "succeeded",
                PaymentIntentId = "pi_test_123",
                Amount = 99.99m
            };

            var updatedSubscription = new Subscription
            {
                Id = Guid.Parse(subscriptionId),
                Status = Subscription.SubscriptionStatuses.Active
            };

            var subscriptionDto = new SubscriptionDto
            {
                Id = subscriptionId,
                Status = Subscription.SubscriptionStatuses.Active
            };

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(Guid.Parse(subscriptionId)))
                .ReturnsAsync(subscription);

            _mockStripeService.Setup(x => x.ProcessPaymentAsync(
                paymentRequest.PaymentMethodId,
                paymentRequest.Amount,
                paymentRequest.Currency))
                .ReturnsAsync(paymentResult);

            _mockSubscriptionRepository.Setup(x => x.UpdateAsync(It.IsAny<Subscription>()))
                .ReturnsAsync(updatedSubscription);

            _mockMapper.Setup(x => x.Map<SubscriptionDto>(updatedSubscription))
                .Returns(subscriptionDto);

            // Act
            var result = await _subscriptionService.ProcessPaymentAsync(subscriptionId, paymentRequest);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("succeeded", result.Data.Status);
        }

        [Fact]
        public async Task ProcessPaymentAsync_WithFailedPayment_ReturnsErrorResponse()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var paymentRequest = new PaymentRequestDto
            {
                PaymentMethodId = "pm_test_123",
                Amount = 99.99m,
                Currency = "usd"
            };

            var subscription = new Subscription
            {
                Id = Guid.Parse(subscriptionId),
                Status = Subscription.SubscriptionStatuses.Active,
                StripeCustomerId = "cus_test_123"
            };

            var paymentResult = new PaymentResultDto
            {
                Status = "failed",
                ErrorMessage = "Payment method declined"
            };

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(Guid.Parse(subscriptionId)))
                .ReturnsAsync(subscription);

            _mockStripeService.Setup(x => x.ProcessPaymentAsync(
                paymentRequest.PaymentMethodId,
                paymentRequest.Amount,
                paymentRequest.Currency))
                .ReturnsAsync(paymentResult);

            // Act
            var result = await _subscriptionService.ProcessPaymentAsync(subscriptionId, paymentRequest);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Payment failed", result.Message);
        }

        #endregion

        #region Admin Operations Tests

        [Fact]
        public async Task GetAllPlansAsync_ReturnsAllPlans()
        {
            // Arrange
            var plans = new List<SubscriptionPlan>
            {
                new SubscriptionPlan
                {
                    Id = Guid.NewGuid(),
                    Name = "Basic Plan",
                    Price = 29.99m,
                    IsActive = true
                },
                new SubscriptionPlan
                {
                    Id = Guid.NewGuid(),
                    Name = "Premium Plan",
                    Price = 99.99m,
                    IsActive = true
                }
            };

            var planDtos = plans.Select(p => new SubscriptionPlanDto
            {
                Id = p.Id.ToString(),
                Name = p.Name,
                Price = p.Price,
                IsActive = p.IsActive
            }).ToList();

            _mockSubscriptionRepository.Setup(x => x.GetAllSubscriptionPlansAsync())
                .ReturnsAsync(plans);

            _mockMapper.Setup(x => x.Map<IEnumerable<SubscriptionPlanDto>>(plans))
                .Returns(planDtos);

            // Act
            var result = await _subscriptionService.GetAllPlansAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task CreatePlanAsync_WithValidData_ReturnsCreatedPlan()
        {
            // Arrange
            var createDto = new CreateSubscriptionPlanDto
            {
                Name = "New Plan",
                Description = "A new subscription plan",
                Price = 49.99m,
                BillingCycleId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                IsActive = true
            };

            var plan = new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = createDto.Name,
                Description = createDto.Description,
                Price = createDto.Price,
                BillingCycleId = createDto.BillingCycleId,
                CurrencyId = createDto.CurrencyId,
                IsActive = createDto.IsActive
            };

            var planDto = new SubscriptionPlanDto
            {
                Id = plan.Id.ToString(),
                Name = plan.Name,
                Price = plan.Price,
                IsActive = plan.IsActive
            };

            _mockSubscriptionRepository.Setup(x => x.CreateSubscriptionPlanAsync(It.IsAny<SubscriptionPlan>()))
                .ReturnsAsync(plan);

            _mockMapper.Setup(x => x.Map<SubscriptionPlanDto>(plan))
                .Returns(planDto);

            // Act
            var result = await _subscriptionService.CreatePlanAsync(createDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(createDto.Name, result.Data.Name);
            Assert.Equal(createDto.Price, result.Data.Price);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task CreateSubscriptionAsync_WhenRepositoryThrowsException_ReturnsErrorResponse()
        {
            // Arrange
            var createDto = new CreateSubscriptionDto
            {
                UserId = Guid.NewGuid().ToString(),
                PlanId = Guid.NewGuid().ToString(),
                Price = 99.99m
            };

            var subscriptionPlan = new SubscriptionPlan
            {
                Id = Guid.Parse(createDto.PlanId),
                Name = "Premium Plan",
                Price = 99.99m,
                IsActive = true
            };

            _mockSubscriptionRepository.Setup(x => x.GetSubscriptionPlanByIdAsync(Guid.Parse(createDto.PlanId)))
                .ReturnsAsync(subscriptionPlan);

            _mockSubscriptionRepository.Setup(x => x.GetByUserIdAsync(Guid.Parse(createDto.UserId)))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _subscriptionService.CreateSubscriptionAsync(createDto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Failed to create subscription", result.Message);
        }

        [Fact]
        public async Task GetSubscriptionAsync_WithNonExistentSubscription_ReturnsErrorResponse()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(Guid.Parse(subscriptionId)))
                .ReturnsAsync((Subscription?)null);

            // Act
            var result = await _subscriptionService.GetSubscriptionAsync(subscriptionId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Subscription not found", result.Message);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task CompleteSubscriptionWorkflow_FromCreationToUsage_WorksEndToEnd()
        {
            // Arrange
            var createDto = new CreateSubscriptionDto
            {
                UserId = Guid.NewGuid().ToString(),
                PlanId = Guid.NewGuid().ToString(),
                Price = 99.99m
            };

            var subscriptionPlan = new SubscriptionPlan
            {
                Id = Guid.Parse(createDto.PlanId),
                Name = "Premium Plan",
                Price = 99.99m,
                IsActive = true,
                IsTrialAllowed = false
            };

            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse(createDto.UserId),
                SubscriptionPlanId = Guid.Parse(createDto.PlanId),
                Status = Subscription.SubscriptionStatuses.Active,
                StartDate = DateTime.UtcNow,
                NextBillingDate = DateTime.UtcNow.AddMonths(1),
                SubscriptionPlan = subscriptionPlan
            };

            var subscriptionDto = new SubscriptionDto
            {
                Id = subscription.Id.ToString(),
                Status = subscription.Status
            };

            var user = new User
            {
                Id = Guid.Parse(createDto.UserId),
                Email = "test@example.com"
            };

            // Mock subscription creation
            _mockSubscriptionRepository.Setup(x => x.GetSubscriptionPlanByIdAsync(Guid.Parse(createDto.PlanId)))
                .ReturnsAsync(subscriptionPlan);

            _mockSubscriptionRepository.Setup(x => x.GetByUserIdAsync(Guid.Parse(createDto.UserId)))
                .ReturnsAsync(new List<Subscription>());

            _mockMapper.Setup(x => x.Map<Subscription>(createDto))
                .Returns(subscription);

            _mockSubscriptionRepository.Setup(x => x.CreateAsync(subscription))
                .ReturnsAsync(subscription);

            _mockMapper.Setup(x => x.Map<SubscriptionDto>(subscription))
                .Returns(subscriptionDto);

            _mockUserService.Setup(x => x.GetUserByIdAsync(createDto.UserId))
                .ReturnsAsync(ApiResponse<UserDto>.SuccessResponse(new UserDto { Id = createDto.UserId, Email = "test@example.com" }));

            _mockAuditService.Setup(x => x.LogUserActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _mockSubscriptionRepository.Setup(x => x.AddStatusHistoryAsync(It.IsAny<SubscriptionStatusHistory>()))
                .Returns(Task.CompletedTask);

            // Mock usage checking
            var planPrivileges = new List<SubscriptionPlanPrivilege>
            {
                new SubscriptionPlanPrivilege
                {
                    Id = Guid.NewGuid(),
                    Privilege = new Privilege { Name = "Consultations" },
                    Value = 10
                }
            };

            var usages = new List<UserSubscriptionPrivilegeUsage>();

            _mockSubscriptionRepository.Setup(x => x.GetByIdAsync(subscription.Id))
                .ReturnsAsync(subscription);

            _mockPlanPrivilegeRepo.Setup(x => x.GetByPlanIdAsync(subscription.SubscriptionPlanId))
                .ReturnsAsync(planPrivileges);

            _mockUsageRepo.Setup(x => x.GetBySubscriptionIdAsync(subscription.Id))
                .ReturnsAsync(usages);

            // Act - Step 1: Create subscription
            var createResult = await _subscriptionService.CreateSubscriptionAsync(createDto);
            Assert.True(createResult.Success);

            // Act - Step 2: Check privilege usage
            var usageResult = await _subscriptionService.CanUsePrivilegeAsync(subscription.Id.ToString(), "Consultations");
            Assert.True(usageResult.Success);
            Assert.True(usageResult.Data);

            // Act - Step 3: Get usage statistics
            var statsResult = await _subscriptionService.GetUsageStatisticsAsync(subscription.Id.ToString());
            Assert.True(statsResult.Success);
            Assert.Equal(1, statsResult.Data.TotalPrivileges);

            // Assert
            Assert.Equal(Subscription.SubscriptionStatuses.Active, createResult.Data.Status);
        }

        #endregion
    }
}
