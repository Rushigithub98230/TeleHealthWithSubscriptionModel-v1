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
    public class ProviderFeeService : IProviderFeeService
    {
        private readonly IProviderFeeRepository _providerFeeRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProviderFeeService> _logger;

        public ProviderFeeService(
            IProviderFeeRepository providerFeeRepository,
            IUserRepository userRepository,
            ICategoryRepository categoryRepository,
            IAuditService auditService,
            INotificationService notificationService,
            IMapper mapper,
            ILogger<ProviderFeeService> logger)
        {
            _providerFeeRepository = providerFeeRepository;
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
            _auditService = auditService;
            _notificationService = notificationService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<ProviderFeeDto>> CreateFeeAsync(CreateProviderFeeDto createDto)
        {
            try
            {
                // Validate provider exists
                var provider = await _userRepository.GetByIdAsync(createDto.ProviderId);
                if (provider == null)
                {
                    return new ApiResponse<ProviderFeeDto>
                    {
                        Success = false,
                        Message = "Provider not found",
                        StatusCode = 404
                    };
                }

                // Validate category exists
                var category = await _categoryRepository.GetByIdAsync(createDto.CategoryId);
                if (category == null)
                {
                    return new ApiResponse<ProviderFeeDto>
                    {
                        Success = false,
                        Message = "Category not found",
                        StatusCode = 404
                    };
                }

                // Check if fee already exists for this provider and category
                var existingFee = await _providerFeeRepository.GetByProviderAndCategoryAsync(createDto.ProviderId, createDto.CategoryId);
                if (existingFee != null)
                {
                    return new ApiResponse<ProviderFeeDto>
                    {
                        Success = false,
                        Message = "Fee proposal already exists for this provider and category",
                        StatusCode = 400
                    };
                }

                var fee = new ProviderFee
                {
                    ProviderId = createDto.ProviderId,
                    CategoryId = createDto.CategoryId,
                    ProposedFee = createDto.ProposedFee,
                    Status = FeeStatus.Pending,
                    ProviderNotes = createDto.ProviderNotes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdFee = await _providerFeeRepository.AddAsync(fee);
                var feeDto = _mapper.Map<ProviderFeeDto>(createdFee);

                // Log audit
                await _auditService.LogActionAsync("ProviderFee", "Create", createDto.ProviderId.ToString(), $"Created fee proposal for category {category.Name}");

                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await _notificationService.SendNotificationAsync("Admin", "New Fee Proposal",
                //     $"Provider {fee.ProviderId} has submitted a new fee proposal for {fee.CategoryName} with rate {fee.Rate:C}");
                _logger.LogInformation("Email notifications disabled - would have sent admin notification for new fee proposal");

                return new ApiResponse<ProviderFeeDto>
                {
                    Success = true,
                    Message = "Fee proposal created successfully",
                    Data = feeDto,
                    StatusCode = 201
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating provider fee");
                return new ApiResponse<ProviderFeeDto>
                {
                    Success = false,
                    Message = "Failed to create fee proposal",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<ProviderFeeDto>> GetFeeByIdAsync(Guid id)
        {
            try
            {
                var fee = await _providerFeeRepository.GetByIdAsync(id);
                if (fee == null)
                {
                    return new ApiResponse<ProviderFeeDto>
                    {
                        Success = false,
                        Message = "Fee proposal not found",
                        StatusCode = 404
                    };
                }

                var feeDto = _mapper.Map<ProviderFeeDto>(fee);
                return new ApiResponse<ProviderFeeDto>
                {
                    Success = true,
                    Message = "Fee proposal retrieved successfully",
                    Data = feeDto,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving provider fee");
                return new ApiResponse<ProviderFeeDto>
                {
                    Success = false,
                    Message = "Failed to retrieve fee proposal",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<ProviderFeeDto>>> GetFeesByProviderAsync(int providerId)
        {
            try
            {
                var fees = await _providerFeeRepository.GetByProviderIdAsync(providerId);
                var feeDtos = _mapper.Map<IEnumerable<ProviderFeeDto>>(fees);

                return new ApiResponse<IEnumerable<ProviderFeeDto>>
                {
                    Success = true,
                    Message = "Provider fees retrieved successfully",
                    Data = feeDtos,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving provider fees");
                return new ApiResponse<IEnumerable<ProviderFeeDto>>
                {
                    Success = false,
                    Message = "Failed to retrieve provider fees",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<ProviderFeeDto>>> GetFeesByCategoryAsync(Guid categoryId)
        {
            try
            {
                var fees = await _providerFeeRepository.GetByCategoryIdAsync(categoryId);
                var feeDtos = _mapper.Map<IEnumerable<ProviderFeeDto>>(fees);

                return new ApiResponse<IEnumerable<ProviderFeeDto>>
                {
                    Success = true,
                    Message = "Category fees retrieved successfully",
                    Data = feeDtos,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category fees");
                return new ApiResponse<IEnumerable<ProviderFeeDto>>
                {
                    Success = false,
                    Message = "Failed to retrieve category fees",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<ProviderFeeDto>>> GetAllFeesAsync(string? status = null, int page = 1, int pageSize = 50)
        {
            try
            {
                IEnumerable<ProviderFee> fees;
                if (!string.IsNullOrEmpty(status))
                {
                    fees = await _providerFeeRepository.GetByStatusWithPaginationAsync(status, page, pageSize);
                }
                else
                {
                    fees = await _providerFeeRepository.GetAllAsync();
                    fees = fees.Skip((page - 1) * pageSize).Take(pageSize);
                }
                var feeDtos = _mapper.Map<IEnumerable<ProviderFeeDto>>(fees);

                return new ApiResponse<IEnumerable<ProviderFeeDto>>
                {
                    Success = true,
                    Message = "Fees retrieved successfully",
                    Data = feeDtos,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving fees");
                return new ApiResponse<IEnumerable<ProviderFeeDto>>
                {
                    Success = false,
                    Message = "Failed to retrieve fees",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<ProviderFeeDto>> UpdateFeeAsync(Guid id, UpdateProviderFeeDto updateDto)
        {
            try
            {
                var fee = await _providerFeeRepository.GetByIdAsync(id);
                if (fee == null)
                {
                    return new ApiResponse<ProviderFeeDto>
                    {
                        Success = false,
                        Message = "Fee proposal not found",
                        StatusCode = 404
                    };
                }

                // Only allow updates if status is Pending
                if (fee.Status != FeeStatus.Pending)
                {
                    return new ApiResponse<ProviderFeeDto>
                    {
                        Success = false,
                        Message = "Cannot update fee proposal that is not pending",
                        StatusCode = 400
                    };
                }

                fee.ProposedFee = updateDto.ProposedFee;
                fee.ProviderNotes = updateDto.ProviderNotes;
                fee.UpdatedAt = DateTime.UtcNow;

                var updatedFee = await _providerFeeRepository.UpdateAsync(fee);
                var feeDto = _mapper.Map<ProviderFeeDto>(updatedFee);

                // Log audit
                await _auditService.LogActionAsync("ProviderFee", "Update", fee.ProviderId.ToString(), $"Updated fee proposal {id}");

                return new ApiResponse<ProviderFeeDto>
                {
                    Success = true,
                    Message = "Fee proposal updated successfully",
                    Data = feeDto,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating provider fee");
                return new ApiResponse<ProviderFeeDto>
                {
                    Success = false,
                    Message = "Failed to update fee proposal",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<ProviderFeeDto>> ReviewFeeAsync(Guid id, ReviewProviderFeeDto reviewDto)
        {
            try
            {
                var fee = await _providerFeeRepository.GetByIdAsync(id);
                if (fee == null)
                {
                    return new ApiResponse<ProviderFeeDto>
                    {
                        Success = false,
                        Message = "Fee proposal not found",
                        StatusCode = 404
                    };
                }

                // Only allow review if status is Pending
                if (fee.Status != FeeStatus.Pending)
                {
                    return new ApiResponse<ProviderFeeDto>
                    {
                        Success = false,
                        Message = "Cannot review fee proposal that is not pending",
                        StatusCode = 400
                    };
                }

                fee.Status = Enum.Parse<FeeStatus>(reviewDto.Status);
                fee.ApprovedFee = reviewDto.ApprovedFee ?? 0m;
                fee.AdminRemarks = reviewDto.AdminRemarks;
                fee.ReviewedAt = DateTime.UtcNow;
                fee.ReviewedByUserId = reviewDto.ReviewedByUserId;
                fee.UpdatedAt = DateTime.UtcNow;

                var updatedFee = await _providerFeeRepository.UpdateAsync(fee);
                var feeDto = _mapper.Map<ProviderFeeDto>(updatedFee);

                // Log audit
                await _auditService.LogActionAsync("ProviderFee", "Review", fee.ProviderId.ToString(), $"Reviewed fee proposal {id} with status {reviewDto.Status}");

                // Send notification to provider
                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await _notificationService.SendNotificationAsync(fee.ProviderId.ToString(), "Fee Proposal Reviewed",
                //     $"Your fee proposal for {fee.CategoryName} has been reviewed and {reviewDto.Status.ToLower()}.");
                _logger.LogInformation("Email notifications disabled - would have sent provider notification for fee proposal review");

                return new ApiResponse<ProviderFeeDto>
                {
                    Success = true,
                    Message = "Fee proposal reviewed successfully",
                    Data = feeDto,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing provider fee");
                return new ApiResponse<ProviderFeeDto>
                {
                    Success = false,
                    Message = "Failed to review fee proposal",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteFeeAsync(Guid id)
        {
            try
            {
                var fee = await _providerFeeRepository.GetByIdAsync(id);
                if (fee == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Fee proposal not found",
                        StatusCode = 404
                    };
                }

                // Only allow deletion if status is Pending
                if (fee.Status != FeeStatus.Pending)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Cannot delete fee proposal that is not pending",
                        StatusCode = 400
                    };
                }

                var result = await _providerFeeRepository.DeleteAsync(id);
                if (!result)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Failed to delete fee proposal",
                        StatusCode = 500
                    };
                }

                // Log audit
                await _auditService.LogActionAsync("ProviderFee", "Delete", fee.ProviderId.ToString(), $"Deleted fee proposal {id}");

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Fee proposal deleted successfully",
                    Data = true,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting provider fee");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Failed to delete fee proposal",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<FeeStatisticsDto>> GetFeeStatisticsAsync()
        {
            try
            {
                var statistics = await _providerFeeRepository.GetFeeStatisticsAsync();
                var statisticsDto = _mapper.Map<FeeStatisticsDto>(statistics);

                return new ApiResponse<FeeStatisticsDto>
                {
                    Success = true,
                    Message = "Fee statistics retrieved successfully",
                    Data = statisticsDto,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving fee statistics");
                return new ApiResponse<FeeStatisticsDto>
                {
                    Success = false,
                    Message = "Failed to retrieve fee statistics",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<ProviderFeeDto>>> GetPendingFeesAsync()
        {
            try
            {
                var fees = await _providerFeeRepository.GetPendingFeesAsync();
                var feeDtos = _mapper.Map<IEnumerable<ProviderFeeDto>>(fees);

                return new ApiResponse<IEnumerable<ProviderFeeDto>>
                {
                    Success = true,
                    Message = "Pending fees retrieved successfully",
                    Data = feeDtos,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending fees");
                return new ApiResponse<IEnumerable<ProviderFeeDto>>
                {
                    Success = false,
                    Message = "Failed to retrieve pending fees",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<ProviderFeeDto>>> GetFeesByStatusAsync(string status)
        {
            try
            {
                var fees = await _providerFeeRepository.GetByStatusAsync(status);
                var feeDtos = _mapper.Map<IEnumerable<ProviderFeeDto>>(fees);

                return new ApiResponse<IEnumerable<ProviderFeeDto>>
                {
                    Success = true,
                    Message = $"Fees with status {status} retrieved successfully",
                    Data = feeDtos,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving fees by status");
                return new ApiResponse<IEnumerable<ProviderFeeDto>>
                {
                    Success = false,
                    Message = "Failed to retrieve fees by status",
                    StatusCode = 500
                };
            }
        }

        public Task<ApiResponse<ProviderFeeDto>> GetFeeAsync(Guid id) => throw new NotImplementedException();
        public Task<ApiResponse<ProviderFeeDto>> GetFeeByProviderAndCategoryAsync(int providerId, Guid categoryId) => throw new NotImplementedException();
        public Task<ApiResponse<ProviderFeeDto>> ProposeFeeAsync(Guid id) => throw new NotImplementedException();
    }
} 