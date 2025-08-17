using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : BaseController
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpGet("{id}")]
    public async Task<JsonModel> GetSubscription(string id)
    {
        return await _subscriptionService.GetSubscriptionAsync(id, GetToken(HttpContext));
    }

    [HttpGet("user/{userId}")]
    public async Task<JsonModel> GetUserSubscriptions(string userId)
    {
        if (!int.TryParse(userId, out int userIdInt))
        {
            return new JsonModel { data = new object(), Message = "Invalid user ID format", StatusCode = 400 };
        }
        return await _subscriptionService.GetUserSubscriptionsAsync(userIdInt, GetToken(HttpContext));
    }

    [HttpPost]
    public async Task<JsonModel> CreateSubscription([FromBody] CreateSubscriptionDto createDto)
    {
        return await _subscriptionService.CreateSubscriptionAsync(createDto, GetToken(HttpContext));
    }

    [HttpPost("{id}/cancel")]
    public async Task<JsonModel> CancelSubscription(string id, [FromBody] string reason)
    {
        return await _subscriptionService.CancelSubscriptionAsync(id, reason, GetToken(HttpContext));
    }

    [HttpPost("{id}/pause")]
    public async Task<JsonModel> PauseSubscription(string id)
    {
        return await _subscriptionService.PauseSubscriptionAsync(id, GetToken(HttpContext));
    }

    [HttpPost("{id}/resume")]
    public async Task<JsonModel> ResumeSubscription(string id)
    {
        return await _subscriptionService.ResumeSubscriptionAsync(id, GetToken(HttpContext));
    }

    [HttpPost("{id}/upgrade")]
    public async Task<JsonModel> UpgradeSubscription(string id, [FromBody] string newPlanId)
    {
        return await _subscriptionService.UpgradeSubscriptionAsync(id, newPlanId, GetToken(HttpContext));
    }

    [HttpPost("{id}/reactivate")]
    public async Task<JsonModel> ReactivateSubscription(string id)
    {
        return await _subscriptionService.ReactivateSubscriptionAsync(id, GetToken(HttpContext));
    }

    [HttpGet("plans")]
    public async Task<JsonModel> GetAllPlans()
    {
        return await _subscriptionService.GetAllPlansAsync(GetToken(HttpContext));
    }

    [HttpGet("plans/{planId}")]
    public async Task<JsonModel> GetPlanById(string planId)
    {
        return await _subscriptionService.GetPlanByIdAsync(planId, GetToken(HttpContext));
    }

    [HttpGet("{id}/billing-history")]
    public async Task<JsonModel> GetBillingHistory(string id)
    {
        return await _subscriptionService.GetBillingHistoryAsync(id, GetToken(HttpContext));
    }

    [HttpGet("user/{userId}/payment-methods")]
    public async Task<JsonModel> GetPaymentMethods(string userId)
    {
        if (!int.TryParse(userId, out int userIdInt))
        {
            return new JsonModel { data = new object(), Message = "Invalid user ID format", StatusCode = 400 };
        }
        return await _subscriptionService.GetPaymentMethodsAsync(userIdInt, GetToken(HttpContext));
    }

    [HttpPost("user/{userId}/payment-methods")]
    public async Task<JsonModel> AddPaymentMethod(string userId, [FromBody] string paymentMethodId)
    {
        if (!int.TryParse(userId, out int userIdInt))
        {
            return new JsonModel { data = new object(), Message = "Invalid user ID format", StatusCode = 400 };
        }
        return await _subscriptionService.AddPaymentMethodAsync(userIdInt, paymentMethodId, GetToken(HttpContext));
    }

    [HttpGet("plan/{planId}")]
    public async Task<JsonModel> GetSubscriptionByPlanId(string planId)
    {
        return await _subscriptionService.GetSubscriptionByPlanIdAsync(planId, GetToken(HttpContext));
    }

    [HttpGet("active")]
    public async Task<JsonModel> GetActiveSubscriptions()
    {
        return await _subscriptionService.GetActiveSubscriptionsAsync(GetToken(HttpContext));
    }

    [HttpPut("{id}")]
    public async Task<JsonModel> UpdateSubscription(string id, [FromBody] UpdateSubscriptionDto updateDto)
    {
        return await _subscriptionService.UpdateSubscriptionAsync(id, updateDto, GetToken(HttpContext));
    }

    [HttpPost("{id}/process-payment")]
    public async Task<JsonModel> ProcessPayment(string id, [FromBody] PaymentRequestDto paymentRequest)
    {
        return await _subscriptionService.ProcessPaymentAsync(id, paymentRequest, GetToken(HttpContext));
    }

    [HttpGet("{id}/usage-statistics")]
    public async Task<JsonModel> GetUsageStatistics(string id)
    {
        return await _subscriptionService.GetUsageStatisticsAsync(id, GetToken(HttpContext));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> GetAllSubscriptions()
    {
        return await _subscriptionService.GetAllSubscriptionsAsync(GetToken(HttpContext));
    }

    [HttpGet("{id}/analytics")]
    public async Task<JsonModel> GetSubscriptionAnalytics(string id)
    {
        return await _subscriptionService.GetSubscriptionAnalyticsAsync(id, GetToken(HttpContext));
    }

    [HttpPost("plans")]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> CreatePlan([FromBody] CreateSubscriptionPlanDto createPlanDto)
    {
        return await _subscriptionService.CreatePlanAsync(createPlanDto, GetToken(HttpContext));
    }

    [HttpPut("plans/{planId}")]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> UpdatePlan(string planId, [FromBody] UpdateSubscriptionPlanDto updatePlanDto)
    {
        return await _subscriptionService.UpdatePlanAsync(planId, updatePlanDto, GetToken(HttpContext));
    }

    [HttpPost("plans/{planId}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> ActivatePlan(string planId)
    {
        return await _subscriptionService.ActivatePlanAsync(planId, GetToken(HttpContext));
    }

    [HttpPost("plans/{planId}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> DeactivatePlan(string planId)
    {
        return await _subscriptionService.DeactivatePlanAsync(planId, GetToken(HttpContext));
    }

    [HttpDelete("plans/{planId}")]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> DeletePlan(string planId)
    {
        return await _subscriptionService.DeletePlanAsync(planId, GetToken(HttpContext));
    }

    [HttpGet("stripe/{stripeSubscriptionId}")]
    public async Task<JsonModel> GetByStripeSubscriptionId(string stripeSubscriptionId)
    {
        return await _subscriptionService.GetByStripeSubscriptionIdAsync(stripeSubscriptionId, GetToken(HttpContext));
    }

    [HttpGet("admin/user-subscriptions")]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> GetAllUserSubscriptions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? userId = null,
        [FromQuery] string? planId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        return await _subscriptionService.GetAllUserSubscriptionsAsync(page, pageSize, userId, planId, status, startDate, endDate, GetToken(HttpContext));
    }

    [HttpPost("admin/{id}/cancel")]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> CancelUserSubscription(string id, [FromBody] string? reason)
    {
        return await _subscriptionService.CancelUserSubscriptionAsync(id, reason, GetToken(HttpContext));
    }

    [HttpPost("admin/{id}/pause")]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> PauseUserSubscription(string id)
    {
        return await _subscriptionService.PauseUserSubscriptionAsync(id, GetToken(HttpContext));
    }

    [HttpPost("admin/{id}/resume")]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> ResumeUserSubscription(string id)
    {
        return await _subscriptionService.ResumeUserSubscriptionAsync(id, GetToken(HttpContext));
    }

    [HttpPost("admin/{id}/extend")]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> ExtendUserSubscription(string id, [FromBody] int additionalDays)
    {
        return await _subscriptionService.ExtendUserSubscriptionAsync(id, additionalDays, GetToken(HttpContext));
    }

    [HttpPost("admin/bulk-action")]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> PerformBulkAction([FromBody] List<BulkActionRequestDto> actions)
    {
        return await _subscriptionService.PerformBulkActionAsync(actions, GetToken(HttpContext));
    }

    [HttpGet("admin/plans")]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> GetAllSubscriptionPlans()
    {
        return await _subscriptionService.GetAllSubscriptionPlansAsync(GetToken(HttpContext));
    }

    [HttpGet("admin/plans/active")]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> GetActiveSubscriptionPlans()
    {
        return await _subscriptionService.GetActiveSubscriptionPlansAsync(GetToken(HttpContext));
    }

    [HttpGet("admin/plans/category/{category}")]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> GetSubscriptionPlansByCategory(string category)
    {
        return await _subscriptionService.GetSubscriptionPlansByCategoryAsync(category, GetToken(HttpContext));
    }

    [HttpGet("admin/plans/{planId}")]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> GetSubscriptionPlan(string planId)
    {
        return await _subscriptionService.GetSubscriptionPlanAsync(planId, GetToken(HttpContext));
    }

    [HttpPost("admin/plans")]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> CreateSubscriptionPlan([FromBody] CreateSubscriptionDto createDto)
    {
        return await _subscriptionService.CreateSubscriptionPlanAsync(createDto, GetToken(HttpContext));
    }

    [HttpPut("admin/plans/{planId}")]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> UpdateSubscriptionPlan(string planId, [FromBody] UpdateSubscriptionPlanDto updateDto)
    {
        return await _subscriptionService.UpdateSubscriptionPlanAsync(planId, updateDto, GetToken(HttpContext));
    }

    [HttpDelete("admin/plans/{planId}")]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> DeleteSubscriptionPlan(string planId)
    {
        return await _subscriptionService.DeleteSubscriptionPlanAsync(planId, GetToken(HttpContext));
    }

    [HttpGet("admin/categories")]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> GetAllCategories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null)
    {
        return await _subscriptionService.GetAllCategoriesAsync(page, pageSize, searchTerm, isActive, GetToken(HttpContext));
    }

    [HttpGet("admin/plans/paged")]
    [Authorize(Roles = "Admin")]
    public async Task<JsonModel> GetAllPlansPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? categoryId = null,
        [FromQuery] bool? isActive = null)
    {
        return await _subscriptionService.GetAllPlansAsync(page, pageSize, searchTerm, categoryId, isActive, GetToken(HttpContext));
    }
} 