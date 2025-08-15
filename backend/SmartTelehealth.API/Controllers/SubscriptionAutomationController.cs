using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Services;
using SmartTelehealth.Application.Services.BackgroundServices;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class SubscriptionAutomationController : ControllerBase
{
    private readonly IAutomatedBillingService _automatedBillingService;
    private readonly ISubscriptionLifecycleService _lifecycleService;
    private readonly ILogger<SubscriptionAutomationController> _logger;

    public SubscriptionAutomationController(
        IAutomatedBillingService automatedBillingService,
        ISubscriptionLifecycleService lifecycleService,
        ILogger<SubscriptionAutomationController> logger)
    {
        _automatedBillingService = automatedBillingService;
        _lifecycleService = lifecycleService;
        _logger = logger;
    }

    /// <summary>
    /// Manually trigger automated billing process
    /// </summary>
    [HttpPost("trigger-billing")]
    public async Task<ActionResult<JsonModel> TriggerAutomatedBilling()
    {
        try
        {
            _logger.LogInformation("Manual billing trigger requested by admin");
            await _automatedBillingService.ProcessRecurringBillingAsync();
            
            return Ok(JsonModel.SuccessResponse("Billing process completed successfully", 
                "Automated billing process triggered successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in manual billing trigger");
            return StatusCode(500, JsonModel.ErrorResponse("Failed to process automated billing"));
        }
    }

    /// <summary>
    /// Process subscription renewal
    /// </summary>
    [HttpPost("renew/{subscriptionId}")]
    public async Task<ActionResult<JsonModel> RenewSubscription(string subscriptionId)
    {
        try
        {
            await _automatedBillingService.ProcessSubscriptionRenewalAsync();
            return Ok(JsonModel.SuccessResponse("Subscription renewal processed successfully", 
                "Subscription renewal completed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing subscription {SubscriptionId}", subscriptionId);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to renew subscription"));
        }
    }

    /// <summary>
    /// Process plan change with proration
    /// </summary>
    [HttpPost("change-plan/{subscriptionId}")]
    public async Task<ActionResult<JsonModel> ChangePlan(string subscriptionId, [FromBody] ChangePlanRequest request)
    {
        try
        {
            if (!Guid.TryParse(subscriptionId, out var subscriptionGuid) || !Guid.TryParse(request.NewPlanId, out var planGuid))
            {
                return BadRequest(JsonModel.ErrorResponse("Invalid subscription or plan ID"));
            }
            
            await _automatedBillingService.ProcessPlanChangeAsync(subscriptionGuid, planGuid);
            return Ok(JsonModel.SuccessResponse("Plan change processed successfully", 
                "Plan change completed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing plan for subscription {SubscriptionId}", subscriptionId);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to change plan"));
        }
    }

    /// <summary>
    /// Process state transition
    /// </summary>
    [HttpPost("state-transition/{subscriptionId}")]
    public async Task<ActionResult<JsonModel> ProcessStateTransition(string subscriptionId, [FromBody] StateTransitionRequest request)
    {
        try
        {
            if (!Guid.TryParse(subscriptionId, out var subscriptionGuid))
            {
                return BadRequest(JsonModel.ErrorResponse("Invalid subscription ID"));
            }
            
            var success = await _lifecycleService.UpdateSubscriptionStatusAsync(subscriptionGuid, request.NewStatus, request.Reason);
            if (success)
            {
                return Ok(JsonModel.SuccessResponse("State transition processed successfully", 
                    "State transition completed"));
            }
            else
            {
                return BadRequest(JsonModel.ErrorResponse("Failed to process state transition"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing state transition for subscription {SubscriptionId}", subscriptionId);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to process state transition"));
        }
    }

    /// <summary>
    /// Process subscription expiration
    /// </summary>
    [HttpPost("expire/{subscriptionId}")]
    public async Task<ActionResult<JsonModel> ProcessExpiration(string subscriptionId)
    {
        try
        {
            if (!Guid.TryParse(subscriptionId, out var subscriptionGuid))
            {
                return BadRequest(JsonModel.ErrorResponse("Invalid subscription ID"));
            }
            
            var success = await _lifecycleService.ExpireSubscriptionAsync(subscriptionGuid);
            if (success)
            {
                return Ok(JsonModel.SuccessResponse("Subscription expired successfully", 
                    "Subscription expiration completed"));
            }
            else
            {
                return BadRequest(JsonModel.ErrorResponse("Failed to expire subscription"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing expiration for subscription {SubscriptionId}", subscriptionId);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to process expiration"));
        }
    }

    /// <summary>
    /// Suspend subscription
    /// </summary>
    [HttpPost("suspend/{subscriptionId}")]
    public async Task<ActionResult<JsonModel> SuspendSubscription(string subscriptionId, [FromBody] SuspendRequest request)
    {
        try
        {
            if (!Guid.TryParse(subscriptionId, out var subscriptionGuid))
            {
                return BadRequest(JsonModel.ErrorResponse("Invalid subscription ID"));
            }
            
            var success = await _lifecycleService.SuspendSubscriptionAsync(subscriptionGuid, request.Reason);
            if (success)
            {
                return Ok(JsonModel.SuccessResponse("Subscription suspended successfully", 
                    "Subscription suspension completed"));
            }
            else
            {
                return BadRequest(JsonModel.ErrorResponse("Failed to suspend subscription"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending subscription {SubscriptionId}", subscriptionId);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to suspend subscription"));
        }
    }
}

public class ChangePlanRequest
{
    public string NewPlanId { get; set; } = string.Empty;
    public bool Prorate { get; set; } = true;
}

public class StateTransitionRequest
{
    public string NewStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class SuspendRequest
{
    public string Reason { get; set; } = string.Empty;
}
