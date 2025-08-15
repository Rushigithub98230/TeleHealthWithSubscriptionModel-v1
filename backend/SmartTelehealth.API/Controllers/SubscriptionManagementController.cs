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
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var response = await _subscriptionService.GetAllSubscriptionPlansAsync();
            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            var plans = response.Data?.ToList() ?? new List<SubscriptionPlanDto>();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                plans = plans.Where(p => 
                    p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true
                ).ToList();
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                plans = plans.Where(p => p.IsActive.ToString().Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Apply pagination
            var totalCount = plans.Count;
            var paginatedPlans = plans
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

                               var result = JsonModel>.PaginatedResponse(
                       paginatedPlans, 
                       totalCount, 
                       page, 
                       pageSize, 
                       $"Retrieved {paginatedPlans.Count} plans"
                   );

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription plans for admin");
            return StatusCode(500, JsonModel>.ErrorResponse("Failed to retrieve subscription plans"));
        }
    }

    /// <summary>
    /// Create a new subscription plan
    /// </summary>
    [HttpPost("plans")]
    public async Task<ActionResult<JsonModel> CreatePlan([FromBody] CreateSubscriptionPlanDto createDto)
    {
        try
        {
            var response = await _subscriptionService.CreateSubscriptionPlanAsync(createDto);
            if (response.Success)
            {
                await _auditService.LogActionAsync(
                    "SubscriptionPlan",
                    "CreateSubscriptionPlan",
                    null,
                    $"Created subscription plan: {createDto.Name}"
                );
            }
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription plan");
            return StatusCode(500, JsonModel.ErrorResponse("Failed to create subscription plan"));
        }
    }

    /// <summary>
    /// Update an existing subscription plan
    /// </summary>
    [HttpPut("plans/{id}")]
    public async Task<ActionResult<JsonModel> UpdatePlan(string id, [FromBody] UpdateSubscriptionPlanDto updateDto)
    {
        try
        {
            if (id != updateDto.Id)
                return BadRequest(JsonModel.ErrorResponse("ID mismatch"));

            var response = await _subscriptionService.UpdateSubscriptionPlanAsync(id, updateDto);
            if (response.Success)
            {
                await _auditService.LogActionAsync(
                    "SubscriptionPlan",
                    "UpdateSubscriptionPlan",
                    id,
                    $"Updated subscription plan: {updateDto.Name}"
                );
            }
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription plan {PlanId}", id);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to update subscription plan"));
        }
    }

    /// <summary>
    /// Delete a subscription plan
    /// </summary>
    [HttpDelete("plans/{id}")]
    public async Task<ActionResult<JsonModel> DeletePlan(string id)
    {
        try
        {
            var response = await _subscriptionService.DeleteSubscriptionPlanAsync(id);
            if (response.Success)
            {
                await _auditService.LogActionAsync(
                    "SubscriptionPlan",
                    "DeleteSubscriptionPlan",
                    id,
                    $"Deleted subscription plan: {id}"
                );
            }
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subscription plan {PlanId}", id);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to delete subscription plan"));
        }
    }

    /// <summary>
    /// Activate a subscription plan
    /// </summary>
    [HttpPost("plans/{id}/activate")]
    public async Task<ActionResult<JsonModel> ActivatePlan(string id)
    {
        try
        {
            var response = await _subscriptionService.ActivatePlanAsync(id);
            if (response.Success)
            {
                await _auditService.LogActionAsync(
                    "SubscriptionPlan",
                    "ActivateSubscriptionPlan",
                    id,
                    $"Activated subscription plan: {id}"
                );
            }
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating subscription plan {PlanId}", id);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to activate subscription plan"));
        }
    }

    /// <summary>
    /// Deactivate a subscription plan
    /// </summary>
    [HttpPost("plans/{id}/deactivate")]
    public async Task<ActionResult<JsonModel> DeactivatePlan(string id)
    {
        try
        {
            var response = await _subscriptionService.DeactivatePlanAsync(id);
            if (response.Success)
            {
                await _auditService.LogActionAsync(
                    "SubscriptionPlan",
                    "DeactivateSubscriptionPlan",
                    id,
                    $"Deactivated subscription plan: {id}"
                );
            }
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating subscription plan {PlanId}", id);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to deactivate subscription plan"));
        }
    }

    #endregion

    #region User Subscriptions Management

    /// <summary>
    /// Get all user subscriptions for admin management
    /// </summary>
    [HttpGet("user-subscriptions")]
    public async Task<ActionResult<JsonModel>> GetAllUserSubscriptions(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = null,
        [FromQuery] string? category = null,
        [FromQuery] string? plan = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var response = await _subscriptionService.GetAllSubscriptionsAsync();
            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            var subscriptions = response.Data?.ToList() ?? new List<SubscriptionDto>();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                subscriptions = subscriptions.Where(s => 
                    s.UserName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    s.PlanName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                subscriptions = subscriptions.Where(s => s.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Apply date range filter
            if (startDate.HasValue)
            {
                subscriptions = subscriptions.Where(s => s.StartDate >= startDate.Value).ToList();
            }
            if (endDate.HasValue)
            {
                subscriptions = subscriptions.Where(s => s.StartDate <= endDate.Value).ToList();
            }

            // Apply pagination
            var totalCount = subscriptions.Count;
            var paginatedSubscriptions = subscriptions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

                   var result = JsonModel>.PaginatedResponse(
                       paginatedSubscriptions, 
                       totalCount, 
                       page, 
                       pageSize, 
                       $"Retrieved {paginatedSubscriptions.Count} subscriptions"
                   );

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user subscriptions for admin");
            return StatusCode(500, JsonModel>.ErrorResponse("Failed to retrieve user subscriptions"));
        }
    }

    /// <summary>
    /// Cancel a user subscription
    /// </summary>
    [HttpPut("user-subscriptions/{id}/cancel")]
    public async Task<ActionResult<JsonModel> CancelUserSubscription(string id, [FromBody] string? reason = null)
    {
        try
        {
            var response = await _subscriptionService.CancelSubscriptionAsync(id, reason);
            if (response.Success)
            {
                await _auditService.LogActionAsync(
                    "Subscription",
                    "CancelUserSubscription",
                    id,
                    $"Cancelled user subscription: {id}"
                );
            }
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling user subscription {SubscriptionId}", id);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to cancel user subscription"));
        }
    }

    /// <summary>
    /// Pause a user subscription
    /// </summary>
    [HttpPut("user-subscriptions/{id}/pause")]
    public async Task<ActionResult<JsonModel> PauseUserSubscription(string id, [FromBody] string? reason = null)
    {
        try
        {
            var response = await _subscriptionService.PauseSubscriptionAsync(id);
            if (response.Success)
            {
                await _auditService.LogActionAsync(
                    "Subscription",
                    "PauseUserSubscription",
                    id,
                    $"Paused user subscription: {id}"
                );
            }
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing user subscription {SubscriptionId}", id);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to pause user subscription"));
        }
    }

    /// <summary>
    /// Resume a user subscription
    /// </summary>
    [HttpPut("user-subscriptions/{id}/resume")]
    public async Task<ActionResult<JsonModel> ResumeUserSubscription(string id)
    {
        try
        {
            var response = await _subscriptionService.ResumeSubscriptionAsync(id);
            if (response.Success)
            {
                await _auditService.LogActionAsync(
                    "Subscription",
                    "ResumeUserSubscription",
                    id,
                    $"Resumed user subscription: {id}"
                );
            }
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming user subscription {SubscriptionId}", id);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to resume user subscription"));
        }
    }

    /// <summary>
    /// Extend a user subscription
    /// </summary>
    [HttpPut("user-subscriptions/{id}/extend")]
    public async Task<ActionResult<JsonModel> ExtendUserSubscription(string id, [FromBody] ExtendSubscriptionDto extendDto)
    {
        try
        {
            // Create update DTO for extension
            var updateDto = new UpdateSubscriptionDto
            {
                Id = id,
                EndDate = extendDto.NewEndDate,
                Notes = extendDto.Reason
            };

            var response = await _subscriptionService.UpdateSubscriptionAsync(id, updateDto);
            if (response.Success)
            {
                await _auditService.LogActionAsync(
                    "Subscription",
                    "ExtendUserSubscription",
                    id,
                    $"Extended user subscription: {id}"
                );
            }
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending user subscription {SubscriptionId}", id);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to extend user subscription"));
        }
    }

    #endregion

    #region Categories Management

    /// <summary>
    /// Get all categories for admin management
    /// </summary>
    [HttpGet("categories")]
    public async Task<ActionResult<JsonModel>> GetAllCategories(
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var response = await _categoryService.GetAllCategoriesAsync();
            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            var categories = response.Data?.ToList() ?? new List<CategoryDto>();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                categories = categories.Where(c => 
                    c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true
                ).ToList();
            }

            // Apply pagination
            var totalCount = categories.Count;
            var paginatedCategories = categories
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = JsonModel>.PaginatedResponse(
                paginatedCategories, 
                totalCount, 
                page, 
                pageSize, 
                $"Retrieved {paginatedCategories.Count} categories"
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories for admin");
            return StatusCode(500, JsonModel>.ErrorResponse("Failed to retrieve categories"));
        }
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost("categories")]
    public async Task<ActionResult<JsonModel> CreateCategory([FromBody] CreateCategoryDto createDto)
    {
        try
        {
            var response = await _categoryService.CreateCategoryAsync(createDto);
            if (response.Success)
            {
                await _auditService.LogActionAsync(
                    "Category",
                    "CreateCategory",
                    null,
                    $"Created category: {createDto.Name}"
                );
            }
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, JsonModel.ErrorResponse("Failed to create category"));
        }
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    [HttpPut("categories/{id}")]
    public async Task<ActionResult<JsonModel> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto updateDto)
    {
        try
        {
            var response = await _categoryService.UpdateCategoryAsync(id, updateDto);
            if (response.Success)
            {
                await _auditService.LogActionAsync(
                    "Category",
                    "UpdateCategory",
                    id.ToString(),
                    $"Updated category: {updateDto.Name}"
                );
            }
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to update category"));
        }
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    [HttpDelete("categories/{id}")]
    public async Task<ActionResult<JsonModel> DeleteCategory(Guid id)
    {
        try
        {
            var response = await _categoryService.DeleteCategoryAsync(id);
            if (response.Success)
            {
                await _auditService.LogActionAsync(
                    "Category",
                    "DeleteCategory",
                    id.ToString(),
                    $"Deleted category: {id}"
                );
            }
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to delete category"));
        }
    }

    #endregion

    #region Analytics

    /// <summary>
    /// Get subscription analytics for admin dashboard
    /// </summary>
    [HttpGet("analytics")]
    public async Task<ActionResult<JsonModel> GetAnalytics(
        [FromQuery] string period = "month",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var response = await _analyticsService.GetSubscriptionAnalyticsAsync();
            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription analytics");
            return StatusCode(500, JsonModel.ErrorResponse("Failed to retrieve analytics"));
        }
    }

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Perform bulk operations on subscriptions
    /// </summary>
    [HttpPost("bulk-actions")]
    public async Task<ActionResult<JsonModel> PerformBulkAction([FromBody] BulkActionRequestDto request)
    {
        try
        {
            var results = new List<BulkActionResultDto>();
            var successCount = 0;
            var failureCount = 0;

            foreach (var subscriptionId in request.SubscriptionIds)
            {
                try
                {
                    JsonModel response = null;

                    switch (request.Action.ToLower())
                    {
                        case "cancel":
                            response = await _subscriptionService.CancelSubscriptionAsync(subscriptionId, request.Reason);
                            break;
                        case "pause":
                            response = await _subscriptionService.PauseSubscriptionAsync(subscriptionId);
                            break;
                        case "resume":
                            response = await _subscriptionService.ResumeSubscriptionAsync(subscriptionId);
                            break;
                        default:
                            results.Add(new BulkActionResultDto
                            {
                                SubscriptionId = subscriptionId,
                                Success = false,
                                Message = "Invalid action"
                            });
                            failureCount++;
                            continue;
                    }

                    if (response.Success)
                    {
                        results.Add(new BulkActionResultDto
                        {
                            SubscriptionId = subscriptionId,
                            Success = true,
                            Message = $"{request.Action} successful"
                        });
                        successCount++;

                        // Log audit
                        await _auditService.LogActionAsync(
                            "Subscription",
                            "BulkAction",
                            null,
                            $"Performed bulk action: {request.Action} on {request.SubscriptionIds.Count} subscriptions"
                        );
                    }
                    else
                    {
                        results.Add(new BulkActionResultDto
                        {
                            SubscriptionId = subscriptionId,
                            Success = false,
                            Message = response.Message
                        });
                        failureCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error performing bulk action {Action} on subscription {SubscriptionId}", request.Action, subscriptionId);
                    results.Add(new BulkActionResultDto
                    {
                        SubscriptionId = subscriptionId,
                        Success = false,
                        Message = "Internal error"
                    });
                    failureCount++;
                }
            }

            var result = new JsonModel
            {
                Success = true,
                Data = new BulkActionResultDto
                {
                    TotalCount = request.SubscriptionIds.Count,
                    SuccessCount = successCount,
                    FailureCount = failureCount,
                    Results = results
                },
                Message = $"Bulk {request.Action} completed: {successCount} successful, {failureCount} failed"
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk action {Action}", request.Action);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to perform bulk action"));
        }
    }

    #endregion
}

#region DTOs for Admin Subscription Management

public class ExtendSubscriptionDto
{
    public DateTime NewEndDate { get; set; }
    public string? Reason { get; set; }
}

public class BulkActionRequestDto
{
    public List<string> SubscriptionIds { get; set; } = new();
    public string Action { get; set; } = string.Empty; // cancel, pause, resume
    public string? Reason { get; set; }
}

public class BulkActionResultDto
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<BulkActionResultDto> Results { get; set; } = new();
    public string SubscriptionId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

#endregion 