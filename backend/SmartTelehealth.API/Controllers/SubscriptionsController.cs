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
    public async Task<ActionResult<JsonModel> GetSubscription(string id)
    {
        var result = await _subscriptionService.GetSubscriptionAsync(id);
        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<JsonModel> GetUserSubscriptions(string userId)
    {
        if (!int.TryParse(userId, out int userIdInt))
        {
            return BadRequest(new JsonModel
            {
                data = new object(),
                Message = "Invalid user ID format",
                StatusCode = 400
            });
        }
        var result = await _subscriptionService.GetUserSubscriptionsAsync(userIdInt);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<JsonModel> CreateSubscription([FromBody] CreateSubscriptionDto createDto)
    {
        var result = await _subscriptionService.CreateSubscriptionAsync(createDto);
        return Ok(result);
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<JsonModel> CancelSubscription(string id, [FromBody] string reason)
    {
        var result = await _subscriptionService.CancelSubscriptionAsync(id, reason);
        return Ok(result);
    }

    [HttpPost("{id}/pause")]
    public async Task<ActionResult<JsonModel> PauseSubscription(string id)
    {
        var result = await _subscriptionService.PauseSubscriptionAsync(id);
        return Ok(result);
    }

    [HttpPost("{id}/resume")]
    public async Task<ActionResult<JsonModel> ResumeSubscription(string id)
    {
        var result = await _subscriptionService.ResumeSubscriptionAsync(id);
        return Ok(result);
    }

    [HttpPost("{id}/upgrade")]
    public async Task<ActionResult<JsonModel> UpgradeSubscription(string id, [FromBody] string newPlanId)
    {
        var result = await _subscriptionService.UpgradeSubscriptionAsync(id, newPlanId);
        return Ok(result);
    }

    [HttpPost("{id}/reactivate")]
    public async Task<ActionResult<JsonModel> ReactivateSubscription(string id)
    {
        var result = await _subscriptionService.ReactivateSubscriptionAsync(id);
        return Ok(result);
    }

    [HttpGet("plans")]
    public async Task<ActionResult<JsonModel>> GetAllPlans()
    {
        var result = await _subscriptionService.GetAllPlansAsync();
        return Ok(result);
    }

    [HttpGet("plans/{planId}")]
    public async Task<ActionResult<JsonModel> GetPlanById(string planId)
    {
        var result = await _subscriptionService.GetPlanByIdAsync(planId);
        return Ok(result);
    }

    [HttpGet("{id}/billing-history")]
    public async Task<ActionResult<JsonModel>> GetBillingHistory(string id)
    {
        var result = await _subscriptionService.GetBillingHistoryAsync(id);
        return Ok(result);
    }

    [HttpGet("payment-methods")]
    public async Task<ActionResult<JsonModel>> GetPaymentMethods()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out int userIdInt))
        {
            return BadRequest(new JsonModel>
            {
                Success = false,
                Message = "Invalid user ID format"
            });
        }
        var result = await _subscriptionService.GetPaymentMethodsAsync(userIdInt);
        return Ok(result);
    }

    [HttpPost("payment-methods")]
    public async Task<ActionResult<JsonModel> AddPaymentMethod([FromBody] string paymentMethodId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out int userIdInt))
        {
            return BadRequest(new JsonModel
            {
                Success = false,
                Message = "Invalid user ID format"
            });
        }
        var result = await _subscriptionService.AddPaymentMethodAsync(userIdInt, paymentMethodId);
        return Ok(result);
    }

    [HttpPost("plans")]
    [Authorize(Roles = "Admin,Superadmin")]
    public async Task<ActionResult<JsonModel> CreatePlan([FromBody] CreateSubscriptionPlanDto createDto)
    {
        var result = await _subscriptionService.CreatePlanAsync(createDto);
        return Ok(result);
    }

    [HttpPut("plans/{id}")]
    [Authorize(Roles = "Admin,Superadmin")]
    public async Task<ActionResult<JsonModel> UpdatePlan(string id, [FromBody] UpdateSubscriptionPlanDto updateDto)
    {
        var result = await _subscriptionService.UpdatePlanAsync(id, updateDto);
        return Ok(result);
    }

    [HttpPost("plans/{id}/activate")]
    [Authorize(Roles = "Admin,Superadmin")]
    public async Task<ActionResult<JsonModel> ActivatePlan(string id)
    {
        var result = await _subscriptionService.ActivatePlanAsync(id);
        return Ok(result);
    }

    [HttpPost("plans/{id}/deactivate")]
    [Authorize(Roles = "Admin,Superadmin")]
    public async Task<ActionResult<JsonModel> DeactivatePlan(string id)
    {
        var result = await _subscriptionService.DeactivatePlanAsync(id);
        return Ok(result);
    }

    [HttpDelete("plans/{id}")]
    [Authorize(Roles = "Admin,Superadmin")]
    public async Task<ActionResult<JsonModel> DeletePlan(string id)
    {
        var result = await _subscriptionService.DeletePlanAsync(id);
        return Ok(result);
    }
} 