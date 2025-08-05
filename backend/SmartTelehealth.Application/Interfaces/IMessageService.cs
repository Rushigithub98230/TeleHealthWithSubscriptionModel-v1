using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IMessageService
{
    Task<ApiResponse<MessageDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<IEnumerable<MessageDto>>> GetByUserIdAsync(Guid userId);
    Task<ApiResponse<MessageDto>> CreateAsync(CreateMessageDto createDto);
    Task<ApiResponse<MessageDto>> UpdateAsync(Guid id, UpdateMessageDto updateDto);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
    Task<ApiResponse<IEnumerable<MessageDto>>> GetAllAsync();
} 