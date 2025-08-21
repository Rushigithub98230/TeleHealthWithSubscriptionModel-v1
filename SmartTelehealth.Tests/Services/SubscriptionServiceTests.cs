using Microsoft.Extensions.Logging;
using Moq;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.Services;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Application.DTOs;
using Xunit;
using System.Net;
using AutoMapper;

namespace SmartTelehealth.Tests.Services
{
    public class SubscriptionServiceTests : ServiceTestBase
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
            _mockPrivilegeService = new Mock<PrivilegeService>();
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

        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Assert
            Assert.NotNull(_subscriptionService);
        }

        [Fact]
        public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SubscriptionService(
                null,
                _mockMapper.Object,
                _mockLogger.Object,
                _mockStripeService.Object,
                _mockPrivilegeService.Object,
                _mockNotificationService.Object,
                _mockAuditService.Object,
                _mockUserService.Object,
                _mockPlanPrivilegeRepo.Object,
                _mockUsageRepo.Object,
                _mockBillingService.Object));
        }
    }
}
