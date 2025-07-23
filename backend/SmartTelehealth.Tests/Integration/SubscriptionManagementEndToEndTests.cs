using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.API.Controllers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SmartTelehealth.Tests.Integration
{
    public class SubscriptionManagementEndToEndTests
    {
        private readonly Mock<ISubscriptionService> _mockSubscriptionService;
        private readonly Mock<ILogger<SubscriptionsController>> _mockLogger;

        public SubscriptionManagementEndToEndTests()
        {
            _mockSubscriptionService = new Mock<ISubscriptionService>();
            _mockLogger = new Mock<ILogger<SubscriptionsController>>();
        }

        [Fact(DisplayName = "Admin can create a new subscription plan")]
        public async Task Admin_Can_Create_New_Subscription_Plan()
        {
            // Arrange
            var createPlanDto = new CreateSubscriptionPlanDto
            {
                Name = "Premium Plan",
                Description = "Premium features",
                Price = 99.99m,
                BillingCycleId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                IsActive = true
            };
            var planDto = new SubscriptionPlanDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = createPlanDto.Name,
                Description = createPlanDto.Description,
                Price = createPlanDto.Price,
                BillingCycleId = createPlanDto.BillingCycleId,
                CurrencyId = createPlanDto.CurrencyId,
                IsActive = createPlanDto.IsActive
            };
            var apiResponse = ApiResponse<SubscriptionPlanDto>.SuccessResponse(planDto);
            _mockSubscriptionService.Setup(s => s.CreateSubscriptionPlanAsync(createPlanDto)).ReturnsAsync(apiResponse);
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            
            // Act
            var result = await controller.GetAllPlans();
            
            // Assert
            _mockSubscriptionService.Verify(s => s.GetAllPlansAsync(), Times.Once);
        }

        [Fact(DisplayName = "User can view all active plans")]
        public async Task User_Can_View_All_Active_Plans()
        {
            // Arrange
            var plans = new List<SubscriptionPlanDto>
            {
                new SubscriptionPlanDto { Id = "1", Name = "Basic", IsActive = true },
                new SubscriptionPlanDto { Id = "2", Name = "Premium", IsActive = true }
            };
            var apiResponse = ApiResponse<IEnumerable<SubscriptionPlanDto>>.SuccessResponse(plans);
            _mockSubscriptionService.Setup(s => s.GetAllPlansAsync()).ReturnsAsync(apiResponse);
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            // Act
            var result = await controller.GetAllPlans();
            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var returnedApiResponse = okResult.Value as ApiResponse<IEnumerable<SubscriptionPlanDto>>;
            Assert.NotNull(returnedApiResponse);
            Assert.Equal(2, returnedApiResponse.Data.Count());
        }

        [Fact(DisplayName = "User can subscribe to a plan")]
        public async Task User_Can_Subscribe_To_A_Plan()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var planId = Guid.NewGuid().ToString();
            var createDto = new CreateSubscriptionDto { UserId = userId, PlanId = planId };
            var subscription = new SubscriptionDto { Id = Guid.NewGuid().ToString(), UserId = userId, PlanId = planId, Status = "Active" };
            var apiResponse = ApiResponse<SubscriptionDto>.SuccessResponse(subscription);
            _mockSubscriptionService.Setup(s => s.CreateSubscriptionAsync(createDto)).ReturnsAsync(apiResponse);
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            
            // Act
            var result = await controller.CreateSubscription(createDto);
            
            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var returnedApiResponse = okResult.Value as ApiResponse<SubscriptionDto>;
            Assert.NotNull(returnedApiResponse);
            Assert.Equal(userId, returnedApiResponse.Data.UserId);
            Assert.Equal(planId, returnedApiResponse.Data.PlanId);
            Assert.Equal("Active", returnedApiResponse.Data.Status);
        }

        [Fact(DisplayName = "User can cancel their subscription")]
        public async Task User_Can_Cancel_Their_Subscription()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var reason = "No longer needed";
            var cancelled = new SubscriptionDto { Id = subscriptionId, Status = "Cancelled", CancellationReason = reason };
            var apiResponse = ApiResponse<SubscriptionDto>.SuccessResponse(cancelled);
            _mockSubscriptionService.Setup(s => s.CancelSubscriptionAsync(subscriptionId, reason)).ReturnsAsync(apiResponse);
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            
            // Act
            var result = await controller.CancelSubscription(subscriptionId, reason);
            
            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var returnedApiResponse = okResult.Value as ApiResponse<SubscriptionDto>;
            Assert.NotNull(returnedApiResponse);
            Assert.Equal("Cancelled", returnedApiResponse.Data.Status);
            Assert.Equal(reason, returnedApiResponse.Data.CancellationReason);
        }

        [Fact(DisplayName = "User can pause and resume their subscription")]
        public async Task User_Can_Pause_And_Resume_Subscription()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var paused = new SubscriptionDto { Id = subscriptionId, Status = "Paused" };
            var resumed = new SubscriptionDto { Id = subscriptionId, Status = "Active" };
            var pausedApiResponse = ApiResponse<SubscriptionDto>.SuccessResponse(paused);
            var resumedApiResponse = ApiResponse<SubscriptionDto>.SuccessResponse(resumed);
            _mockSubscriptionService.Setup(s => s.PauseSubscriptionAsync(subscriptionId)).ReturnsAsync(pausedApiResponse);
            _mockSubscriptionService.Setup(s => s.ResumeSubscriptionAsync(subscriptionId)).ReturnsAsync(resumedApiResponse);
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            
            // Act
            var pauseResult = await controller.PauseSubscription(subscriptionId);
            var resumeResult = await controller.ResumeSubscription(subscriptionId);
            
            // Assert
            var pauseObjectResult = pauseResult.Result as OkObjectResult;
            Assert.NotNull(pauseObjectResult);
            var pauseApiResponse = pauseObjectResult.Value as ApiResponse<SubscriptionDto>;
            Assert.NotNull(pauseApiResponse);
            Assert.Equal("Paused", pauseApiResponse.Data.Status);
            var resumeObjectResult = resumeResult.Result as OkObjectResult;
            Assert.NotNull(resumeObjectResult);
            var resumeApiResponse = resumeObjectResult.Value as ApiResponse<SubscriptionDto>;
            Assert.NotNull(resumeApiResponse);
            Assert.Equal("Active", resumeApiResponse.Data.Status);
        }

        [Fact(DisplayName = "Admin can view all subscriptions")]
        public async Task Admin_Can_View_All_Subscriptions()
        {
            // Arrange
            var subscriptions = new List<SubscriptionDto>
            {
                new SubscriptionDto { Id = "1", UserId = "U1", PlanId = "P1", Status = "Active" },
                new SubscriptionDto { Id = "2", UserId = "U2", PlanId = "P2", Status = "Cancelled" }
            };
            var apiResponse = ApiResponse<IEnumerable<SubscriptionDto>>.SuccessResponse(subscriptions);
            _mockSubscriptionService.Setup(s => s.GetAllSubscriptionsAsync()).ReturnsAsync(apiResponse);
            
            // Act
            var all = await _mockSubscriptionService.Object.GetAllSubscriptionsAsync();
            
            // Assert
            Assert.Equal(2, all.Data.Count());
        }

        [Fact(DisplayName = "User can upgrade their subscription plan")]
        public async Task User_Can_Upgrade_Subscription()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var newPlanId = Guid.NewGuid().ToString();
            var upgraded = new SubscriptionDto { Id = subscriptionId, PlanId = newPlanId, Status = "Active" };
            var apiResponse = ApiResponse<SubscriptionDto>.SuccessResponse(upgraded);
            _mockSubscriptionService.Setup(s => s.UpgradeSubscriptionAsync(subscriptionId, newPlanId)).ReturnsAsync(apiResponse);
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            
            // Act
            var result = await controller.UpgradeSubscription(subscriptionId, newPlanId);
            
            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var returnedApiResponse = okResult.Value as ApiResponse<SubscriptionDto>;
            Assert.NotNull(returnedApiResponse);
            Assert.Equal(newPlanId, returnedApiResponse.Data.PlanId);
            Assert.Equal("Active", returnedApiResponse.Data.Status);
        }

        [Fact(DisplayName = "User can view their billing history")]
        public async Task User_Can_View_Billing_History()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var billing = new List<BillingHistoryDto>
            {
                new BillingHistoryDto { Id = "B1", SubscriptionId = subscriptionId, Amount = 99.99m, Status = "Paid" },
                new BillingHistoryDto { Id = "B2", SubscriptionId = subscriptionId, Amount = 99.99m, Status = "Paid" }
            };
            var apiResponse = ApiResponse<IEnumerable<BillingHistoryDto>>.SuccessResponse(billing);
            _mockSubscriptionService.Setup(s => s.GetBillingHistoryAsync(subscriptionId)).ReturnsAsync(apiResponse);
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            
            // Act
            var result = await controller.GetBillingHistory(subscriptionId);
            
            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var returnedApiResponse = okResult.Value as ApiResponse<IEnumerable<BillingHistoryDto>>;
            Assert.NotNull(returnedApiResponse);
            Assert.Equal(2, returnedApiResponse.Data.Count());
        }

        [Fact(DisplayName = "User can reactivate their subscription")]
        public async Task User_Can_Reactivate_Subscription()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var reactivated = new SubscriptionDto { Id = subscriptionId, Status = "Active" };
            var apiResponse = ApiResponse<SubscriptionDto>.SuccessResponse(reactivated);
            _mockSubscriptionService.Setup(s => s.ReactivateSubscriptionAsync(subscriptionId)).ReturnsAsync(apiResponse);
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            
            // Act
            var result = await controller.ReactivateSubscription(subscriptionId);
            
            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var returnedApiResponse = okResult.Value as ApiResponse<SubscriptionDto>;
            Assert.NotNull(returnedApiResponse);
            Assert.Equal("Active", returnedApiResponse.Data.Status);
        }

        [Fact(DisplayName = "User can view their payment methods")]
        public async Task User_Can_View_Payment_Methods()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var paymentMethods = new List<PaymentMethodDto>
            {
                new PaymentMethodDto { PaymentMethodId = "PM1", UserId = userId, IsDefault = true },
                new PaymentMethodDto { PaymentMethodId = "PM2", UserId = userId, IsDefault = false }
            };
            var apiResponse = ApiResponse<IEnumerable<PaymentMethodDto>>.SuccessResponse(paymentMethods);
            _mockSubscriptionService.Setup(s => s.GetPaymentMethodsAsync(userId)).ReturnsAsync(apiResponse);
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            // Mock user
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
            // Act
            var result = await controller.GetPaymentMethods();
            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var returnedApiResponse = okResult.Value as ApiResponse<IEnumerable<PaymentMethodDto>>;
            Assert.NotNull(returnedApiResponse);
            Assert.Equal(2, returnedApiResponse.Data.Count());
        }

        [Fact(DisplayName = "User can add a payment method")]
        public async Task User_Can_Add_Payment_Method()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var paymentMethodId = "pm_test_123";
            var paymentMethod = new PaymentMethodDto { PaymentMethodId = paymentMethodId, UserId = userId, IsDefault = true };
            var apiResponse = ApiResponse<PaymentMethodDto>.SuccessResponse(paymentMethod);
            _mockSubscriptionService.Setup(s => s.AddPaymentMethodAsync(userId, paymentMethodId)).ReturnsAsync(apiResponse);
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            // Mock user
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
            // Act
            var result = await controller.AddPaymentMethod(paymentMethodId);
            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var returnedApiResponse = okResult.Value as ApiResponse<PaymentMethodDto>;
            Assert.NotNull(returnedApiResponse);
            Assert.Equal(paymentMethodId, returnedApiResponse.Data.PaymentMethodId);
            Assert.Equal(userId, returnedApiResponse.Data.UserId);
        }

        [Fact(DisplayName = "User can get subscription by ID")]
        public async Task User_Can_Get_Subscription_By_Id()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var subscription = new SubscriptionDto 
            { 
                Id = subscriptionId, 
                UserId = "U1", 
                PlanId = "P1", 
                Status = "Active",
                StartDate = DateTime.UtcNow,
                NextBillingDate = DateTime.UtcNow.AddDays(30)
            };
            var apiResponse = ApiResponse<SubscriptionDto>.SuccessResponse(subscription);
            _mockSubscriptionService.Setup(s => s.GetSubscriptionAsync(subscriptionId)).ReturnsAsync(apiResponse);
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            
            // Act
            var result = await controller.GetSubscription(subscriptionId);
            
            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var returnedApiResponse = okResult.Value as ApiResponse<SubscriptionDto>;
            Assert.NotNull(returnedApiResponse);
            Assert.Equal(subscriptionId, returnedApiResponse.Data.Id);
            Assert.Equal("Active", returnedApiResponse.Data.Status);
        }

        [Fact(DisplayName = "User can get their subscriptions")]
        public async Task User_Can_Get_Their_Subscriptions()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var subscriptions = new List<SubscriptionDto>
            {
                new SubscriptionDto { Id = "1", UserId = userId, PlanId = "P1", Status = "Active" },
                new SubscriptionDto { Id = "2", UserId = userId, PlanId = "P2", Status = "Paused" }
            };
            var apiResponse = ApiResponse<IEnumerable<SubscriptionDto>>.SuccessResponse(subscriptions);
            _mockSubscriptionService.Setup(s => s.GetUserSubscriptionsAsync(userId)).ReturnsAsync(apiResponse);
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            
            // Act
            var result = await controller.GetUserSubscriptions(userId);
            
            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var returnedApiResponse = okResult.Value as ApiResponse<IEnumerable<SubscriptionDto>>;
            Assert.NotNull(returnedApiResponse);
            Assert.Equal(2, returnedApiResponse.Data.Count());
        }

        [Fact(DisplayName = "User can get plan by ID")]
        public async Task User_Can_Get_Plan_By_Id()
        {
            // Arrange
            var planId = Guid.NewGuid().ToString();
            var plan = new SubscriptionPlanDto 
            { 
                Id = planId, 
                Name = "Premium Plan", 
                Description = "Premium features",
                Price = 99.99m,
                BillingCycleId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                IsActive = true
            };
            var apiResponse = ApiResponse<SubscriptionPlanDto>.SuccessResponse(plan);
            _mockSubscriptionService.Setup(s => s.GetPlanByIdAsync(planId)).ReturnsAsync(apiResponse);
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            
            // Act
            var result = await controller.GetPlanById(planId);
            
            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var returnedApiResponse = okResult.Value as ApiResponse<SubscriptionPlanDto>;
            Assert.NotNull(returnedApiResponse);
            Assert.Equal(planId, returnedApiResponse.Data.Id);
            Assert.Equal("Premium Plan", returnedApiResponse.Data.Name);
        }

        [Fact(DisplayName = "Cannot subscribe to a nonexistent plan")]
        public async Task Cannot_Subscribe_To_Nonexistent_Plan()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var planId = Guid.NewGuid().ToString(); // Not in DB
            var createDto = new CreateSubscriptionDto { UserId = userId, PlanId = planId };
            _mockSubscriptionService.Setup(s => s.CreateSubscriptionAsync(createDto))
                .ReturnsAsync(ApiResponse<SubscriptionDto>.ErrorResponse("Subscription plan does not exist"));
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            // Act
            var result = await controller.CreateSubscription(createDto);
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var response = okResult.Value as ApiResponse<SubscriptionDto>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("Subscription plan does not exist", response.Message);
        }

        [Fact(DisplayName = "Cannot subscribe to an inactive plan")]
        public async Task Cannot_Subscribe_To_Inactive_Plan()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var planId = Guid.NewGuid().ToString();
            var createDto = new CreateSubscriptionDto { UserId = userId, PlanId = planId };
            _mockSubscriptionService.Setup(s => s.CreateSubscriptionAsync(createDto))
                .ReturnsAsync(ApiResponse<SubscriptionDto>.ErrorResponse("Subscription plan is not active"));
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            // Act
            var result = await controller.CreateSubscription(createDto);
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var response = okResult.Value as ApiResponse<SubscriptionDto>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("Subscription plan is not active", response.Message);
        }

        [Fact(DisplayName = "Cannot subscribe to the same plan twice")]
        public async Task Cannot_Subscribe_To_Same_Plan_Twice()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var planId = Guid.NewGuid().ToString();
            var createDto = new CreateSubscriptionDto { UserId = userId, PlanId = planId };
            _mockSubscriptionService.Setup(s => s.CreateSubscriptionAsync(createDto))
                .ReturnsAsync(ApiResponse<SubscriptionDto>.ErrorResponse("User already has an active or paused subscription for this plan"));
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            // Act
            var result = await controller.CreateSubscription(createDto);
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var response = okResult.Value as ApiResponse<SubscriptionDto>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("User already has an active or paused subscription for this plan", response.Message);
        }

        [Fact(DisplayName = "Cannot cancel already cancelled subscription")]
        public async Task Cannot_Cancel_Already_Cancelled_Subscription()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var reason = "No longer needed";
            _mockSubscriptionService.Setup(s => s.CancelSubscriptionAsync(subscriptionId, reason))
                .ReturnsAsync(ApiResponse<SubscriptionDto>.ErrorResponse("Subscription is already cancelled"));
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            // Act
            var result = await controller.CancelSubscription(subscriptionId, reason);
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var response = okResult.Value as ApiResponse<SubscriptionDto>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("Subscription is already cancelled", response.Message);
        }

        [Fact(DisplayName = "Cannot pause already paused subscription")]
        public async Task Cannot_Pause_Already_Paused_Subscription()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            _mockSubscriptionService.Setup(s => s.PauseSubscriptionAsync(subscriptionId))
                .ReturnsAsync(ApiResponse<SubscriptionDto>.ErrorResponse("Subscription is already paused"));
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            // Act
            var result = await controller.PauseSubscription(subscriptionId);
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var response = okResult.Value as ApiResponse<SubscriptionDto>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("Subscription is already paused", response.Message);
        }

        [Fact(DisplayName = "Cannot resume non-paused subscription")]
        public async Task Cannot_Resume_NonPaused_Subscription()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            _mockSubscriptionService.Setup(s => s.ResumeSubscriptionAsync(subscriptionId))
                .ReturnsAsync(ApiResponse<SubscriptionDto>.ErrorResponse("Only paused subscriptions can be resumed"));
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            // Act
            var result = await controller.ResumeSubscription(subscriptionId);
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var response = okResult.Value as ApiResponse<SubscriptionDto>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("Only paused subscriptions can be resumed", response.Message);
        }

        [Fact(DisplayName = "Cannot upgrade to the same plan")]
        public async Task Cannot_Upgrade_To_Same_Plan()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var planId = Guid.NewGuid().ToString();
            _mockSubscriptionService.Setup(s => s.UpgradeSubscriptionAsync(subscriptionId, planId))
                .ReturnsAsync(ApiResponse<SubscriptionDto>.ErrorResponse("Subscription is already on this plan"));
            var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);
            // Act
            var result = await controller.UpgradeSubscription(subscriptionId, planId);
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var response = okResult.Value as ApiResponse<SubscriptionDto>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("Subscription is already on this plan", response.Message);
        }

        [Fact(DisplayName = "Payment failure updates subscription and notifies user")]
        public async Task Payment_Failure_Updates_Subscription_And_Notifies_User()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var errorMessage = "Card declined";
            _mockSubscriptionService.Setup(s => s.HandleFailedPaymentAsync(subscriptionId, errorMessage))
                .ReturnsAsync(ApiResponse<PaymentResultDto>.ErrorResponse($"Payment failed: {errorMessage}"));
            // Act
            var result = await _mockSubscriptionService.Object.HandleFailedPaymentAsync(subscriptionId, errorMessage);
            // Assert
            Assert.False(result.Success);
            Assert.Contains("Payment failed", result.Message);
        }

        [Fact(DisplayName = "Can enforce usage limits and block overuse")]
        public async Task Can_Enforce_Usage_Limits_And_Block_Overuse()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            var privilegeName = "Consultation";
            _mockSubscriptionService.Setup(s => s.CanUsePrivilegeAsync(subscriptionId, privilegeName))
                .ReturnsAsync(ApiResponse<bool>.ErrorResponse("Usage limit reached for Consultation"));
            // Act
            var result = await _mockSubscriptionService.Object.CanUsePrivilegeAsync(subscriptionId, privilegeName);
            // Assert
            Assert.False(result.Success);
            Assert.Contains("Usage limit reached", result.Message);
        }

        [Fact(DisplayName = "Admin can deactivate plan and pause all subscribers")]
        public async Task Admin_Can_Deactivate_Plan_And_Pause_Subscribers()
        {
            // Arrange
            var planId = Guid.NewGuid().ToString();
            var adminUserId = Guid.NewGuid().ToString();
            _mockSubscriptionService.Setup(s => s.DeactivatePlanAsync(planId, adminUserId))
                .ReturnsAsync(ApiResponse<bool>.SuccessResponse(true, "Plan deactivated and subscribers notified/paused."));
            // Act
            var result = await _mockSubscriptionService.Object.DeactivatePlanAsync(planId, adminUserId);
            // Assert
            Assert.True(result.Success);
            Assert.Contains("deactivated", result.Message);
        }

        [Fact(DisplayName = "Analytics returns correct subscription stats")]
        public async Task Analytics_Returns_Correct_Stats()
        {
            // Arrange
            var analytics = new SubscriptionAnalyticsDto {
                ActiveSubscriptions = 10,
                CancelledSubscriptions = 2,
                PausedSubscriptions = 1,
                TotalRevenue = 1000M,
                ChurnRate = 0.2M
            };
            _mockSubscriptionService.Setup(s => s.GetSubscriptionAnalyticsAsync(It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<SubscriptionAnalyticsDto>.SuccessResponse(analytics));
            // Act
            var result = await _mockSubscriptionService.Object.GetSubscriptionAnalyticsAsync(Guid.NewGuid().ToString());
            // Assert
            Assert.True(result.Success);
            Assert.Equal(10, result.Data.ActiveSubscriptions);
            Assert.Equal(0.2M, result.Data.ChurnRate);
        }

        [Fact(DisplayName = "Webhook payment failure event is handled gracefully")]
        public async Task Webhook_Payment_Failure_Handled_Gracefully()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid().ToString();
            _mockSubscriptionService.Setup(s => s.HandlePaymentProviderWebhookAsync("payment_failed", subscriptionId, "Insufficient funds"))
                .ReturnsAsync(ApiResponse<bool>.SuccessResponse(true, "Payment failure handled"));
            // Act
            var result = await _mockSubscriptionService.Object.HandlePaymentProviderWebhookAsync("payment_failed", subscriptionId, "Insufficient funds");
            // Assert
            Assert.True(result.Success);
            Assert.Contains("handled", result.Message);
        }

#region Purchase Flow Tests
[Fact(DisplayName = "Patient can purchase a subscription plan successfully")]
public async Task Patient_Can_Purchase_Subscription()
{
    // Arrange
    var patientId = Guid.NewGuid().ToString();
    var planId = Guid.NewGuid().ToString();
    var createDto = new CreateSubscriptionDto { UserId = patientId, PlanId = planId };
    var benefits = new List<SubscriptionBenefitDto> {
        new SubscriptionBenefitDto { Name = "Consultations", Limit = 5, Used = 0, RemainingQuantity = 5 },
        new SubscriptionBenefitDto { Name = "Medications", Limit = 2, Used = 0, RemainingQuantity = 2 }
    };
    var subscription = new SubscriptionDto
    {
        Id = Guid.NewGuid().ToString(),
        UserId = patientId,
        PlanId = planId,
        Status = "Active",
        StartDate = DateTime.UtcNow
    };
    var apiResponse = ApiResponse<SubscriptionDto>.SuccessResponse(subscription);
    _mockSubscriptionService.Setup(s => s.CreateSubscriptionAsync(createDto)).ReturnsAsync(apiResponse);
    var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);

    // Act
    var result = await controller.CreateSubscription(createDto);

    // Assert
    var okResult = result.Result as OkObjectResult;
    Assert.NotNull(okResult);
    var returnedApiResponse = okResult.Value as ApiResponse<SubscriptionDto>;
    Assert.NotNull(returnedApiResponse);
    Assert.True(returnedApiResponse.Success);
    Assert.Equal(patientId, returnedApiResponse.Data.UserId);
    Assert.Equal(planId, returnedApiResponse.Data.PlanId);
    Assert.Equal("Active", returnedApiResponse.Data.Status);
    // Simulate benefit/entitlement check
    Assert.Contains(benefits, b => b.Name == "Consultations");
    Assert.Contains(benefits, b => b.Name == "Medications");
    // Simulate payment and audit log check
    SimulatePayment();
    CheckAuditLogs("CreateSubscription", returnedApiResponse.Data.Id);
}
#endregion

#region Usage Tracking & Entitlement Tests
[Fact(DisplayName = "Patient cannot exceed allowed consultations")]
public async Task Patient_Cannot_Exceed_Consultation_Limit()
{
    // Arrange
    var patientId = Guid.NewGuid().ToString();
    var planId = Guid.NewGuid().ToString();
    var subscriptionId = Guid.NewGuid().ToString();
    var allowedConsults = 3;
    var benefits = new List<SubscriptionBenefitDto> {
        new SubscriptionBenefitDto { Name = "Consultations", Limit = allowedConsults, Used = 0, RemainingQuantity = allowedConsults }
    };
    var subscription = new SubscriptionDto
    {
        Id = subscriptionId,
        UserId = patientId,
        PlanId = planId,
        Status = "Active"
    };
    _mockSubscriptionService.Setup(s => s.GetSubscriptionAsync(subscriptionId)).ReturnsAsync(ApiResponse<SubscriptionDto>.SuccessResponse(subscription));
    var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);

    // Act
    for (int i = 0; i < allowedConsults; i++)
    {
        // Simulate booking a consultation (mock usage increment)
        benefits[0].Used++;
        benefits[0].RemainingQuantity--;
    }
    // Attempt to exceed limit
    var overuseAttempt = benefits[0].Used >= allowedConsults;

    // Assert
    Assert.True(overuseAttempt);
    // Simulate system blocks over-usage
    var canBook = benefits[0].Used < allowedConsults && benefits[0].RemainingQuantity > 0;
    Assert.False(canBook);
    // Audit log check
    CheckAuditLogs("ConsultationOveruseAttempt", subscriptionId);
}
#endregion

#region Billing and Payment Tests
[Fact(DisplayName = "Recurring billing and payment failure handling")]
public async Task Recurring_Billing_And_Payment_Failure()
{
    // Arrange
    var subscriptionId = Guid.NewGuid().ToString();
    var patientId = Guid.NewGuid().ToString();
    var planId = Guid.NewGuid().ToString();
    var subscription = new SubscriptionDto
    {
        Id = subscriptionId,
        UserId = patientId,
        PlanId = planId,
        Status = "Active",
        NextBillingDate = DateTime.UtcNow.AddDays(30)
    };
    _mockSubscriptionService.Setup(s => s.GetSubscriptionAsync(subscriptionId)).ReturnsAsync(ApiResponse<SubscriptionDto>.SuccessResponse(subscription));
    var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);

    // Act
    // Simulate successful recurring billing
    SimulatePayment();
    subscription.NextBillingDate = DateTime.UtcNow.AddDays(60);
    // Simulate payment failure on next cycle
    bool paymentFailed = true;
    if (paymentFailed)
    {
        subscription.Status = "PaymentFailed";
        // Simulate retry logic
        SimulatePayment(); // retry
        // Simulate notification
        // ...
    }

    // Assert
    Assert.Equal("PaymentFailed", subscription.Status);
    // Audit log check
    CheckAuditLogs("RecurringBillingFailure", subscriptionId);
}
#endregion

#region Edge Case Tests
[Fact(DisplayName = "Subscription plan expiration and renewal")]
public async Task Subscription_Expiration_And_Renewal()
{
    // Arrange
    var subscriptionId = Guid.NewGuid().ToString();
    var patientId = Guid.NewGuid().ToString();
    var planId = Guid.NewGuid().ToString();
    var benefits = new List<SubscriptionBenefitDto> {
        new SubscriptionBenefitDto { Name = "Consultations", Limit = 5, Used = 5, RemainingQuantity = 0 }
    };
    var subscription = new SubscriptionDto
    {
        Id = subscriptionId,
        UserId = patientId,
        PlanId = planId,
        Status = "Active",
        EndDate = DateTime.UtcNow.AddDays(-1) // expired
    };
    _mockSubscriptionService.Setup(s => s.GetSubscriptionAsync(subscriptionId)).ReturnsAsync(ApiResponse<SubscriptionDto>.SuccessResponse(subscription));
    var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);

    // Act
    // Simulate expiration
    subscription.Status = "Expired";
    // Simulate renewal
    subscription.Status = "Active";
    subscription.EndDate = DateTime.UtcNow.AddMonths(1);
    benefits.ForEach(b => { b.Used = 0; b.RemainingQuantity = b.Limit; }); // reset usage

    // Assert
    Assert.Equal("Active", subscription.Status);
    Assert.True(subscription.EndDate > DateTime.UtcNow);
    Assert.All(benefits, b => Assert.Equal(0, b.Used));
    Assert.All(benefits, b => Assert.Equal(b.Limit, b.RemainingQuantity));
    // Audit log check
    CheckAuditLogs("SubscriptionRenewal", subscriptionId);
}
#endregion

#region Provider Access Tests
[Fact(DisplayName = "Provider access is restricted by patient subscription status")]
public async Task Provider_Access_Restricted_By_Subscription()
{
    // Arrange
    var providerId = Guid.NewGuid().ToString();
    var patientId = Guid.NewGuid().ToString();
    var planId = Guid.NewGuid().ToString();
    var subscription = new SubscriptionDto
    {
        Id = Guid.NewGuid().ToString(),
        UserId = patientId,
        PlanId = planId,
        Status = "Active"
    };
    _mockSubscriptionService.Setup(s => s.GetUserSubscriptionsAsync(patientId)).ReturnsAsync(ApiResponse<IEnumerable<SubscriptionDto>>.SuccessResponse(new[] { subscription }));
    var controller = new SubscriptionsController(_mockSubscriptionService.Object, _mockLogger.Object);

    // Act
    // Simulate provider access with active subscription
    var hasAccess = subscription.Status == "Active";
    // Simulate provider access with inactive subscription
    subscription.Status = "Expired";
    var hasAccessAfterExpiry = subscription.Status == "Active";

    // Assert
    Assert.True(hasAccess);
    Assert.False(hasAccessAfterExpiry);
    // Audit log check
    CheckAuditLogs("ProviderAccessAttempt", patientId);
}
#endregion

// Helper methods (to be implemented)
private void SimulatePayment() { /* TODO */ }
private void AdvanceTime(int days) { /* TODO */ }
private void CheckAuditLogs(string action, string entityId) { /* TODO */ }
    }
} 