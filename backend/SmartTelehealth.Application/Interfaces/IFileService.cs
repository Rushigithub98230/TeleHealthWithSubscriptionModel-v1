using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IFileService
{
    Task<ApiResponse<FileDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<IEnumerable<FileDto>>> GetByUserIdAsync(Guid userId);
    Task<ApiResponse<FileDto>> CreateAsync(CreateFileDto createDto);
    Task<ApiResponse<FileDto>> UpdateAsync(Guid id, UpdateFileDto updateDto);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
    Task<ApiResponse<IEnumerable<FileDto>>> GetAllAsync();
} 