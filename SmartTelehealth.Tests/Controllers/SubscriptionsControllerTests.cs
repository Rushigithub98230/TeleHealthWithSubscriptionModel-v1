using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartTelehealth.API.Controllers;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Application.DTOs;
using Xunit;

namespace SmartTelehealth.Tests.Controllers
{
    public class SubscriptionsControllerTests : ControllerTestBase
    {
        private readonly Mock<ISubscriptionService> _mockSubscriptionService;
        private readonly SubscriptionsController _controller;

        public SubscriptionsControllerTests()
        {
            _mockSubscriptionService = new Mock<ISubscriptionService>();
            _controller = new SubscriptionsController(_mockSubscriptionService.Object);
            
            var httpContext = CreateMockHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task GetSubscription_ReturnsJsonModel()
        {
            // Arrange
            var id = "test-subscription-id";
            var expectedResult = new JsonModel { StatusCode = 200, Message = "Success" };
            _mockSubscriptionService.Setup(x => x.GetSubscriptionAsync(id, It.IsAny<TokenModel>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetSubscription(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.StatusCode, result.StatusCode);
            Assert.Equal(expectedResult.Message, result.Message);
        }

        [Fact]
        public async Task GetUserSubscriptions_ReturnsJsonModel()
        {
            // Arrange
            var userId = 1;
            var expectedResult = new JsonModel { StatusCode = 200, Message = "Success" };
            _mockSubscriptionService.Setup(x => x.GetUserSubscriptionsAsync(userId, It.IsAny<TokenModel>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetUserSubscriptions(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.StatusCode, result.StatusCode);
            Assert.Equal(expectedResult.Message, result.Message);
        }

        [Fact]
        public async Task GetAllPlans_ReturnsJsonModel()
        {
            // Arrange
            var expectedResult = new JsonModel { StatusCode = 200, Message = "Success" };
            _mockSubscriptionService.Setup(x => x.GetAllPlansAsync(It.IsAny<TokenModel>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetAllPlans();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.StatusCode, result.StatusCode);
            Assert.Equal(expectedResult.Message, result.Message);
        }
    }
}
