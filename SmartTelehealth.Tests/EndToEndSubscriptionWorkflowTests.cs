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
using SmartTelehealth.Core.Interfaces;
using Xunit;
using FluentAssertions;

namespace SmartTelehealth.Tests
{
    public class EndToEndSubscriptionWorkflowTests : IDisposable
    {
        #region Test Data Setup
        private User _testUser;
        private User _adminUser;
        private SubscriptionPlan _basicPlan;
        private SubscriptionPlan _premiumPlan;
        private MasterBillingCycle _monthlyBillingCycle;
        private MasterBillingCycle _annualBillingCycle;
        private MasterCurrency _usdCurrency;
        private Privilege _consultationPrivilege;
        private Privilege _videoCallPrivilege;
        private SubscriptionPlanPrivilege _basicPlanConsultationPrivilege;
        private SubscriptionPlanPrivilege _basicPlanVideoCallPrivilege;
        #endregion

        #region Mock Services
        private Mock<ISubscriptionService> _mockSubscriptionService;
        private Mock<IBillingService> _mockBillingService;
        private Mock<IStripeService> _mockStripeService;
        private Mock<IPrivilegeService> _mockPrivilegeService;
        private Mock<IAnalyticsService> _mockAnalyticsService;
        private Mock<INotificationService> _mockNotificationService;
        private Mock<IAuditService> _mockAuditService;
        private Mock<IUserService> _mockUserService;
        private Mock<ICategoryService> _mockCategoryService;
        private Mock<IPdfService> _mockPdfService;
        private Mock<IPaymentSecurityService> _mockPaymentSecurityService;
        private Mock<ISubscriptionRepository> _mockSubscriptionRepository;
        private Mock<ISubscriptionPlanRepository> _mockPlanRepository;
        private Mock<IPrivilegeRepository> _mockPrivilegeRepository;

        private Mock<IUserSubscriptionPrivilegeUsageRepository> _mockUsageRepository;
        #endregion

        #region Controllers
        private SubscriptionsController _subscriptionsController;
        private SubscriptionManagementController _subscriptionManagementController;
        private PaymentController _paymentController;
        private BillingController _billingController;

        #endregion

        public EndToEndSubscriptionWorkflowTests()
        {
            InitializeTestData();
            InitializeMocks();
            InitializeControllers();
            SetupControllerContext();
        }

        #region Test Data Initialization
        private void InitializeTestData()
        {
            // Test Users
            _testUser = new User
            {
                Id = 1,
                Email = "testuser@example.com",
                FirstName = "Test",
                LastName = "User",
                UserRoleId = 2, // Regular user role
                IsActive = true
            };

            _adminUser = new User
            {
                Id = 2,
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                UserRoleId = 1, // Admin role
                IsActive = true
            };

            // Billing Cycles
            _monthlyBillingCycle = new MasterBillingCycle
            {
                Id = Guid.NewGuid(),
                Name = "Monthly",
                DurationInDays = 30,
                IsActive = true
            };

            _annualBillingCycle = new MasterBillingCycle
            {
                Id = Guid.NewGuid(),
                Name = "Annual",
                DurationInDays = 365,
                IsActive = true
            };

            // Currency
            _usdCurrency = new MasterCurrency
            {
                Id = Guid.NewGuid(),
                Code = "USD",
                Name = "US Dollar",
                Symbol = "$",
                IsActive = true
            };

            // Privileges
            _consultationPrivilege = new Privilege
            {
                Id = Guid.NewGuid(),
                Name = "Teleconsultation",
                Description = "Video consultation with healthcare providers",
                IsActive = true
            };

            _videoCallPrivilege = new Privilege
            {
                Id = Guid.NewGuid(),
                Name = "VideoCall",
                Description = "Video call functionality",
                IsActive = true
            };

            // Subscription Plans
            _basicPlan = new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Basic Plan",
                Description = "Basic healthcare plan with limited features",
                Price = 29.99m,
                BillingCycleId = _monthlyBillingCycle.Id,
                CurrencyId = _usdCurrency.Id,
                IsActive = true,
                IsTrialAllowed = true,
                TrialDurationInDays = 7
            };

            _premiumPlan = new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Premium Plan",
                Description = "Premium healthcare plan with unlimited features",
                Price = 79.99m,
                BillingCycleId = _monthlyBillingCycle.Id,
                CurrencyId = _usdCurrency.Id,
                IsActive = true,
                IsTrialAllowed = true,
                TrialDurationInDays = 14
            };

            // Plan Privileges
            _basicPlanConsultationPrivilege = new SubscriptionPlanPrivilege
            {
                Id = Guid.NewGuid(),
                SubscriptionPlanId = _basicPlan.Id,
                PrivilegeId = _consultationPrivilege.Id,
                Value = 5, // 5 consultations per month
                UsagePeriodId = _monthlyBillingCycle.Id,
                DurationMonths = 1,
                IsActive = true
            };

            _basicPlanVideoCallPrivilege = new SubscriptionPlanPrivilege
            {
                Id = Guid.NewGuid(),
                SubscriptionPlanId = _basicPlan.Id,
                PrivilegeId = _videoCallPrivilege.Id,
                Value = 10, // 10 video calls per month
                UsagePeriodId = _monthlyBillingCycle.Id,
                DurationMonths = 1,
                IsActive = true
            };
        }
        #endregion

        #region Mock Initialization
        private void InitializeMocks()
        {
            _mockSubscriptionService = new Mock<ISubscriptionService>();
            _mockBillingService = new Mock<IBillingService>();
            _mockStripeService = new Mock<IStripeService>();
            _mockPrivilegeService = new Mock<IPrivilegeService>();
            _mockAnalyticsService = new Mock<IAnalyticsService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockAuditService = new Mock<IAuditService>();
            _mockUserService = new Mock<IUserService>();
            _mockCategoryService = new Mock<ICategoryService>();
            _mockPdfService = new Mock<IPdfService>();
            _mockPaymentSecurityService = new Mock<IPaymentSecurityService>();
            _mockSubscriptionRepository = new Mock<ISubscriptionRepository>();
            _mockPlanRepository = new Mock<ISubscriptionPlanRepository>();
            _mockPrivilegeRepository = new Mock<IPrivilegeRepository>();

            _mockUsageRepository = new Mock<IUserSubscriptionPrivilegeUsageRepository>();

            SetupCommonMocks();
        }

        private void SetupCommonMocks()
        {
            // Setup common service mocks
            _mockSubscriptionService.Setup(x => x.GetAllPlansAsync(It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = new List<SubscriptionPlanDto> { new SubscriptionPlanDto() }, Message = "Success", StatusCode = 200 });

            _mockAnalyticsService.Setup(x => x.GetRevenueAnalyticsAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = new RevenueAnalyticsDto(), Message = "Success", StatusCode = 200 });

            _mockBillingService.Setup(x => x.GetBillingAnalyticsAsync(It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = new BillingAnalyticsDto(), Message = "Success", StatusCode = 200 });
        }
        #endregion

        #region Controller Initialization
        private void InitializeControllers()
        {
            _subscriptionsController = new SubscriptionsController(_mockSubscriptionService.Object);
            _subscriptionManagementController = new SubscriptionManagementController(
                _mockSubscriptionService.Object,
                _mockCategoryService.Object,
                _mockAnalyticsService.Object,
                _mockAuditService.Object);
            _paymentController = new PaymentController(
                _mockStripeService.Object,
                _mockBillingService.Object,
                _mockSubscriptionService.Object,
                _mockAuditService.Object,
                _mockPaymentSecurityService.Object);
            _billingController = new BillingController(
                _mockBillingService.Object,
                _mockPdfService.Object,
                _mockUserService.Object,
                _mockSubscriptionService.Object);

        }
        #endregion

        #region Controller Context Setup
        private void SetupControllerContext()
        {
            SetupControllerContext(_subscriptionsController, _testUser);
            SetupControllerContext(_subscriptionManagementController, _adminUser);
            SetupControllerContext(_paymentController, _testUser);
            SetupControllerContext(_billingController, _adminUser);

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

        #region End-to-End User Journey Tests

        [Fact]
        public async Task Test_Complete_User_Subscription_Journey()
        {
            // Step 1: User browses available plans
            var plans = new List<SubscriptionPlanDto>
            {
                new SubscriptionPlanDto
                {
                    Id = _basicPlan.Id.ToString(),
                    Name = _basicPlan.Name,
                    Description = _basicPlan.Description,
                    Price = _basicPlan.Price,
                    IsActive = _basicPlan.IsActive,
                    IsTrialAllowed = _basicPlan.IsTrialAllowed,
                    TrialDurationInDays = _basicPlan.TrialDurationInDays
                },
                new SubscriptionPlanDto
                {
                    Id = _premiumPlan.Id.ToString(),
                    Name = _premiumPlan.Name,
                    Description = _premiumPlan.Description,
                    Price = _premiumPlan.Price,
                    IsActive = _premiumPlan.IsActive,
                    IsTrialAllowed = _premiumPlan.IsTrialAllowed,
                    TrialDurationInDays = _premiumPlan.TrialDurationInDays
                }
            };

            _mockSubscriptionService.Setup(x => x.GetAllPlansAsync(It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = plans, Message = "Plans retrieved successfully", StatusCode = 200 });

            var browseResult = await _subscriptionsController.GetAllPlans();
            browseResult.StatusCode.Should().Be(200);
            var retrievedPlans = browseResult.data as List<SubscriptionPlanDto>;
            retrievedPlans.Should().HaveCount(2);

            // Step 2: User selects and purchases a plan
            var createDto = new CreateSubscriptionDto
            {
                UserId = _testUser.Id,
                PlanId = _basicPlan.Id.ToString(),
                BillingCycleId = _monthlyBillingCycle.Id,
                CurrencyId = _usdCurrency.Id,
                StartImmediately = true,
                AutoRenew = true
            };

            var createdSubscription = new SubscriptionDto
            {
                Id = Guid.NewGuid().ToString(),
                UserId = _testUser.Id.ToString(),
                PlanId = _basicPlan.Id.ToString(),
                PlanName = _basicPlan.Name,
                Status = Subscription.SubscriptionStatuses.TrialActive,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7), // Trial period
                NextBillingDate = DateTime.UtcNow.AddDays(7),
                CurrentPrice = _basicPlan.Price,
                IsActive = true,
                IsInTrial = true,
                TrialStartDate = DateTime.UtcNow,
                TrialEndDate = DateTime.UtcNow.AddDays(7)
            };

            _mockSubscriptionService.Setup(x => x.CreateSubscriptionAsync(It.IsAny<CreateSubscriptionDto>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = createdSubscription, Message = "Subscription created successfully", StatusCode = 201 });

            var purchaseResult = await _subscriptionsController.CreateSubscription(createDto);
            purchaseResult.StatusCode.Should().Be(201);
            purchaseResult.data.Should().NotBeNull();

            // Step 3: User starts trial period
            var subscription = purchaseResult.data as SubscriptionDto;
            subscription.Status.Should().Be(Subscription.SubscriptionStatuses.TrialActive);
            subscription.IsInTrial.Should().BeTrue();
            subscription.TrialStartDate.Should().NotBeNull();
            subscription.TrialEndDate.Should().NotBeNull();

            // Step 4: User uses privileges during trial
            _mockPrivilegeService.Setup(x => x.GetRemainingPrivilegeAsync(It.IsAny<Guid>(), "Teleconsultation", It.IsAny<TokenModel>()))
                .ReturnsAsync(5); // 5 consultations remaining

            _mockPrivilegeService.Setup(x => x.UsePrivilegeAsync(It.IsAny<Guid>(), "Teleconsultation", 1, It.IsAny<TokenModel>()))
                .ReturnsAsync(true);

            // User books a consultation
            var consultationResult = await _mockPrivilegeService.Object.UsePrivilegeAsync(
                Guid.Parse(subscription.Id), "Teleconsultation", 1, null);
            consultationResult.Should().BeTrue();

            // Step 5: Trial expires, subscription becomes active
            var activeSubscription = new SubscriptionDto
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                PlanId = subscription.PlanId,
                PlanName = subscription.PlanName,
                Status = Subscription.SubscriptionStatuses.Active,
                StartDate = DateTime.UtcNow.AddDays(7),
                EndDate = DateTime.UtcNow.AddMonths(1),
                NextBillingDate = DateTime.UtcNow.AddMonths(1),
                CurrentPrice = _basicPlan.Price,
                IsActive = true,
                IsInTrial = false
            };

            _mockSubscriptionService.Setup(x => x.GetSubscriptionAsync(subscription.Id, It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = activeSubscription, Message = "Subscription retrieved successfully", StatusCode = 200 });

            var activeResult = await _subscriptionsController.GetSubscription(subscription.Id);
            activeResult.StatusCode.Should().Be(200);
            var activeSub = activeResult.data as SubscriptionDto;
            activeSub.Status.Should().Be(Subscription.SubscriptionStatuses.Active);
            activeSub.IsInTrial.Should().BeFalse();

            // Step 6: User continues using privileges
            _mockPrivilegeService.Setup(x => x.GetRemainingPrivilegeAsync(It.IsAny<Guid>(), "Teleconsultation", It.IsAny<TokenModel>()))
                .ReturnsAsync(4); // 4 consultations remaining after using 1

            var remainingConsultations = await _mockPrivilegeService.Object.GetRemainingPrivilegeAsync(
                Guid.Parse(subscription.Id), "Teleconsultation", null);
            remainingConsultations.Should().Be(4);

            // Step 7: User reaches privilege limit
            _mockPrivilegeService.Setup(x => x.GetRemainingPrivilegeAsync(It.IsAny<Guid>(), "Teleconsultation", It.IsAny<TokenModel>()))
                .ReturnsAsync(0); // No consultations remaining

            _mockPrivilegeService.Setup(x => x.UsePrivilegeAsync(It.IsAny<Guid>(), "Teleconsultation", 1, It.IsAny<TokenModel>()))
                .ReturnsAsync(false); // Cannot use privilege

            var limitReachedResult = await _mockPrivilegeService.Object.UsePrivilegeAsync(
                Guid.Parse(subscription.Id), "Teleconsultation", 1, null);
            limitReachedResult.Should().BeFalse();

            // Step 8: User manages subscription (pause, resume, cancel)
            _mockSubscriptionService.Setup(x => x.PauseSubscriptionAsync(subscription.Id, It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = new object(), Message = "Subscription paused successfully", StatusCode = 200 });

            var pauseResult = await _subscriptionsController.PauseSubscription(subscription.Id);
            pauseResult.StatusCode.Should().Be(200);
            pauseResult.Message.Should().Be("Subscription paused successfully");

            _mockSubscriptionService.Setup(x => x.ResumeSubscriptionAsync(subscription.Id, It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = new object(), Message = "Subscription resumed successfully", StatusCode = 200 });

            var resumeResult = await _subscriptionsController.ResumeSubscription(subscription.Id);
            resumeResult.StatusCode.Should().Be(200);
            resumeResult.Message.Should().Be("Subscription resumed successfully");

            _mockSubscriptionService.Setup(x => x.CancelSubscriptionAsync(subscription.Id, "User request", It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = new object(), Message = "Subscription cancelled successfully", StatusCode = 200 });

            var cancelResult = await _subscriptionsController.CancelSubscription(subscription.Id, "User request");
            cancelResult.StatusCode.Should().Be(200);
            cancelResult.Message.Should().Be("Subscription cancelled successfully");
        }

        [Fact]
        public async Task Test_Trial_Period_Functionality()
        {
            // Test trial period creation
            var trialPlan = new SubscriptionPlanDto
            {
                Id = _basicPlan.Id.ToString(),
                Name = _basicPlan.Name,
                IsTrialAllowed = true,
                TrialDurationInDays = 7
            };

            var trialSubscription = new SubscriptionDto
            {
                Id = Guid.NewGuid().ToString(),
                Status = Subscription.SubscriptionStatuses.TrialActive,
                IsInTrial = true,
                TrialStartDate = DateTime.UtcNow,
                TrialEndDate = DateTime.UtcNow.AddDays(7),
                TrialDurationInDays = 7
            };

            _mockSubscriptionService.Setup(x => x.CreateSubscriptionAsync(It.IsAny<CreateSubscriptionDto>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = trialSubscription, Message = "Trial subscription created", StatusCode = 201 });

            var createDto = new CreateSubscriptionDto
            {
                UserId = _testUser.Id,
                PlanId = _basicPlan.Id.ToString(),
                StartImmediately = true
            };

            var result = await _subscriptionsController.CreateSubscription(createDto);
            result.StatusCode.Should().Be(201);
            var subscription = result.data as SubscriptionDto;
            subscription.IsInTrial.Should().BeTrue();
            subscription.TrialStartDate.Should().NotBeNull();
            subscription.TrialEndDate.Should().NotBeNull();
            subscription.TrialDurationInDays.Should().Be(7);

            // Test trial expiration
            var expiredSubscription = new SubscriptionDto
            {
                Id = subscription.Id,
                Status = Subscription.SubscriptionStatuses.TrialExpired,
                IsInTrial = false,
                TrialEndDate = DateTime.UtcNow.AddDays(-1) // Trial expired yesterday
            };

            _mockSubscriptionService.Setup(x => x.GetSubscriptionAsync(subscription.Id, It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = expiredSubscription, Message = "Subscription retrieved", StatusCode = 200 });

            var expiredResult = await _subscriptionsController.GetSubscription(subscription.Id);
            var expiredSub = expiredResult.data as SubscriptionDto;
            expiredSub.Status.Should().Be(Subscription.SubscriptionStatuses.TrialExpired);
            expiredSub.IsInTrial.Should().BeFalse();
        }

        [Fact]
        public async Task Test_Privilege_Usage_Tracking()
        {
            var subscriptionId = Guid.NewGuid();
            var privilegeName = "Teleconsultation";

            // Test privilege usage tracking
            _mockPrivilegeService.Setup(x => x.GetRemainingPrivilegeAsync(subscriptionId, privilegeName, It.IsAny<TokenModel>()))
                .ReturnsAsync(5);

            var initialRemaining = await _mockPrivilegeService.Object.GetRemainingPrivilegeAsync(
                subscriptionId, privilegeName, null);
            initialRemaining.Should().Be(5);

            // Use privilege
            _mockPrivilegeService.Setup(x => x.UsePrivilegeAsync(subscriptionId, privilegeName, 1, It.IsAny<TokenModel>()))
                .ReturnsAsync(true);

            var useResult = await _mockPrivilegeService.Object.UsePrivilegeAsync(
                subscriptionId, privilegeName, 1, null);
            useResult.Should().BeTrue();

            // Check remaining after usage
            _mockPrivilegeService.Setup(x => x.GetRemainingPrivilegeAsync(subscriptionId, privilegeName, It.IsAny<TokenModel>()))
                .ReturnsAsync(4);

            var remainingAfterUsage = await _mockPrivilegeService.Object.GetRemainingPrivilegeAsync(
                subscriptionId, privilegeName, null);
            remainingAfterUsage.Should().Be(4);

            // Test privilege limit reached
            _mockPrivilegeService.Setup(x => x.GetRemainingPrivilegeAsync(subscriptionId, privilegeName, It.IsAny<TokenModel>()))
                .ReturnsAsync(0);

            _mockPrivilegeService.Setup(x => x.UsePrivilegeAsync(subscriptionId, privilegeName, 1, It.IsAny<TokenModel>()))
                .ReturnsAsync(false);

            var limitReachedResult = await _mockPrivilegeService.Object.UsePrivilegeAsync(
                subscriptionId, privilegeName, 1, null);
            limitReachedResult.Should().BeFalse();
        }

        [Fact]
        public async Task Test_Payment_Processing_And_Billing()
        {
            var subscriptionId = Guid.NewGuid().ToString();
            var amount = 29.99m;

            // Test payment processing
            var paymentRequest = new PaymentRequestDto
            {
                PaymentMethodId = "pm_test_123",
                Amount = amount,
                Currency = "usd"
            };

            var paymentResult = new PaymentResultDto
            {
                Status = "succeeded",
                PaymentIntentId = "pi_test_123",
                Amount = amount,
                Currency = "usd",
                ProcessedAt = DateTime.UtcNow
            };

            _mockSubscriptionService.Setup(x => x.ProcessPaymentAsync(subscriptionId, paymentRequest, It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = paymentResult, Message = "Payment processed successfully", StatusCode = 200 });

            var processResult = await _subscriptionsController.ProcessPayment(subscriptionId, paymentRequest);
            processResult.StatusCode.Should().Be(200);
            var result = processResult.data as PaymentResultDto;
            result.Status.Should().Be("succeeded");
            result.Amount.Should().Be(amount);

            // Test billing record creation
            var billingRecord = new BillingRecordDto
            {
                Id = Guid.NewGuid().ToString(),
                UserId = _testUser.Id.ToString(),
                SubscriptionId = subscriptionId,
                Amount = amount,
                Status = BillingRecord.BillingStatus.Paid.ToString(),
                BillingDate = DateTime.UtcNow,
                PaidAt = DateTime.UtcNow
            };

            _mockBillingService.Setup(x => x.GetBillingRecordAsync(It.IsAny<Guid>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = billingRecord, Message = "Billing record retrieved", StatusCode = 200 });

            var billingResult = await _billingController.GetBillingRecord(Guid.NewGuid());
            billingResult.StatusCode.Should().Be(200);
            var billing = billingResult.data as BillingRecordDto;
            billing.Status.Should().Be(BillingRecord.BillingStatus.Paid.ToString());
            billing.Amount.Should().Be(amount);
        }

        [Fact]
        public async Task Test_Stripe_Integration_And_Webhooks()
        {
            var customerId = "cus_test_123";
            var subscriptionId = "sub_test_123";

            // Test Stripe customer creation
            _mockStripeService.Setup(x => x.CreateCustomerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(customerId);

            var customerResult = await _mockStripeService.Object.CreateCustomerAsync(
                _testUser.Email, $"{_testUser.FirstName} {_testUser.LastName}", new TokenModel());
            customerResult.Should().Be(customerId);

            // Test Stripe subscription creation
            _mockStripeService.Setup(x => x.CreateSubscriptionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(subscriptionId);

            var stripeSubResult = await _mockStripeService.Object.CreateSubscriptionAsync(
                customerId, "price_test_123", "stripe", new TokenModel());
            stripeSubResult.Should().Be(subscriptionId);

            // Test webhook processing
            var webhookEvent = new { Type = "customer.subscription.created" };
            _mockSubscriptionService.Setup(x => x.HandlePaymentProviderWebhookAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = new object(), Message = "Webhook processed", StatusCode = 200 });

            var webhookResult = await _mockSubscriptionService.Object.HandlePaymentProviderWebhookAsync(
                "customer.subscription.created", subscriptionId, null, null);
            webhookResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Test_Admin_Subscription_Management()
        {
            // Test admin getting all subscriptions
            var adminSubscriptions = new List<SubscriptionDto>
            {
                new SubscriptionDto { Id = "1", UserId = "1", Status = "Active" },
                new SubscriptionDto { Id = "2", UserId = "2", Status = "TrialActive" }
            };

            _mockSubscriptionService.Setup(x => x.GetAllUserSubscriptionsAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = adminSubscriptions, Message = "Subscriptions retrieved", StatusCode = 200 });

            var adminResult = await _subscriptionManagementController.GetAllUserSubscriptions();
            adminResult.StatusCode.Should().Be(200);
            var subs = adminResult.data as List<SubscriptionDto>;
            subs.Should().HaveCount(2);

            // Test admin canceling user subscription
            _mockSubscriptionService.Setup(x => x.CancelUserSubscriptionAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = new object(), Message = "Subscription cancelled by admin", StatusCode = 200 });

            var cancelResult = await _subscriptionManagementController.CancelUserSubscription("1", "Admin request");
            cancelResult.StatusCode.Should().Be(200);
            cancelResult.Message.Should().Be("Subscription cancelled by admin");

            // Test admin pausing user subscription
            _mockSubscriptionService.Setup(x => x.PauseUserSubscriptionAsync(
                It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = new object(), Message = "Subscription paused by admin", StatusCode = 200 });

            var pauseResult = await _subscriptionManagementController.PauseUserSubscription("2");
            pauseResult.StatusCode.Should().Be(200);
            pauseResult.Message.Should().Be("Subscription paused by admin");

            // Test admin resuming user subscription
            _mockSubscriptionService.Setup(x => x.ResumeUserSubscriptionAsync(
                It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = new object(), Message = "Subscription resumed by admin", StatusCode = 200 });

            var resumeResult = await _subscriptionManagementController.ResumeUserSubscription("2");
            resumeResult.StatusCode.Should().Be(200);
            resumeResult.Message.Should().Be("Subscription resumed by admin");

            // Test admin extending user subscription
            var extendDto = new ExtendSubscriptionDto
            {
                NewEndDate = DateTime.UtcNow.AddMonths(2),
                Reason = "Admin extension"
            };

            _mockSubscriptionService.Setup(x => x.ExtendUserSubscriptionAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = new object(), Message = "Subscription extended by admin", StatusCode = 200 });

            var extendResult = await _subscriptionManagementController.ExtendUserSubscription("1", extendDto);
            extendResult.StatusCode.Should().Be(200);
            extendResult.Message.Should().Be("Subscription extended by admin");
        }

        [Fact]
        public async Task Test_Subscription_Plan_Management()
        {
            // Test creating new subscription plan
            var createPlanDto = new CreateSubscriptionPlanDto
            {
                Name = "Enterprise Plan",
                Description = "Enterprise healthcare plan",
                Price = 199.99m,
                BillingCycleId = _annualBillingCycle.Id,
                CurrencyId = _usdCurrency.Id,
                IsActive = true
            };

            var createdPlan = new SubscriptionPlanDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = createPlanDto.Name,
                Description = createPlanDto.Description,
                Price = createPlanDto.Price,
                IsActive = createPlanDto.IsActive
            };

            _mockSubscriptionService.Setup(x => x.CreatePlanAsync(createPlanDto, It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = createdPlan, Message = "Plan created successfully", StatusCode = 201 });

            var createResult = await _subscriptionManagementController.CreatePlan(createPlanDto);
            createResult.StatusCode.Should().Be(201);
            var plan = createResult.data as SubscriptionPlanDto;
            plan.Name.Should().Be("Enterprise Plan");
            plan.Price.Should().Be(199.99m);

            // Test updating subscription plan
            var updatePlanDto = new UpdateSubscriptionPlanDto
            {
                Id = plan.Id,
                Name = "Enterprise Plan Updated",
                Description = "Updated enterprise plan",
                Price = 249.99m
            };

            var updatedPlan = new SubscriptionPlanDto
            {
                Id = plan.Id,
                Name = updatePlanDto.Name,
                Description = updatePlanDto.Description,
                Price = updatePlanDto.Price
            };

            _mockSubscriptionService.Setup(x => x.UpdatePlanAsync(plan.Id, updatePlanDto, It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = updatedPlan, Message = "Plan updated successfully", StatusCode = 200 });

            var updateResult = await _subscriptionManagementController.UpdatePlan(plan.Id, updatePlanDto);
            updateResult.StatusCode.Should().Be(200);
            var updated = updateResult.data as SubscriptionPlanDto;
            updated.Name.Should().Be("Enterprise Plan Updated");
            updated.Price.Should().Be(249.99m);

            // Test activating/deactivating plan
            _mockSubscriptionService.Setup(x => x.ActivatePlanAsync(plan.Id, It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = new object(), Message = "Plan activated successfully", StatusCode = 200 });

            var activateResult = await _subscriptionManagementController.ActivatePlan(plan.Id);
            activateResult.StatusCode.Should().Be(200);
            activateResult.Message.Should().Be("Plan activated successfully");

            _mockSubscriptionService.Setup(x => x.DeactivatePlanAsync(plan.Id, It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = new object(), Message = "Plan deactivated successfully", StatusCode = 200 });

            var deactivateResult = await _subscriptionManagementController.DeactivatePlan(plan.Id);
            deactivateResult.StatusCode.Should().Be(200);
            deactivateResult.Message.Should().Be("Plan deactivated successfully");
        }

        [Fact]
        public async Task Test_Usage_Statistics_And_Analytics()
        {
            var subscriptionId = Guid.NewGuid().ToString();

            // Test usage statistics
            var usageStats = new UsageStatisticsDto
            {
                SubscriptionId = subscriptionId,
                PlanName = "Basic Plan",
                CurrentPeriodStart = DateTime.UtcNow.AddDays(-30),
                CurrentPeriodEnd = DateTime.UtcNow,
                TotalPrivileges = 2,
                UsedPrivileges = 2,
                PrivilegeUsage = new List<PrivilegeUsageDto>
                {
                    new PrivilegeUsageDto
                    {
                        PrivilegeName = "Teleconsultation",
                        UsedValue = 3,
                        AllowedValue = 5,
                        RemainingValue = 2,
                        UsagePercentage = 60.0m
                    },
                    new PrivilegeUsageDto
                    {
                        PrivilegeName = "VideoCall",
                        UsedValue = 7,
                        AllowedValue = 10,
                        RemainingValue = 3,
                        UsagePercentage = 70.0m
                    }
                }
            };

            _mockSubscriptionService.Setup(x => x.GetUsageStatisticsAsync(subscriptionId, It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = usageStats, Message = "Usage statistics retrieved", StatusCode = 200 });

            var statsResult = await _subscriptionsController.GetUsageStatistics(subscriptionId);
            statsResult.StatusCode.Should().Be(200);
            var stats = statsResult.data as UsageStatisticsDto;
            stats.TotalPrivileges.Should().Be(2);
            stats.UsedPrivileges.Should().Be(2);
            stats.PrivilegeUsage.Should().HaveCount(2);

            // Test analytics
            var analytics = new SubscriptionAnalyticsDto
            {
                TotalSubscriptions = 100,
                ActiveSubscriptions = 85,
                TrialSubscriptions = 15,
                MonthlyRecurringRevenue = 5000.00m,
                ChurnRate = 0.05m,
                AverageSubscriptionValue = 58.82m
            };

            _mockAnalyticsService.Setup(x => x.GetSubscriptionAnalyticsAsync(
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(new JsonModel { data = analytics, Message = "Analytics retrieved", StatusCode = 200 });

            var analyticsResult = await _subscriptionManagementController.GetAnalytics();
            analyticsResult.StatusCode.Should().Be(200);
            var analyticsData = analyticsResult.data as SubscriptionAnalyticsDto;
            analyticsData.TotalSubscriptions.Should().Be(100);
            analyticsData.ActiveSubscriptions.Should().Be(85);
            analyticsData.MonthlyRecurringRevenue.Should().Be(5000.00m);
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
        #endregion
    }

    #region DTO Classes for Testing
    public class SubscriptionAnalyticsDto
    {
        public int TotalSubscriptions { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int TrialSubscriptions { get; set; }
        public decimal MonthlyRecurringRevenue { get; set; }
        public decimal ChurnRate { get; set; }
        public decimal AverageSubscriptionValue { get; set; }
    }
    #endregion
}
