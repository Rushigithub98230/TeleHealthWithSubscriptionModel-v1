using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IMedicationDeliveryService
{
    Task<ApiResponse<MedicationDeliveryDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<IEnumerable<MedicationDeliveryDto>>> GetByUserIdAsync(Guid userId);
    Task<ApiResponse<MedicationDeliveryDto>> CreateAsync(CreateMedicationDeliveryDto createDto);
    Task<ApiResponse<MedicationDeliveryDto>> UpdateAsync(Guid id, UpdateMedicationDeliveryDto updateDto);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
    Task<ApiResponse<IEnumerable<MedicationDeliveryDto>>> GetAllAsync();
} 