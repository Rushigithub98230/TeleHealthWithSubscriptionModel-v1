using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTelehealth.Application.Services
{
    public class ProviderPayoutService : IProviderPayoutService
    {
        private readonly IProviderPayoutRepository _providerPayoutRepository;
        private readonly IProviderRepository _providerRepository;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProviderPayoutService> _logger;
        public ProviderPayoutService(
            IProviderPayoutRepository providerPayoutRepository,
            IProviderRepository providerRepository,
            IAuditService auditService,
            INotificationService notificationService,
            IMapper mapper,
            ILogger<ProviderPayoutService> logger)
        {
            _providerPayoutRepository = providerPayoutRepository;
            _providerRepository = providerRepository;
            _auditService = auditService;
            _notificationService = notificationService;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<JsonModel> GetPayoutByIdAsync(Guid id)
        {
            try
            {
                var payout = await _providerPayoutRepository.GetByIdAsync(id);
                if (payout == null)
                {
                    return new JsonModel
                    {
                        Success = false,
                        Message = "Payout not found",
                        StatusCode = 404
                    };
                }
                var payoutDto = _mapper.Map<ProviderPayoutDto>(payout);
                return new JsonModel
                {
                    Success = true,
                    Message = "Payout retrieved successfully",
                    Data = payoutDto,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving provider payout");
                return new JsonModel
                {
                    Success = false,
                    Message = "Failed to retrieve payout",
                    StatusCode = 500
                };
            }
        }
        public async Task<JsonModel> GetPayoutAsync(Guid id) => throw new NotImplementedException();
        public async Task<JsonModel> ProcessPayoutAsync(Guid id, ProcessPayoutDto processDto) => throw new NotImplementedException();
        public async Task<JsonModel> GetPayoutsByProviderAsync(int providerId) => throw new NotImplementedException();
        public async Task<JsonModel> GetPayoutsByPeriodAsync(Guid periodId) => throw new NotImplementedException();
        public async Task<JsonModel> GetAllPayoutsAsync(string status = null, int page = 1, int pageSize = 50)
        {
            try
            {
                IEnumerable<ProviderPayout> payouts;
                if (!string.IsNullOrEmpty(status))
                {
                    payouts = await _providerPayoutRepository.GetByStatusWithPaginationAsync(status, page, pageSize);
                }
                else
                {
                    payouts = await _providerPayoutRepository.GetAllAsync();
                    // Optionally page manually if needed
                    payouts = payouts.Skip((page - 1) * pageSize).Take(pageSize);
                }
                var payoutDtos = _mapper.Map<IEnumerable<ProviderPayoutDto>>(payouts);
                return new JsonModel>
                {
                    Success = true,
                    Message = "Payouts retrieved successfully",
                    Data = payoutDtos,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payouts");
                return new JsonModel>
                {
                    Success = false,
                    Message = "Failed to retrieve payouts",
                    StatusCode = 500
                };
            }
        }
        public async Task<JsonModel> GetPendingPayoutsAsync()
        {
            try
            {
                var payouts = await _providerPayoutRepository.GetPendingPayoutsAsync();
                var payoutDtos = _mapper.Map<IEnumerable<ProviderPayoutDto>>(payouts);
                return new JsonModel>
                {
                    Success = true,
                    Message = "Pending payouts retrieved successfully",
                    Data = payoutDtos,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending payouts");
                return new JsonModel>
                {
                    Success = false,
                    Message = "Failed to retrieve pending payouts",
                    StatusCode = 500
                };
            }
        }
        public async Task<JsonModel> GetPayoutsByStatusAsync(string status)
        {
            try
            {
                var payouts = await _providerPayoutRepository.GetByStatusAsync(status);
                var payoutDtos = _mapper.Map<IEnumerable<ProviderPayoutDto>>(payouts);
                return new JsonModel>
                {
                    Success = true,
                    Message = $"Payouts with status {status} retrieved successfully",
                    Data = payoutDtos,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payouts by status");
                return new JsonModel>
                {
                    Success = false,
                    Message = "Failed to retrieve payouts by status",
                    StatusCode = 500
                };
            }
        }
        public async Task<JsonModel> GetProviderEarningsAsync(int providerId)
        {
            try
            {
                var provider = await _providerRepository.GetByIdAsync(providerId);
                if (provider == null)
                {
                    return new JsonModel
                    {
                        Success = false,
                        Message = "Provider not found",
                        StatusCode = 404
                    };
                }
                var totalEarnings = await _providerPayoutRepository.GetTotalPayoutAmountByProviderAsync(providerId);
                var pendingEarnings = await _providerPayoutRepository.GetPendingPayoutAmountByProviderAsync(providerId);
                var payoutCount = await _providerPayoutRepository.GetPayoutCountByProviderAsync(providerId);
                var earnings = new ProviderEarningsDto
                {
                    ProviderId = providerId,
                    ProviderName = $"{provider.FirstName} {provider.LastName}",
                    TotalEarnings = totalEarnings,
                    PendingEarnings = pendingEarnings,
                    ProcessedEarnings = totalEarnings,
                    TotalConsultations = 0, // TODO: Get from consultation service
                    AveragePerConsultation = payoutCount > 0 ? totalEarnings / payoutCount : 0,
                    LastPayoutDate = null, // TODO: Get from payout history
                    NextPayoutDate = null // TODO: Calculate based on payout schedule
                };
                return new JsonModel
                {
                    Success = true,
                    Message = "Provider earnings retrieved successfully",
                    Data = earnings,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving provider earnings");
                return new JsonModel
                {
                    Success = false,
                    Message = "Failed to retrieve provider earnings",
                    StatusCode = 500
                };
            }
        }
        public async Task<JsonModel> GetPayoutStatisticsAsync()
        {
            try
            {
                var statistics = await _providerPayoutRepository.GetPayoutStatisticsAsync();
                var statisticsDto = _mapper.Map<PayoutStatisticsDto>(statistics);
                return new JsonModel
                {
                    Success = true,
                    Message = "Payout statistics retrieved successfully",
                    Data = statisticsDto,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payout statistics");
                return new JsonModel
                {
                    Success = false,
                    Message = "Failed to retrieve payout statistics",
                    StatusCode = 500
                };
            }
        }
        public async Task<JsonModel> ProcessAllPendingPayoutsAsync()
        {
            try
            {
                var pendingPayouts = await _providerPayoutRepository.GetPendingPayoutsAsync();
                int processedCount = 0;
                foreach (var payout in pendingPayouts)
                {
                    try
                    {
                        var processDto = new ProcessPayoutDto
                        {
                            Status = "Completed",
                            PaymentReference = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{payout.Id}",
                            Notes = "Automatically processed"
                        };
                        await ProcessPayoutAsync(payout.Id, processDto);
                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing payout {payout.Id}");
                    }
                }
                await _auditService.LogActionAsync("ProviderPayout", "ProcessAll", "System", $"Processed {processedCount} pending payouts");
                return new JsonModel
                {
                    Success = true,
                    Message = $"Processed {processedCount} pending payouts",
                    Data = true,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing all pending payouts");
                return new JsonModel
                {
                    Success = false,
                    Message = "Failed to process pending payouts",
                    StatusCode = 500
                };
            }
        }
        public async Task<JsonModel> GeneratePayoutsForPeriodAsync(Guid periodId) => throw new NotImplementedException();
        public async Task<JsonModel> CreatePeriodAsync(CreatePayoutPeriodDto createDto)
        {
            try
            {
                var period = new PayoutPeriod
                {
                    Name = createDto.Name,
                    StartDate = createDto.StartDate,
                    EndDate = createDto.EndDate,
                    Status = PayoutPeriodStatus.Open,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                var createdPeriod = await _providerPayoutRepository.AddPeriodAsync();
                var periodDto = _mapper.Map<PayoutPeriodDto>(createdPeriod);
                await _auditService.LogActionAsync("PayoutPeriod", "Create", "System", $"Created payout period {createDto.Name}");
                return new JsonModel
                {
                    Success = true,
                    Message = "Payout period created successfully",
                    Data = periodDto,
                    StatusCode = 201
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payout period");
                return new JsonModel
                {
                    Success = false,
                    Message = "Failed to create payout period",
                    StatusCode = 500
                };
            }
        }
        public async Task<JsonModel> GetAllPeriodsAsync()
        {
            try
            {
                var periods = await _providerPayoutRepository.GetAllPeriodsAsync();
                var periodDtos = _mapper.Map<IEnumerable<PayoutPeriodDto>>(periods);
                return new JsonModel>
                {
                    Success = true,
                    Message = "Payout periods retrieved successfully",
                    Data = periodDtos,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payout periods");
                return new JsonModel>
                {
                    Success = false,
                    Message = "Failed to retrieve payout periods",
                    StatusCode = 500
                };
            }
        }
    }
} 