using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Infrastructure.Services;

public class MedicationDeliveryService : IMedicationDeliveryService
{
    private readonly IMedicationDeliveryRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<MedicationDeliveryService> _logger;

    public MedicationDeliveryService(
        IMedicationDeliveryRepository repository,
        IMapper mapper,
        ILogger<MedicationDeliveryService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<MedicationDeliveryDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var delivery = await _repository.GetByIdAsync(id);
            if (delivery == null)
                return ApiResponse<MedicationDeliveryDto>.ErrorResponse("Medication delivery not found");

            var dto = _mapper.Map<MedicationDeliveryDto>(delivery);
            return ApiResponse<MedicationDeliveryDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting medication delivery by ID {Id}", id);
            return ApiResponse<MedicationDeliveryDto>.ErrorResponse("Failed to get medication delivery");
        }
    }

    public async Task<ApiResponse<IEnumerable<MedicationDeliveryDto>>> GetByUserIdAsync(int userId)
    {
        try
        {
            var deliveries = await _repository.GetByUserIdAsync(userId);
            var dtos = _mapper.Map<IEnumerable<MedicationDeliveryDto>>(deliveries);
            return ApiResponse<IEnumerable<MedicationDeliveryDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting medication deliveries for user {UserId}", userId);
            return ApiResponse<IEnumerable<MedicationDeliveryDto>>.ErrorResponse("Failed to get medication deliveries");
        }
    }

    public async Task<ApiResponse<MedicationDeliveryDto>> CreateAsync(CreateMedicationDeliveryDto createDto)
    {
        try
        {
            var delivery = _mapper.Map<MedicationDelivery>(createDto);
            var created = await _repository.CreateAsync(delivery);
            var dto = _mapper.Map<MedicationDeliveryDto>(created);
            return ApiResponse<MedicationDeliveryDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating medication delivery");
            return ApiResponse<MedicationDeliveryDto>.ErrorResponse("Failed to create medication delivery");
        }
    }

    public async Task<ApiResponse<MedicationDeliveryDto>> UpdateAsync(Guid id, UpdateMedicationDeliveryDto updateDto)
    {
        try
        {
            var delivery = await _repository.GetByIdAsync(id);
            if (delivery == null)
                return ApiResponse<MedicationDeliveryDto>.ErrorResponse("Medication delivery not found");

            _mapper.Map(updateDto, delivery);
            var updated = await _repository.UpdateAsync(delivery);
            var dto = _mapper.Map<MedicationDeliveryDto>(updated);
            return ApiResponse<MedicationDeliveryDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating medication delivery {Id}", id);
            return ApiResponse<MedicationDeliveryDto>.ErrorResponse("Failed to update medication delivery");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var result = await _repository.DeleteAsync(id);
            return ApiResponse<bool>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting medication delivery {Id}", id);
            return ApiResponse<bool>.ErrorResponse("Failed to delete medication delivery");
        }
    }

    public async Task<ApiResponse<IEnumerable<MedicationDeliveryDto>>> GetAllAsync()
    {
        try
        {
            var deliveries = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<MedicationDeliveryDto>>(deliveries);
            return ApiResponse<IEnumerable<MedicationDeliveryDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all medication deliveries");
            return ApiResponse<IEnumerable<MedicationDeliveryDto>>.ErrorResponse("Failed to get medication deliveries");
        }
    }
} 