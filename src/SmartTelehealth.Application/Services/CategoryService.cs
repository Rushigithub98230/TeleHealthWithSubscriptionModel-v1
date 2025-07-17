using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using AutoMapper;

namespace SmartTelehealth.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;
    
    public CategoryService(
        ICategoryRepository categoryRepository,
        ISubscriptionRepository subscriptionRepository,
        IMapper mapper,
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _subscriptionRepository = subscriptionRepository;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<ApiResponse<CategoryDto>> GetCategoryAsync(Guid id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return ApiResponse<CategoryDto>.ErrorResponse("Category not found", 404);
            
            var categoryDto = _mapper.Map<CategoryDto>(category);
            return ApiResponse<CategoryDto>.SuccessResponse(categoryDto, "Category retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {Id}", id);
            return ApiResponse<CategoryDto>.ErrorResponse("An error occurred while retrieving the category", 500);
        }
    }
    
    public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllCategoriesAsync()
    {
        try
        {
            var categories = await _categoryRepository.GetAllActiveAsync();
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            return ApiResponse<IEnumerable<CategoryDto>>.SuccessResponse(categoryDtos, "Categories retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all categories");
            return ApiResponse<IEnumerable<CategoryDto>>.ErrorResponse("An error occurred while retrieving categories", 500);
        }
    }
    
    public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetActiveCategoriesAsync()
    {
        try
        {
            var categories = await _categoryRepository.GetAllActiveAsync();
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            return ApiResponse<IEnumerable<CategoryDto>>.SuccessResponse(categoryDtos, "Active categories retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active categories");
            return ApiResponse<IEnumerable<CategoryDto>>.ErrorResponse("An error occurred while retrieving active categories", 500);
        }
    }
    
    public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryDto createDto)
    {
        try
        {
            var category = _mapper.Map<Category>(createDto);
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;
            
            var createdCategory = await _categoryRepository.CreateAsync(category);
            var categoryDto = _mapper.Map<CategoryDto>(createdCategory);
            return ApiResponse<CategoryDto>.SuccessResponse(categoryDto, "Category created successfully", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return ApiResponse<CategoryDto>.ErrorResponse("An error occurred while creating the category", 500);
        }
    }
    
    public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(Guid id, UpdateCategoryDto updateDto)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return ApiResponse<CategoryDto>.ErrorResponse("Category not found", 404);
            
            _mapper.Map(updateDto, category);
            category.UpdatedAt = DateTime.UtcNow;
            
            var updatedCategory = await _categoryRepository.UpdateAsync(category);
            var categoryDto = _mapper.Map<CategoryDto>(updatedCategory);
            return ApiResponse<CategoryDto>.SuccessResponse(categoryDto, "Category updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {Id}", id);
            return ApiResponse<CategoryDto>.ErrorResponse("An error occurred while updating the category", 500);
        }
    }
    
    public async Task<ApiResponse<object>> DeleteCategoryAsync(Guid id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return ApiResponse<object>.ErrorResponse("Category not found", 404);
            
            var result = await _categoryRepository.DeleteAsync(id);
            if (!result)
                return ApiResponse<object>.ErrorResponse("Failed to delete category", 500);
            
            return ApiResponse<object>.SuccessResponse(null, "Category deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {Id}", id);
            return ApiResponse<object>.ErrorResponse("An error occurred while deleting the category", 500);
        }
    }
    
    public async Task<ApiResponse<bool>> ExistsAsync(Guid id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            var exists = category != null;
            return ApiResponse<bool>.SuccessResponse(exists, "Category existence checked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if category exists {Id}", id);
            return ApiResponse<bool>.ErrorResponse("An error occurred while checking category existence", 500);
        }
    }
    
    public async Task<ApiResponse<IEnumerable<CategoryDto>>> SearchCategoriesAsync(string searchTerm)
    {
        try
        {
            var categories = await _categoryRepository.GetAllActiveAsync();
            var filteredCategories = _mapper.Map<IEnumerable<CategoryDto>>(categories)
                .Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           c.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            
            return ApiResponse<IEnumerable<CategoryDto>>.SuccessResponse(filteredCategories, "Categories searched successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching categories with term {SearchTerm}", searchTerm);
            return ApiResponse<IEnumerable<CategoryDto>>.ErrorResponse("An error occurred while searching categories", 500);
        }
    }
    
    public async Task<ApiResponse<IEnumerable<SubscriptionPlanDto>>> GetCategoryPlansAsync(Guid categoryId)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                return ApiResponse<IEnumerable<SubscriptionPlanDto>>.ErrorResponse("Category not found", 404);
            
            // Get subscription plans for this category
            var plans = await _subscriptionRepository.GetSubscriptionPlansByCategoryAsync(categoryId);
            var planDtos = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(plans);
            return ApiResponse<IEnumerable<SubscriptionPlanDto>>.SuccessResponse(planDtos, "Category plans retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting plans for category {CategoryId}", categoryId);
            return ApiResponse<IEnumerable<SubscriptionPlanDto>>.ErrorResponse("An error occurred while retrieving category plans", 500);
        }
    }
    
    public async Task<ApiResponse<int>> GetActiveCategoryCountAsync()
    {
        try
        {
            var categories = await _categoryRepository.GetAllActiveAsync();
            var count = categories.Count();
            return ApiResponse<int>.SuccessResponse(count, "Active category count retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active category count");
            return ApiResponse<int>.ErrorResponse("An error occurred while retrieving active category count", 500);
        }
    }
} 