using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface ICategoryService
{
    Task<JsonModel> GetCategoryAsync(Guid id);
    Task<JsonModel> GetAllCategoriesAsync();
    Task<JsonModel> GetAllCategoriesAsync(int page, int pageSize, string? searchTerm, bool? isActive);
    Task<JsonModel> GetActiveCategoriesAsync();
    Task<JsonModel> CreateCategoryAsync(CreateCategoryDto createDto);
    Task<JsonModel> UpdateCategoryAsync(Guid id, UpdateCategoryDto updateDto);
    Task<JsonModel> DeleteCategoryAsync(Guid id);
    Task<JsonModel> ExistsAsync(Guid id);
    Task<JsonModel> SearchCategoriesAsync(string searchTerm);
    Task<JsonModel> GetCategoryPlansAsync(Guid categoryId);
    Task<JsonModel> GetActiveCategoryCountAsync();
} 