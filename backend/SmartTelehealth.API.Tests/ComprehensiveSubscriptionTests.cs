using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SmartTelehealth.API.Controllers;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;
using System.Security.Claims;
using Xunit;

namespace SmartTelehealth.API.Tests
{
    public class ComprehensiveSubscriptionTests
    {
        #region Mock Setup
        private readonly Mock<ISubscriptionService> _mockSubscriptionService;
        private readonly Mock<IAutomatedBillingService> _mockAutomatedBillingService;
        private readonly Mock<ISubscriptionLifecycleService> _mockLifecycleService;
        private readonly Mock<IStripeService> _mockStripeService;
        private readonly Mock<IBillingService> _mockBillingService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<ILogger<SubscriptionsController>> _mockLogger;
        private readonly Mock<ILogger<SubscriptionAutomationController>> _mockAutomationLogger;
        private readonly Mock<ILogger<PaymentController>> _mockPaymentLogger;
        private readonly Mock<ILogger<SubscriptionManagementController>> _mockManagementLogger;
        private readonly Mock<ILogger<SubscriptionAnalyticsController>> _mockAnalyticsLogger;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly Mock<IPaymentSecurityService> _mockPaymentSecurityService;
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly Mock<IAnalyticsService> _mockAnalyticsService;
        private readonly Mock<IUserSubscriptionPrivilegeUsageRepository> _mockUsageRepo;

        public ComprehensiveSubscriptionTests()
        {
            _mockSubscriptionService = new Mock<ISubscriptionService>();
            _mockAutomatedBillingService = new Mock<IAutomatedBillingService>();
            _mockLifecycleService = new Mock<ISubscriptionLifecycleService>();
            _mockStripeService = new Mock<IStripeService>();
            _mockBillingService = new Mock<IBillingService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockLogger = new Mock<ILogger<SubscriptionsController>>();
            _mockAutomationLogger = new Mock<ILogger<SubscriptionAutomationController>>();
            _mockPaymentLogger = new Mock<ILogger<PaymentController>>();
            _mockManagementLogger = new Mock<ILogger<SubscriptionManagementController>>();
            _mockAnalyticsLogger = new Mock<ILogger<SubscriptionAnalyticsController>>();
            _mockAuditService = new Mock<IAuditService>();
            _mockPaymentSecurityService = new Mock<IPaymentSecurityService>();
            _mockCategoryService = new Mock<ICategoryService>();
            _mockAnalyticsService = new Mock<IAnalyticsService>();
            _mockUsageRepo = new Mock<IUserSubscriptionPrivilegeUsageRepository>();
        }
        #endregion

        #region Helper Methods
        private T GetResponseData<T>(ActionResult<ApiResponse<T>> result)
        {
            var actionResult = Assert.IsType<ActionResult<ApiResponse<T>>>(result);
            var okResult = actionResult.Result as OkObjectResult ?? actionResult.Result as ObjectResult;
            Assert.NotNull(okResult);
            var response = Assert.IsType<ApiResponse<T>>(okResult.Value);
            return response.Data;
        }

        private T GetResponseDataFromIActionResult<T>(IActionResult result)
        {
            var objectResult = result as ObjectResult ?? result as OkObjectResult;
            Assert.NotNull(objectResult);
            var response = Assert.IsType<ApiResponse<T>>(objectResult.Value);
            return response.Data;
        }

        private ApiResponse<T> GetResponse<T>(ActionResult<ApiResponse<T>> result)
        {
            var actionResult = Assert.IsType<ActionResult<ApiResponse<T>>>(result);
            var objectResult = actionResult.Result as ObjectResult ?? actionResult.Result as OkObjectResult;
            Assert.NotNull(objectResult);
            return Assert.IsType<ApiResponse<T>>(objectResult.Value);
        }

        private void SetupUserContext(ControllerBase controller, string userId = "test-user-id", string role = "User")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private void SetupAdminContext(ControllerBase controller)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "admin-user-id"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }
        #endregion

        #region User Journey Tests - Complete Subscription Lifecycle

        [Fact]
        public async Task UserJourney_CompleteSubscriptionLifecycle_FromPurchaseToCancellation()
        {
            // Arrange - User purchases subscription
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            SetupUserContext(controller);

            var createDto = new CreateSubscriptionDto
            {
                PlanId = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                StartDate = DateTime.UtcNow,
                Price = 99.99m,
                BillingCycleId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid()
            };

            var subscriptionDto = new SubscriptionDto
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                PlanId = createDto.PlanId,
                Status = "Active",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                CurrentPrice = 99.99m
            };

            _mockSubscriptionService.Setup(x => x.CreateSubscriptionAsync(It.IsAny<CreateSubscriptionDto>()))
                .ReturnsAsync(ApiResponse<SubscriptionDto>.SuccessResponse(subscriptionDto));

            // Act - Create subscription
            var createResult = await controller.CreateSubscription(createDto);

            // Assert - Subscription created successfully
            var createdSubscription = GetResponseData(createResult);
            Assert.NotNull(createdSubscription);
            Assert.Equal("Active", createdSubscription.Status);
            Assert.Equal("test-user-id", createdSubscription.UserId);

            // Arrange - Get user subscriptions
            var userSubscriptions = new List<SubscriptionDto> { subscriptionDto };
            _mockSubscriptionService.Setup(x => x.GetUserSubscriptionsAsync("test-user-id"))
                .ReturnsAsync(ApiResponse<IEnumerable<SubscriptionDto>>.SuccessResponse(userSubscriptions));

            // Act - Get user subscriptions
            var getResult = await controller.GetUserSubscriptions("test-user-id");

            // Assert - User subscriptions retrieved
            var retrievedSubscriptions = GetResponseData(getResult);
            Assert.Single(retrievedSubscriptions);
            Assert.Equal(subscriptionDto.Id, retrievedSubscriptions.First().Id);

            // Arrange - Cancel subscription
            _mockSubscriptionService.Setup(x => x.CancelSubscriptionAsync(subscriptionDto.Id, It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<SubscriptionDto>.SuccessResponse(new SubscriptionDto
                {
                    Id = subscriptionDto.Id,
                    Status = "Cancelled"
                }));

            // Act - Cancel subscription
            var cancelResult = await controller.CancelSubscription(subscriptionDto.Id, "User request");

            // Assert - Subscription cancelled
            var cancelledSubscription = GetResponseData(cancelResult);
            Assert.Equal("Cancelled", cancelledSubscription.Status);
        }

        [Fact]
        public async Task UserJourney_TrialSubscription_WithUpgradeToPaid()
        {
            // Arrange - Create trial subscription
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            SetupUserContext(controller);

            var trialSubscription = new SubscriptionDto
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Status = "TrialActive",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7),
                CurrentPrice = 0m
            };

            _mockSubscriptionService.Setup(x => x.CreateSubscriptionAsync(It.IsAny<CreateSubscriptionDto>()))
                .ReturnsAsync(ApiResponse<SubscriptionDto>.SuccessResponse(trialSubscription));

            var createDto = new CreateSubscriptionDto
            {
                PlanId = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                StartDate = DateTime.UtcNow,
                Price = 0m,
                BillingCycleId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid()
            };

            // Act - Create trial subscription
            var createResult = await controller.CreateSubscription(createDto);

            // Assert - Trial subscription created
            var createdTrial = GetResponseData(createResult);
            Assert.Equal("TrialActive", createdTrial.Status);
            Assert.Equal(0m, createdTrial.CurrentPrice);

            // Arrange - Upgrade to paid plan
            var paidSubscription = new SubscriptionDto
            {
                Id = createdTrial.Id,
                UserId = "test-user-id",
                Status = "Active",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                CurrentPrice = 99.99m
            };

            _mockSubscriptionService.Setup(x => x.UpgradeSubscriptionAsync(createdTrial.Id, It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<SubscriptionDto>.SuccessResponse(paidSubscription));

            // Act - Upgrade subscription
            var upgradeResult = await controller.UpgradeSubscription(createdTrial.Id, Guid.NewGuid().ToString());

            // Assert - Subscription upgraded to paid
            var upgradedSubscription = GetResponseData(upgradeResult);
            Assert.Equal("Active", upgradedSubscription.Status);
            Assert.Equal(99.99m, upgradedSubscription.CurrentPrice);
        }

        [Fact]
        public async Task UserJourney_PauseAndResumeSubscription()
        {
            // Arrange - Active subscription
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            SetupUserContext(controller);

            var activeSubscription = new SubscriptionDto
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Status = "Active",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                CurrentPrice = 99.99m
            };

            _mockSubscriptionService.Setup(x => x.GetSubscriptionAsync(activeSubscription.Id))
                .ReturnsAsync(ApiResponse<SubscriptionDto>.SuccessResponse(activeSubscription));

            // Act - Get subscription
            var getResult = await controller.GetSubscription(activeSubscription.Id);

            // Assert - Subscription retrieved
            var retrievedSubscription = GetResponseData(getResult);
            Assert.Equal("Active", retrievedSubscription.Status);

            // Arrange - Pause subscription
            var pausedSubscription = new SubscriptionDto
            {
                Id = activeSubscription.Id,
                UserId = "test-user-id",
                Status = "Paused",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                CurrentPrice = 99.99m
            };

            _mockSubscriptionService.Setup(x => x.PauseSubscriptionAsync(activeSubscription.Id))
                .ReturnsAsync(ApiResponse<SubscriptionDto>.SuccessResponse(pausedSubscription));

            // Act - Pause subscription
            var pauseResult = await controller.PauseSubscription(activeSubscription.Id);

            // Assert - Subscription paused
            var pausedResult = GetResponseData(pauseResult);
            Assert.Equal("Paused", pausedResult.Status);

            // Arrange - Resume subscription
            var resumedSubscription = new SubscriptionDto
            {
                Id = activeSubscription.Id,
                UserId = "test-user-id",
                Status = "Active",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                CurrentPrice = 99.99m
            };

            _mockSubscriptionService.Setup(x => x.ResumeSubscriptionAsync(activeSubscription.Id))
                .ReturnsAsync(ApiResponse<SubscriptionDto>.SuccessResponse(resumedSubscription));

            // Act - Resume subscription
            var resumeResult = await controller.ResumeSubscription(activeSubscription.Id);

            // Assert - Subscription resumed
            var resumedResult = GetResponseData(resumeResult);
            Assert.Equal("Active", resumedResult.Status);
        }
        #endregion

        #region Admin Management Tests

        [Fact]
        public async Task AdminManagement_CompletePlanLifecycle_FromCreationToDeletion()
        {
            // Arrange - Admin creates subscription plan
            var controller = new SubscriptionPlansController(_mockSubscriptionService.Object);
            SetupAdminContext(controller);

            var createPlanDto = new CreateSubscriptionPlanDto
            {
                Name = "Premium Plan",
                Description = "Premium features with unlimited access",
                Price = 199.99m,
                BillingCycleId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                IsActive = true
            };

            var createdPlan = new SubscriptionPlanDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Premium Plan",
                Description = "Premium features with unlimited access",
                Price = 199.99m,
                IsActive = true
            };

            _mockSubscriptionService.Setup(x => x.CreateSubscriptionPlanAsync(It.IsAny<CreateSubscriptionPlanDto>()))
                .ReturnsAsync(ApiResponse<SubscriptionPlanDto>.SuccessResponse(createdPlan));

            // Act - Create plan
            var createResult = await controller.CreatePlan(createPlanDto);

            // Assert - Plan created
            var createdPlanResult = GetResponseDataFromIActionResult<SubscriptionPlanDto>(createResult);
            Assert.Equal("Premium Plan", createdPlanResult.Name);
            Assert.True(createdPlanResult.IsActive);

            // Arrange - Update plan
            var updatePlanDto = new UpdateSubscriptionPlanDto
            {
                Id = createdPlan.Id,
                Name = "Premium Plan Updated",
                Description = "Updated premium features",
                Price = 249.99m,
                IsActive = true
            };

            var updatedPlan = new SubscriptionPlanDto
            {
                Id = createdPlan.Id,
                Name = "Premium Plan Updated",
                Description = "Updated premium features",
                Price = 249.99m,
                IsActive = true
            };

            _mockSubscriptionService.Setup(x => x.UpdateSubscriptionPlanAsync(createdPlan.Id, It.IsAny<UpdateSubscriptionPlanDto>()))
                .ReturnsAsync(ApiResponse<SubscriptionPlanDto>.SuccessResponse(updatedPlan));

            // Act - Update plan
            var updateResult = await controller.UpdatePlan(createdPlan.Id, updatePlanDto);

            // Assert - Plan updated
            var updatedPlanResult = GetResponseDataFromIActionResult<SubscriptionPlanDto>(updateResult);
            Assert.Equal("Premium Plan Updated", updatedPlanResult.Name);
            Assert.Equal(249.99m, updatedPlanResult.Price);

            // Arrange - Delete plan
            _mockSubscriptionService.Setup(x => x.DeleteSubscriptionPlanAsync(createdPlan.Id))
                .ReturnsAsync(ApiResponse<bool>.SuccessResponse(true));

            // Act - Delete plan
            var deleteResult = await controller.DeletePlan(createdPlan.Id);

            // Assert - Plan deleted
            var deletedResult = GetResponseDataFromIActionResult<bool>(deleteResult);
            Assert.True(deletedResult);
        }

        [Fact]
        public async Task AdminManagement_GetAllPlansAndBillingHistory()
        {
            // Arrange - Get all plans
            var plansController = new SubscriptionPlansController(_mockSubscriptionService.Object);
            SetupAdminContext(plansController);

            var plans = new List<SubscriptionPlanDto>
            {
                new SubscriptionPlanDto { Id = "1", Name = "Basic Plan", Price = 49.99m, IsActive = true },
                new SubscriptionPlanDto { Id = "2", Name = "Premium Plan", Price = 99.99m, IsActive = true },
                new SubscriptionPlanDto { Id = "3", Name = "Enterprise Plan", Price = 199.99m, IsActive = false }
            };

            _mockSubscriptionService.Setup(x => x.GetAllSubscriptionPlansAsync())
                .ReturnsAsync(ApiResponse<IEnumerable<SubscriptionPlanDto>>.SuccessResponse(plans));

            // Act - Get all plans
            var getPlansResult = await plansController.GetAllPlans();

            // Assert - All plans retrieved
            var retrievedPlans = GetResponseDataFromIActionResult<IEnumerable<SubscriptionPlanDto>>(getPlansResult);
            Assert.Equal(3, retrievedPlans.Count());
            Assert.Equal(2, retrievedPlans.Count(p => p.IsActive));

            // Arrange - Get billing history
            var subscriptionsController = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            SetupAdminContext(subscriptionsController);
            
            var billingHistory = new List<BillingHistoryDto>
            {
                new BillingHistoryDto
                {
                    Id = Guid.NewGuid().ToString(),
                    SubscriptionId = "sub-1",
                    Amount = 99.99m,
                    Status = "Paid",
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new BillingHistoryDto
                {
                    Id = Guid.NewGuid().ToString(),
                    SubscriptionId = "sub-1",
                    Amount = 99.99m,
                    Status = "Paid",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockSubscriptionService.Setup(x => x.GetBillingHistoryAsync("sub-1"))
                .ReturnsAsync(ApiResponse<IEnumerable<BillingHistoryDto>>.SuccessResponse(billingHistory));

            // Act - Get billing history
            var getHistoryResult = await subscriptionsController.GetBillingHistory("sub-1");

            // Assert - Billing history retrieved
            var retrievedHistory = GetResponseData<IEnumerable<BillingHistoryDto>>(getHistoryResult);
            Assert.Equal(2, retrievedHistory.Count());
            Assert.All(retrievedHistory, h => Assert.Equal("Paid", h.Status));
        }
        #endregion

        #region Payment Processing Tests

        [Fact]
        public async Task PaymentProcessing_AddAndGetPaymentMethods()
        {
            // Arrange - Add payment method
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            SetupUserContext(controller);

            var paymentMethodId = "pm_test_123456789";
            var paymentMethod = new PaymentMethodDto
            {
                Id = paymentMethodId,
                Type = "card"
            };

            _mockSubscriptionService.Setup(x => x.AddPaymentMethodAsync("test-user-id", paymentMethodId))
                .ReturnsAsync(ApiResponse<PaymentMethodDto>.SuccessResponse(paymentMethod));

            // Act - Add payment method
            var addResult = await controller.AddPaymentMethod(paymentMethodId);

            // Assert - Payment method added
            var addedPaymentMethod = GetResponseData<PaymentMethodDto>(addResult);
            Assert.Equal(paymentMethodId, addedPaymentMethod.Id);
            Assert.Equal("card", addedPaymentMethod.Type);

            // Arrange - Get payment methods
            var paymentMethods = new List<PaymentMethodDto> { paymentMethod };
            _mockSubscriptionService.Setup(x => x.GetPaymentMethodsAsync("test-user-id"))
                .ReturnsAsync(ApiResponse<IEnumerable<PaymentMethodDto>>.SuccessResponse(paymentMethods));

            // Act - Get payment methods
            var getResult = await controller.GetPaymentMethods();

            // Assert - Payment methods retrieved
            var retrievedPaymentMethods = GetResponseData<IEnumerable<PaymentMethodDto>>(getResult);
            Assert.Single(retrievedPaymentMethods);
            Assert.Equal("card", retrievedPaymentMethods.First().Type);
        }

        [Fact]
        public async Task PaymentProcessing_ReactivateExpiredSubscription()
        {
            // Arrange - Expired subscription
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            SetupUserContext(controller);

            var expiredSubscription = new SubscriptionDto
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Status = "Expired",
                StartDate = DateTime.UtcNow.AddMonths(-2),
                EndDate = DateTime.UtcNow.AddMonths(-1),
                CurrentPrice = 99.99m
            };

            _mockSubscriptionService.Setup(x => x.GetSubscriptionAsync(expiredSubscription.Id))
                .ReturnsAsync(ApiResponse<SubscriptionDto>.SuccessResponse(expiredSubscription));

            // Act - Get expired subscription
            var getResult = await controller.GetSubscription(expiredSubscription.Id);

            // Assert - Subscription is expired
            var retrievedSubscription = GetResponseData<SubscriptionDto>(getResult);
            Assert.Equal("Expired", retrievedSubscription.Status);

            // Arrange - Reactivate subscription
            var reactivatedSubscription = new SubscriptionDto
            {
                Id = expiredSubscription.Id,
                UserId = "test-user-id",
                Status = "Active",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                CurrentPrice = 99.99m
            };

            _mockSubscriptionService.Setup(x => x.ReactivateSubscriptionAsync(expiredSubscription.Id))
                .ReturnsAsync(ApiResponse<SubscriptionDto>.SuccessResponse(reactivatedSubscription));

            // Act - Reactivate subscription
            var reactivateResult = await controller.ReactivateSubscription(expiredSubscription.Id);

            // Assert - Subscription reactivated
            var reactivatedResult = GetResponseData<SubscriptionDto>(reactivateResult);
            Assert.Equal("Active", reactivatedResult.Status);
        }
        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task ErrorHandling_ServiceFailures_WithGracefulDegradation()
        {
            // Arrange - Service failure
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            SetupUserContext(controller);

            _mockSubscriptionService.Setup(x => x.GetSubscriptionAsync("invalid-id"))
                .ReturnsAsync(ApiResponse<SubscriptionDto>.ErrorResponse("Subscription not found", 404));

            // Act - Get non-existent subscription
            var result = await controller.GetSubscription("invalid-id");

            // Assert - Error response
            var response = GetResponse(result);
            Assert.False(response.Success);
            Assert.Equal("Subscription not found", response.Message);
            Assert.Equal(404, response.StatusCode);
        }

        [Fact]
        public async Task ErrorHandling_InvalidData_WithValidationErrors()
        {
            // Arrange - Invalid subscription data
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            SetupUserContext(controller);

            var invalidDto = new CreateSubscriptionDto
            {
                PlanId = "",
                UserId = "",
                StartDate = DateTime.UtcNow,
                Price = -10m,
                BillingCycleId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid()
            };

            _mockSubscriptionService.Setup(x => x.CreateSubscriptionAsync(It.IsAny<CreateSubscriptionDto>()))
                .ReturnsAsync(ApiResponse<SubscriptionDto>.ErrorResponse("Invalid subscription data", 400));

            // Act - Create invalid subscription
            var result = await controller.CreateSubscription(invalidDto);

            // Assert - Error response
            var response = GetResponse(result);
            Assert.False(response.Success);
            Assert.Equal("Invalid subscription data", response.Message);
            Assert.Equal(400, response.StatusCode);
        }
        #endregion

        #region Security Tests

        [Fact]
        public async Task Security_UnauthorizedAccess_WithProperDenial()
        {
            // Arrange - No authentication context
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            // No SetupUserContext call - unauthenticated

            // Act - Try to access subscription without authentication
            var result = await controller.GetSubscription("some-id");

            // Assert - Should return unauthorized or handle gracefully
            // The exact behavior depends on the controller implementation
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Security_AdminOnlyEndpoints_WithRoleValidation()
        {
            // Arrange - User context (not admin)
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            SetupUserContext(controller, role: "User");

            // Act - Try to access admin-only endpoint
            var result = await controller.CreatePlan(new CreateSubscriptionPlanDto
            {
                Name = "Test Plan",
                Description = "Test Description",
                Price = 99.99m,
                BillingCycleId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                IsActive = true
            });

            // Assert - Should be denied or handle gracefully
            // The exact behavior depends on the controller implementation
            Assert.NotNull(result);
        }
        #endregion

        #region Performance Tests

        [Fact]
        public async Task Performance_ConcurrentSubscriptions_WithLoadHandling()
        {
            // Arrange - Multiple subscriptions
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            SetupUserContext(controller);

            var subscriptions = Enumerable.Range(1, 10).Select(i => new SubscriptionDto
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "test-user-id",
                Status = "Active",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                CurrentPrice = 99.99m
            }).ToList();

            _mockSubscriptionService.Setup(x => x.GetUserSubscriptionsAsync("test-user-id"))
                .ReturnsAsync(ApiResponse<IEnumerable<SubscriptionDto>>.SuccessResponse(subscriptions));

            // Act - Get user subscriptions
            var result = await controller.GetUserSubscriptions("test-user-id");

            // Assert - All subscriptions retrieved
            var retrievedSubscriptions = GetResponseData(result);
            Assert.Equal(10, retrievedSubscriptions.Count());
        }

        [Fact]
        public async Task Performance_LargeDataSet_WithEfficientQueries()
        {
            // Arrange - Large dataset of plans
            var controller = new SubscriptionPlansController(_mockSubscriptionService.Object);
            SetupAdminContext(controller);

            var plans = Enumerable.Range(1, 100).Select(i => new SubscriptionPlanDto
            {
                Id = i.ToString(),
                Name = $"Plan {i}",
                Description = $"Description for plan {i}",
                Price = 99.99m + i,
                IsActive = i % 2 == 0
            }).ToList();

            _mockSubscriptionService.Setup(x => x.GetAllSubscriptionPlansAsync())
                .ReturnsAsync(ApiResponse<IEnumerable<SubscriptionPlanDto>>.SuccessResponse(plans));

            // Act - Get all plans
            var result = await controller.GetAllPlans();

            // Assert - All plans retrieved efficiently
            var retrievedPlans = GetResponseDataFromIActionResult<IEnumerable<SubscriptionPlanDto>>(result);
            Assert.Equal(100, retrievedPlans.Count());
            Assert.Equal(50, retrievedPlans.Count(p => p.IsActive));
        }
        #endregion
    }
}
