using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using SmartTelehealth.Application.Services;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Entities;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Collections.Generic;

namespace SmartTelehealth.Tests.Services
{
    public class ConsultationServiceTests
    {
        private readonly Mock<IConsultationRepository> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<ConsultationService>> _mockLogger;
        private readonly ConsultationService _service;

        public ConsultationServiceTests()
        {
            _mockRepo = new Mock<IConsultationRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<ConsultationService>>();
            _service = new ConsultationService(_mockRepo.Object, _mockMapper.Object, _mockLogger.Object);
        }

        [Fact(DisplayName = "GetUserOneTimeConsultationsAsync returns consultations successfully")]
        public async Task GetUserOneTimeConsultationsAsync_Success()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var consultations = new List<Consultation> { new Consultation { Id = Guid.NewGuid(), UserId = userId, IsOneTime = true } };
            var dtos = new List<ConsultationDto> { new ConsultationDto { Id = consultations[0].Id.ToString(), UserId = userId.ToString() } };
            _mockRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(consultations);
            _mockMapper.Setup(m => m.Map<IEnumerable<ConsultationDto>>(It.IsAny<IEnumerable<Consultation>>())).Returns(dtos);
            // Act
            var result = await _service.GetUserOneTimeConsultationsAsync(userId);
            // Assert
            Assert.True(result.Success);
            Assert.Equal(dtos, result.Data);
        }

        [Fact(DisplayName = "GetUserOneTimeConsultationsAsync returns error on exception")]
        public async Task GetUserOneTimeConsultationsAsync_Exception_ReturnsError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockRepo.Setup(r => r.GetByUserIdAsync(userId)).ThrowsAsync(new Exception("fail"));
            // Act
            var result = await _service.GetUserOneTimeConsultationsAsync(userId);
            // Assert
            Assert.False(result.Success);
            Assert.Contains("error", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact(DisplayName = "CreateConsultationAsync throws NotImplementedException")]
        public async Task CreateConsultationAsync_NotImplemented()
        {
            // Arrange
            var createDto = new CreateConsultationDto();
            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => _service.CreateConsultationAsync(createDto));
        }

        [Fact(DisplayName = "GetConsultationByIdAsync throws NotImplementedException")]
        public async Task GetConsultationByIdAsync_NotImplemented()
        {
            // Arrange
            var id = Guid.NewGuid();
            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => _service.GetConsultationByIdAsync(id));
        }

        [Fact(DisplayName = "UpdateConsultationAsync throws NotImplementedException")]
        public async Task UpdateConsultationAsync_NotImplemented()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new UpdateConsultationDto();
            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => _service.UpdateConsultationAsync(id, updateDto));
        }

        [Fact(DisplayName = "DeleteConsultationAsync throws NotImplementedException")]
        public async Task DeleteConsultationAsync_NotImplemented()
        {
            // Arrange
            var id = Guid.NewGuid();
            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => _service.DeleteConsultationAsync(id));
        }

        [Fact(DisplayName = "CancelConsultationAsync throws NotImplementedException")]
        public async Task CancelConsultationAsync_NotImplemented()
        {
            // Arrange
            var id = Guid.NewGuid();
            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => _service.CancelConsultationAsync(id, "reason"));
        }

        [Fact(DisplayName = "StartConsultationAsync throws NotImplementedException")]
        public async Task StartConsultationAsync_NotImplemented()
        {
            // Arrange
            var id = Guid.NewGuid();
            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => _service.StartConsultationAsync(id));
        }

        [Fact(DisplayName = "CompleteConsultationAsync throws NotImplementedException")]
        public async Task CompleteConsultationAsync_NotImplemented()
        {
            // Arrange
            var id = Guid.NewGuid();
            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => _service.CompleteConsultationAsync(id, "notes"));
        }
    }
} 