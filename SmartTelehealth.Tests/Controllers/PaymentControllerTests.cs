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
    public class PaymentControllerTests : ControllerTestBase
    {
        private readonly Mock<IStripeService> _mockStripeService;
        private readonly Mock<IBillingService> _mockBillingService;
        private readonly Mock<ISubscriptionService> _mockSubscriptionService;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly Mock<IPaymentSecurityService> _mockPaymentSecurityService;
        private readonly PaymentController _controller;

        public PaymentControllerTests()
        {
            _mockStripeService = new Mock<IStripeService>();
            _mockBillingService = new Mock<IBillingService>();
            _mockSubscriptionService = new Mock<ISubscriptionService>();
            _mockAuditService = new Mock<IAuditService>();
            _mockPaymentSecurityService = new Mock<IPaymentSecurityService>();
            _controller = new PaymentController(_mockStripeService.Object, _mockBillingService.Object, _mockSubscriptionService.Object, _mockAuditService.Object, _mockPaymentSecurityService.Object);
            
            var httpContext = CreateMockHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Assert
            Assert.NotNull(_controller);
        }
    }
}
