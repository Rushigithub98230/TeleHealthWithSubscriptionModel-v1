using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface ICategoryService
{
    Task<ApiResponse<CategoryDto>> GetCategoryAsync(Guid id);
    Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllCategoriesAsync();
    Task<ApiResponse<IEnumerable<CategoryDto>>> GetActiveCategoriesAsync();
    Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryDto createDto);
    Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(Guid id, UpdateCategoryDto updateDto);
    Task<ApiResponse<object>> DeleteCategoryAsync(Guid id);
    Task<ApiResponse<bool>> ExistsAsync(Guid id);
    Task<ApiResponse<IEnumerable<CategoryDto>>> SearchCategoriesAsync(string searchTerm);
    Task<ApiResponse<IEnumerable<SubscriptionPlanDto>>> GetCategoryPlansAsync(Guid categoryId);
    Task<ApiResponse<int>> GetActiveCategoryCountAsync();
} 