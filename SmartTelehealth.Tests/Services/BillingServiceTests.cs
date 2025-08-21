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
    public class BillingServiceTests : ServiceTestBase
    {
        private readonly Mock<IBillingRepository> _mockBillingRepository;
        private readonly Mock<ISubscriptionRepository> _mockSubscriptionRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<BillingService>> _mockLogger;
        private readonly BillingService _billingService;

        public BillingServiceTests()
        {
            _mockBillingRepository = new Mock<IBillingRepository>();
            _mockSubscriptionRepository = new Mock<ISubscriptionRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<BillingService>>();
            
            _billingService = new BillingService(
                _mockBillingRepository.Object,
                _mockSubscriptionRepository.Object,
                _mockMapper.Object,
                _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Assert
            Assert.NotNull(_billingService);
        }

        [Fact]
        public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BillingService(
                null,
                _mockSubscriptionRepository.Object,
                _mockMapper.Object,
                _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullSubscriptionRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BillingService(
                _mockBillingRepository.Object,
                null,
                _mockMapper.Object,
                _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullMapper_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BillingService(
                _mockBillingRepository.Object,
                _mockSubscriptionRepository.Object,
                null,
                _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BillingService(
                _mockBillingRepository.Object,
                _mockSubscriptionRepository.Object,
                _mockMapper.Object,
                null));
        }
    }
}
