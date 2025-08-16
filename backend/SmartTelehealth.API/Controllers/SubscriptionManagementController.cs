using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("webadmin/subscription-management")]
[Authorize(Policy = "AdminOnly")]
public class SubscriptionManagementController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ICategoryService _categoryService;
    private readonly IAnalyticsService _analyticsService;
    private readonly IAuditService _auditService;
    private readonly ILogger<SubscriptionManagementController> _logger;

    public SubscriptionManagementController(
        ISubscriptionService subscriptionService,
        ICategoryService categoryService,
        IAnalyticsService analyticsService,
        IAuditService auditService,
        ILogger<SubscriptionManagementController> logger)
    {
        _subscriptionService = subscriptionService;
        _categoryService = categoryService;
        _analyticsService = analyticsService;
        _auditService = auditService;
        _logger = logger;
    }

    #region Subscription Plans Management

    /// <summary>
    /// Get all subscription plans for admin management
    /// </summary>
    [HttpGet("plans")]
    public async Task<ActionResult<JsonModel>> GetAllPlans(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? categoryId = null,
        [FromQuery] bool? isActive = null)
    {
        var response = await _subscriptionService.GetAllPlansAsync(page, pageSize, searchTerm, categoryId, isActive);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Create a new subscription plan
    /// </summary>
    [HttpPost("plans")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> CreatePlan([FromBody] CreateSubscriptionPlanDto createDto)
    {
        var response = await _subscriptionService.CreatePlanAsync(createDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Update an existing subscription plan
    /// </summary>
    [HttpPut("plans/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> UpdatePlan(string id, [FromBody] UpdateSubscriptionPlanDto updateDto)
    {
        if (id != updateDto.Id)
            return BadRequest(new JsonModel { data = new object(), Message = "ID mismatch", StatusCode = 400 });
        
        var response = await _subscriptionService.UpdatePlanAsync(id, updateDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Delete a subscription plan
    /// </summary>
    [HttpDelete("plans/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> DeletePlan(string id)
    {
        var response = await _subscriptionService.DeletePlanAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Activate a subscription plan
    /// </summary>
    [HttpPost("plans/{id}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> ActivatePlan(string id)
    {
        var response = await _subscriptionService.ActivatePlanAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Deactivate a subscription plan
    /// </summary>
    [HttpPost("plans/{id}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> DeactivatePlan(string id)
    {
        var response = await _subscriptionService.DeactivatePlanAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    #region User Subscriptions Management

    /// <summary>
    /// Get all user subscriptions for admin management
    /// </summary>
    [HttpGet("subscriptions")]
    public async Task<ActionResult<JsonModel>> GetAllUserSubscriptions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? userId = null,
        [FromQuery] string? planId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var response = await _subscriptionService.GetAllUserSubscriptionsAsync(page, pageSize, userId, planId, status, startDate, endDate);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Cancel a user subscription
    /// </summary>
    [HttpPost("subscriptions/{id}/cancel")]
    public async Task<ActionResult<JsonModel>> CancelUserSubscription(string id, [FromBody] string? reason = null)
    {
        var response = await _subscriptionService.CancelUserSubscriptionAsync(id, reason);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Pause a user subscription
    /// </summary>
    [HttpPost("subscriptions/{id}/pause")]
    public async Task<ActionResult<JsonModel>> PauseUserSubscription(string id, [FromBody] string? reason = null)
    {
        var response = await _subscriptionService.PauseUserSubscriptionAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Resume a user subscription
    /// </summary>
    [HttpPost("subscriptions/{id}/resume")]
    public async Task<ActionResult<JsonModel>> ResumeUserSubscription(string id)
    {
        var response = await _subscriptionService.ResumeUserSubscriptionAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Extend a user subscription
    /// </summary>
    [HttpPost("subscriptions/{id}/extend")]
    public async Task<ActionResult<JsonModel>> ExtendUserSubscription(string id, [FromBody] ExtendSubscriptionDto extendDto)
    {
        // Calculate additional days from the new end date
        var additionalDays = (int)(extendDto.NewEndDate - DateTime.UtcNow).TotalDays;
        if (additionalDays <= 0)
        {
            return BadRequest(new JsonModel { data = new object(), Message = "New end date must be in the future", StatusCode = 400 });
        }
        
        var response = await _subscriptionService.ExtendUserSubscriptionAsync(id, additionalDays);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    #region Categories Management

    /// <summary>
    /// Get all categories for admin management
    /// </summary>
    [HttpGet("categories")]
    public async Task<ActionResult<JsonModel>> GetAllCategories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null)
    {
        var response = await _categoryService.GetAllCategoriesAsync(page, pageSize, searchTerm, isActive);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost("categories")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> CreateCategory([FromBody] CreateCategoryDto createDto)
    {
        var response = await _categoryService.CreateCategoryAsync(createDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    [HttpPut("categories/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto updateDto)
    {
        if (id.ToString() != updateDto.Id)
            return BadRequest(new JsonModel { data = new object(), Message = "ID mismatch", StatusCode = 400 });
        
        var response = await _categoryService.UpdateCategoryAsync(id, updateDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> DeleteCategory(Guid id)
    {
        var response = await _categoryService.DeleteCategoryAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    #region Analytics

    /// <summary>
    /// Get subscription analytics for admin dashboard
    /// </summary>
    [HttpGet("analytics")]
    public async Task<ActionResult<JsonModel>> GetAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? planId = null)
    {
        var response = await _analyticsService.GetSubscriptionAnalyticsAsync(startDate, endDate, planId);
        return StatusCode(response.StatusCode, response);
    }

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Perform bulk operations on subscriptions
    /// </summary>
    [HttpPost("bulk-action")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> PerformBulkAction([FromBody] BulkActionRequestDto request)
    {
        var results = new List<BulkActionResultDto>();
        
        try
        {
            var result = await _subscriptionService.PerformBulkActionAsync(new List<BulkActionRequestDto> { request });
            results.Add(new BulkActionResultDto
            {
                SubscriptionId = request.SubscriptionId,
                Action = request.Action,
                Success = true,
                Message = "Action completed successfully"
            });
        }
        catch (Exception ex)
        {
            results.Add(new BulkActionResultDto
            {
                SubscriptionId = request.SubscriptionId,
                Action = request.Action,
                Success = false,
                Message = ex.Message
            });
        }
        
        var response = new JsonModel
        {
            data = new BulkActionResultDto
            {
                TotalCount = 1,
                SuccessCount = results.Count(r => r.Success),
                FailureCount = results.Count(r => !r.Success),
                Results = results
            },
            Message = "Action completed",
            StatusCode = 200
        };
        
        return Ok(response);
    }

    #endregion
}

#region DTOs for Admin Subscription Management

public class ExtendSubscriptionDto
{
    public DateTime NewEndDate { get; set; }
    public string? Reason { get; set; }
}

#endregion 