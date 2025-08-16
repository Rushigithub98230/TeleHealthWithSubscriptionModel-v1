using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionAutomationController : ControllerBase
{
    private readonly ISubscriptionAutomationService _automationService;
    private readonly ISubscriptionLifecycleService _lifecycleService;
    private readonly IAutomatedBillingService _automatedBillingService;
    private readonly ILogger<SubscriptionAutomationController> _logger;

    public SubscriptionAutomationController(
        ISubscriptionAutomationService automationService,
        ISubscriptionLifecycleService lifecycleService,
        IAutomatedBillingService automatedBillingService,
        ILogger<SubscriptionAutomationController> logger)
    {
        _automationService = automationService;
        _lifecycleService = lifecycleService;
        _automatedBillingService = automatedBillingService;
        _logger = logger;
    }

    /// <summary>
    /// Manual billing trigger
    /// </summary>
    [HttpPost("billing/trigger")]
    public async Task<ActionResult<JsonModel>> TriggerBilling()
    {
        try
        {
            _logger.LogInformation("Manual billing trigger requested by admin");
            await _automatedBillingService.ProcessRecurringBillingAsync();
            
            return Ok(new JsonModel { 
                data = true, 
                Message = "Billing process completed successfully", 
                StatusCode = 200 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in manual billing trigger");
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to process automated billing", StatusCode = 500 });
        }
    }

    /// <summary>
    /// Process subscription renewal
    /// </summary>
    [HttpPost("renew/{subscriptionId}")]
    public async Task<ActionResult<JsonModel>> RenewSubscription(string subscriptionId)
    {
        try
        {
            await _automatedBillingService.ProcessSubscriptionRenewalAsync();
            return Ok(new JsonModel { 
                data = true, 
                Message = "Subscription renewal processed successfully", 
                StatusCode = 200 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing subscription {SubscriptionId}", subscriptionId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to renew subscription", StatusCode = 500 });
        }
    }

    /// <summary>
    /// Process plan change with proration
    /// </summary>
    [HttpPost("change-plan/{subscriptionId}")]
    public async Task<ActionResult<JsonModel>> ChangePlan(string subscriptionId, [FromBody] ChangePlanRequest request)
    {
        try
        {
            if (!Guid.TryParse(subscriptionId, out var subscriptionGuid) || !Guid.TryParse(request.NewPlanId, out var planGuid))
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Invalid subscription or plan ID", StatusCode = 400 });
            }
            
            await _automatedBillingService.ProcessPlanChangeAsync(subscriptionGuid, planGuid);
            return Ok(new JsonModel { 
                data = true, 
                Message = "Plan change processed successfully", 
                StatusCode = 200 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing plan for subscription {SubscriptionId}", subscriptionId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to change plan", StatusCode = 500 });
        }
    }

    /// <summary>
    /// Process state transition
    /// </summary>
    [HttpPost("state-transition/{subscriptionId}")]
    public async Task<ActionResult<JsonModel>> ProcessStateTransition(string subscriptionId, [FromBody] StateTransitionRequest request)
    {
        try
        {
            if (!Guid.TryParse(subscriptionId, out var subscriptionGuid))
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Invalid subscription ID", StatusCode = 400 });
            }
            
            var success = await _lifecycleService.UpdateSubscriptionStatusAsync(subscriptionGuid, request.NewStatus, request.Reason);
            if (success)
            {
                return Ok(new JsonModel { 
                    data = true, 
                    Message = "State transition processed successfully", 
                    StatusCode = 200 
                });
            }
            else
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Failed to process state transition", StatusCode = 400 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing state transition for subscription {SubscriptionId}", subscriptionId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to process state transition", StatusCode = 400 });
        }
    }

    /// <summary>
    /// Process subscription expiration
    /// </summary>
    [HttpPost("expire/{subscriptionId}")]
    public async Task<ActionResult<JsonModel>> ProcessExpiration(string subscriptionId)
    {
        try
        {
            if (!Guid.TryParse(subscriptionId, out var subscriptionGuid))
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Invalid subscription ID", StatusCode = 400 });
            }
            
            await _lifecycleService.ProcessSubscriptionExpirationAsync(subscriptionGuid);
            return Ok(new JsonModel { 
                data = true, 
                Message = "Subscription expired successfully", 
                StatusCode = 200 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing expiration for subscription {SubscriptionId}", subscriptionId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to process expiration", StatusCode = 500 });
        }
    }

    /// <summary>
    /// Process subscription suspension
    /// </summary>
    [HttpPost("suspend/{subscriptionId}")]
    public async Task<ActionResult<JsonModel>> ProcessSuspension(string subscriptionId, [FromBody] SuspensionRequest request)
    {
        try
        {
            if (!Guid.TryParse(subscriptionId, out var subscriptionGuid))
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Invalid subscription ID", StatusCode = 400 });
            }
            
            await _lifecycleService.ProcessSubscriptionSuspensionAsync(subscriptionGuid, request.Reason);
            return Ok(new JsonModel { 
                data = true, 
                Message = "Subscription suspended successfully", 
                StatusCode = 200 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing suspension for subscription {SubscriptionId}", subscriptionId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to process suspension", StatusCode = 500 });
        }
    }

    /// <summary>
    /// Get automation status
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<JsonModel>> GetAutomationStatus()
    {
        try
        {
            var status = await _automationService.GetAutomationStatusAsync();
            return Ok(new JsonModel { 
                data = status, 
                Message = "Automation status retrieved successfully", 
                StatusCode = 200 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving automation status");
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to retrieve automation status", StatusCode = 500 });
        }
    }

    /// <summary>
    /// Get automation logs
    /// </summary>
    [HttpGet("logs")]
    public async Task<ActionResult<JsonModel>> GetAutomationLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var logs = await _automationService.GetAutomationLogsAsync(page, pageSize);
            return Ok(new JsonModel { 
                data = logs, 
                Message = "Automation logs retrieved successfully", 
                StatusCode = 200 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving automation logs");
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to retrieve automation logs", StatusCode = 500 });
        }
    }
}
