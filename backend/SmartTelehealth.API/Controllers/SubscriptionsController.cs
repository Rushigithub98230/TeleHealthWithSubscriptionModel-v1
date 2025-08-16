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
    public async Task<ActionResult<JsonModel>> GetSubscription(string id)
    {
        var response = await _subscriptionService.GetSubscriptionAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<JsonModel>> GetUserSubscriptions(string userId)
    {
        if (!int.TryParse(userId, out int userIdInt))
        {
            return BadRequest(new JsonModel { data = new object(), Message = "Invalid user ID format", StatusCode = 400 });
        }
        var response = await _subscriptionService.GetUserSubscriptionsAsync(userIdInt);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost]
    public async Task<ActionResult<JsonModel>> CreateSubscription([FromBody] CreateSubscriptionDto createDto)
    {
        var response = await _subscriptionService.CreateSubscriptionAsync(createDto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<JsonModel>> CancelSubscription(string id, [FromBody] string reason)
    {
        var response = await _subscriptionService.CancelSubscriptionAsync(id, reason);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("{id}/pause")]
    public async Task<ActionResult<JsonModel>> PauseSubscription(string id)
    {
        var response = await _subscriptionService.PauseSubscriptionAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("{id}/resume")]
    public async Task<ActionResult<JsonModel>> ResumeSubscription(string id)
    {
        var response = await _subscriptionService.ResumeSubscriptionAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("{id}/upgrade")]
    public async Task<ActionResult<JsonModel>> UpgradeSubscription(string id, [FromBody] string newPlanId)
    {
        var response = await _subscriptionService.UpgradeSubscriptionAsync(id, newPlanId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("{id}/reactivate")]
    public async Task<ActionResult<JsonModel>> ReactivateSubscription(string id)
    {
        var response = await _subscriptionService.ReactivateSubscriptionAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("plans")]
    public async Task<ActionResult<JsonModel>> GetAllPlans()
    {
        var response = await _subscriptionService.GetAllPlansAsync();
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("plans/{planId}")]
    public async Task<ActionResult<JsonModel>> GetPlanById(string planId)
    {
        var response = await _subscriptionService.GetPlanByIdAsync(planId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id}/billing-history")]
    public async Task<ActionResult<JsonModel>> GetBillingHistory(string id)
    {
        var response = await _subscriptionService.GetBillingHistoryAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id}/payment-methods")]
    public async Task<ActionResult<JsonModel>> GetPaymentMethods()
    {
        var userId = GetCurrentUserId();
        var response = await _subscriptionService.GetPaymentMethodsAsync(userId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("{id}/payment-methods")]
    public async Task<ActionResult<JsonModel>> AddPaymentMethod([FromBody] string paymentMethodId)
    {
        var userId = GetCurrentUserId();
        var response = await _subscriptionService.AddPaymentMethodAsync(userId, paymentMethodId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("plans")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> CreatePlan([FromBody] CreateSubscriptionPlanDto createDto)
    {
        var response = await _subscriptionService.CreatePlanAsync(createDto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("plans/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> UpdatePlan(string id, [FromBody] UpdateSubscriptionPlanDto updateDto)
    {
        var response = await _subscriptionService.UpdatePlanAsync(id, updateDto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("plans/{id}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> ActivatePlan(string id)
    {
        var response = await _subscriptionService.ActivatePlanAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("plans/{id}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> DeactivatePlan(string id)
    {
        var response = await _subscriptionService.DeactivatePlanAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("plans/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> DeletePlan(string id)
    {
        var response = await _subscriptionService.DeletePlanAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
} 