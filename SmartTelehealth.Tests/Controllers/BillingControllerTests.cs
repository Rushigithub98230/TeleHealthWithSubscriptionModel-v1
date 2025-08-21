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
    public class BillingControllerTests : ControllerTestBase
    {
        private readonly Mock<IBillingService> _mockBillingService;
        private readonly Mock<IPdfService> _mockPdfService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ISubscriptionService> _mockSubscriptionService;
        private readonly BillingController _controller;

        public BillingControllerTests()
        {
            _mockBillingService = new Mock<IBillingService>();
            _mockPdfService = new Mock<IPdfService>();
            _mockUserService = new Mock<IUserService>();
            _mockSubscriptionService = new Mock<ISubscriptionService>();
            _controller = new BillingController(_mockBillingService.Object, _mockPdfService.Object, _mockUserService.Object, _mockSubscriptionService.Object);
            
            var httpContext = CreateMockHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task GetAllBillingRecords_ReturnsJsonModel()
        {
            // Arrange
            var expectedResult = new JsonModel { StatusCode = 200, Message = "Success" };
            _mockBillingService.Setup(x => x.GetAllBillingRecordsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TokenModel>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetAllBillingRecords();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.StatusCode, result.StatusCode);
            Assert.Equal(expectedResult.Message, result.Message);
        }

        [Fact]
        public async Task GetBillingRecord_ReturnsJsonModel()
        {
            // Arrange
            var id = Guid.NewGuid();
            var expectedResult = new JsonModel { StatusCode = 200, Message = "Success" };
            _mockBillingService.Setup(x => x.GetBillingRecordAsync(id, It.IsAny<TokenModel>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetBillingRecord(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.StatusCode, result.StatusCode);
            Assert.Equal(expectedResult.Message, result.Message);
        }

        [Fact]
        public async Task CreateBillingRecord_ReturnsJsonModel()
        {
            // Arrange
            var createDto = new CreateBillingRecordDto
            {
                UserId = 1,
                Amount = 100.00m,
                Description = "Test billing record"
            };
            var expectedResult = new JsonModel { StatusCode = 201, Message = "Created" };
            _mockBillingService.Setup(x => x.CreateBillingRecordAsync(createDto, It.IsAny<TokenModel>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.CreateBillingRecord(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.StatusCode, result.StatusCode);
            Assert.Equal(expectedResult.Message, result.Message);
        }
    }
}
