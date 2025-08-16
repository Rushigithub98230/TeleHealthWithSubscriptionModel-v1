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
    
    public async Task<JsonModel> GetCategoryAsync(Guid id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return new JsonModel { data = new object(), Message = "Category not found", StatusCode = 404 };
            
            var categoryDto = _mapper.Map<CategoryDto>(category);
            return new JsonModel { data = categoryDto, Message = "Category retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {Id}", id);
            return new JsonModel { data = new object(), Message = "An error occurred while retrieving the category", StatusCode = 500 };
        }
    }
    
    public async Task<JsonModel> GetAllCategoriesAsync()
    {
        try
        {
            var categories = await _categoryRepository.GetAllActiveAsync();
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            return new JsonModel { data = categoryDtos, Message = "Categories retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all categories");
            return new JsonModel { data = new object(), Message = "An error occurred while retrieving categories", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetAllCategoriesAsync(int page, int pageSize, string? searchTerm, bool? isActive)
    {
        try
        {
            var categories = await _categoryRepository.GetAllActiveAsync();
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            
            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                categoryDtos = categoryDtos.Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }
            
            // Apply active filter if provided
            if (isActive.HasValue)
            {
                categoryDtos = categoryDtos.Where(c => c.IsActive == isActive.Value);
            }
            
            // Apply pagination
            var totalCount = categoryDtos.Count();
            var pagedCategories = categoryDtos
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            var result = new
            {
                Categories = pagedCategories,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
            
            return new JsonModel { data = result, Message = "Categories retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all categories with filters");
            return new JsonModel { data = new object(), Message = "An error occurred while retrieving categories", StatusCode = 500 };
        }
    }
    
    public async Task<JsonModel> GetActiveCategoriesAsync()
    {
        try
        {
            var categories = await _categoryRepository.GetAllActiveAsync();
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            return new JsonModel { data = categoryDtos, Message = "Active categories retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active categories");
            return new JsonModel { data = new object(), Message = "An error occurred while retrieving active categories", StatusCode = 500 };
        }
    }
    
    public async Task<JsonModel> CreateCategoryAsync(CreateCategoryDto createDto)
    {
        try
        {
            var category = _mapper.Map<Category>(createDto);
            category.CreatedDate = DateTime.UtcNow;
            category.UpdatedDate = DateTime.UtcNow;
            
            var createdCategory = await _categoryRepository.CreateAsync(category);
            var categoryDto = _mapper.Map<CategoryDto>(createdCategory);
            return new JsonModel { data = categoryDto, Message = "Category created successfully", StatusCode = 201 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return new JsonModel { data = new object(), Message = "An error occurred while creating the category", StatusCode = 500 };
        }
    }
    
    public async Task<JsonModel> UpdateCategoryAsync(Guid id, UpdateCategoryDto updateDto)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return new JsonModel { data = new object(), Message = "Category not found", StatusCode = 404 };
            
            _mapper.Map(updateDto, category);
            category.UpdatedDate = DateTime.UtcNow;
            
            var updatedCategory = await _categoryRepository.UpdateAsync(category);
            var categoryDto = _mapper.Map<CategoryDto>(updatedCategory);
            return new JsonModel { data = categoryDto, Message = "Category updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {Id}", id);
            return new JsonModel { data = new object(), Message = "An error occurred while updating the category", StatusCode = 500 };
        }
    }
    
    public async Task<JsonModel> DeleteCategoryAsync(Guid id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return new JsonModel { data = new object(), Message = "Category not found", StatusCode = 404 };
            
            var result = await _categoryRepository.DeleteAsync(id);
            if (!result)
                return new JsonModel { data = new object(), Message = "Failed to delete category", StatusCode = 500 };
            
            return new JsonModel { data = null, Message = "Category deleted successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {Id}", id);
            return new JsonModel { data = new object(), Message = "An error occurred while deleting the category", StatusCode = 500 };
        }
    }
    
    public async Task<JsonModel> ExistsAsync(Guid id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            var exists = category != null;
            return new JsonModel { data = exists, Message = "Category existence checked successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if category exists {Id}", id);
            return new JsonModel { data = new object(), Message = "An error occurred while checking category existence", StatusCode = 500 };
        }
    }
    
    public async Task<JsonModel> SearchCategoriesAsync(string searchTerm)
    {
        try
        {
            var categories = await _categoryRepository.GetAllActiveAsync();
            var filteredCategories = _mapper.Map<IEnumerable<CategoryDto>>(categories)
                .Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           c.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            
            return new JsonModel { data = filteredCategories, Message = "Categories searched successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching categories with term {SearchTerm}", searchTerm);
            return new JsonModel { data = new object(), Message = "An error occurred while searching categories", StatusCode = 500 };
        }
    }
    
    public async Task<JsonModel> GetCategoryPlansAsync(Guid categoryId)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                return new JsonModel { data = new object(), Message = "Category not found", StatusCode = 404 };
            
            // Get subscription plans for this category
            var plans = await _subscriptionRepository.GetSubscriptionPlansByCategoryAsync(categoryId);
            var planDtos = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(plans);
            return new JsonModel { data = planDtos, Message = "Category plans retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting plans for category {CategoryId}", categoryId);
            return new JsonModel { data = new object(), Message = "An error occurred while retrieving category plans", StatusCode = 500 };
        }
    }
    
    public async Task<JsonModel> GetActiveCategoryCountAsync()
    {
        try
        {
            var categories = await _categoryRepository.GetAllActiveAsync();
            var count = categories.Count();
            return new JsonModel { data = count, Message = "Active category count retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active category count");
            return new JsonModel { data = new object(), Message = "An error occurred while retrieving active category count", StatusCode = 500 };
        }
    }
} 