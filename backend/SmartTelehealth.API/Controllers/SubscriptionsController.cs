using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(ISubscriptionService subscriptionService, ILogger<SubscriptionsController> logger)
    {
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> GetSubscription(string id)
    {
        var result = await _subscriptionService.GetSubscriptionAsync(id);
        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SubscriptionDto>>>> GetUserSubscriptions(string userId)
    {
        var result = await _subscriptionService.GetUserSubscriptionsAsync(userId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> CreateSubscription([FromBody] CreateSubscriptionDto createDto)
    {
        var result = await _subscriptionService.CreateSubscriptionAsync(createDto);
        return Ok(result);
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> CancelSubscription(string id, [FromBody] string reason)
    {
        var result = await _subscriptionService.CancelSubscriptionAsync(id, reason);
        return Ok(result);
    }

    [HttpPost("{id}/pause")]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> PauseSubscription(string id)
    {
        var result = await _subscriptionService.PauseSubscriptionAsync(id);
        return Ok(result);
    }

    [HttpPost("{id}/resume")]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> ResumeSubscription(string id)
    {
        var result = await _subscriptionService.ResumeSubscriptionAsync(id);
        return Ok(result);
    }

    [HttpPost("{id}/upgrade")]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> UpgradeSubscription(string id, [FromBody] string newPlanId)
    {
        var result = await _subscriptionService.UpgradeSubscriptionAsync(id, newPlanId);
        return Ok(result);
    }

    [HttpPost("{id}/reactivate")]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> ReactivateSubscription(string id)
    {
        var result = await _subscriptionService.ReactivateSubscriptionAsync(id);
        return Ok(result);
    }

    [HttpGet("plans")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SubscriptionPlanDto>>>> GetAllPlans()
    {
        var result = await _subscriptionService.GetAllPlansAsync();
        return Ok(result);
    }

    [HttpGet("plans/{planId}")]
    public async Task<ActionResult<ApiResponse<SubscriptionPlanDto>>> GetPlanById(string planId)
    {
        var result = await _subscriptionService.GetPlanByIdAsync(planId);
        return Ok(result);
    }

    [HttpGet("{id}/billing-history")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BillingHistoryDto>>>> GetBillingHistory(string id)
    {
        var result = await _subscriptionService.GetBillingHistoryAsync(id);
        return Ok(result);
    }

    [HttpGet("payment-methods")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentMethodDto>>>> GetPaymentMethods()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _subscriptionService.GetPaymentMethodsAsync(userId);
        return Ok(result);
    }

    [HttpPost("payment-methods")]
    public async Task<ActionResult<ApiResponse<PaymentMethodDto>>> AddPaymentMethod([FromBody] string paymentMethodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _subscriptionService.AddPaymentMethodAsync(userId, paymentMethodId);
        return Ok(result);
    }

    [HttpPost("plans")]
    [Authorize(Roles = "Admin,Superadmin")]
    public async Task<ActionResult<ApiResponse<SubscriptionPlanDto>>> CreatePlan([FromBody] CreateSubscriptionPlanDto createDto)
    {
        var result = await _subscriptionService.CreatePlanAsync(createDto);
        return Ok(result);
    }

    [HttpPut("plans/{id}")]
    [Authorize(Roles = "Admin,Superadmin")]
    public async Task<ActionResult<ApiResponse<SubscriptionPlanDto>>> UpdatePlan(string id, [FromBody] UpdateSubscriptionPlanDto updateDto)
    {
        var result = await _subscriptionService.UpdatePlanAsync(id, updateDto);
        return Ok(result);
    }

    [HttpPost("plans/{id}/activate")]
    [Authorize(Roles = "Admin,Superadmin")]
    public async Task<ActionResult<ApiResponse<bool>>> ActivatePlan(string id)
    {
        var result = await _subscriptionService.ActivatePlanAsync(id);
        return Ok(result);
    }

    [HttpPost("plans/{id}/deactivate")]
    [Authorize(Roles = "Admin,Superadmin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeactivatePlan(string id)
    {
        var result = await _subscriptionService.DeactivatePlanAsync(id);
        return Ok(result);
    }

    [HttpDelete("plans/{id}")]
    [Authorize(Roles = "Admin,Superadmin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePlan(string id)
    {
        var result = await _subscriptionService.DeletePlanAsync(id);
        return Ok(result);
    }
} 