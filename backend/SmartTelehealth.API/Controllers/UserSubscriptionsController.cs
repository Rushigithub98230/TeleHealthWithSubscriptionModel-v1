using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Services;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System.Security.Claims;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/user/[controller]")]
[Authorize]
public class UserSubscriptionsController : ControllerBase
{
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly ISubscriptionPlanRepository _planRepo;
    private readonly PrivilegeService _privilegeService;
    private readonly SubscriptionService _subscriptionService;

    public UserSubscriptionsController(
        ISubscriptionRepository subscriptionRepo,
        ISubscriptionPlanRepository planRepo,
        PrivilegeService privilegeService,
        SubscriptionService subscriptionService)
    {
        _subscriptionRepo = subscriptionRepo;
        _planRepo = planRepo;
        _privilegeService = privilegeService;
        _subscriptionService = subscriptionService;
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("subscriptions")]
    public async Task<ActionResult<JsonModel>> GetUserSubscriptions()
    {
        var userId = GetCurrentUserId();
        var response = await _subscriptionService.GetUserSubscriptionsAsync(userId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("subscriptions")]
    public async Task<ActionResult> PurchaseSubscription([FromBody] PurchaseSubscriptionDto dto)
    {
        var userId = GetCurrentUserId();
        var createDto = new CreateSubscriptionDto
        {
            UserId = userId,
            PlanId = dto.PlanId.ToString()
        };
        var result = await _subscriptionService.CreateSubscriptionAsync(createDto);
        if (result.StatusCode != 200) return BadRequest(result.Message);
        return Ok(result);
    }

    [HttpPost("subscriptions/cancel")]
    public async Task<ActionResult> CancelSubscription([FromBody] CancelSubscriptionDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _subscriptionService.CancelSubscriptionAsync(dto.SubscriptionId.ToString());
        if (result.StatusCode != 200) return BadRequest(result.Message);
        return Ok(result);
    }

    [HttpGet("privilege-usage")]
    public async Task<ActionResult<JsonModel>> GetPrivilegeUsage()
    {
        var userId = GetCurrentUserId();
        var subscriptions = await _subscriptionService.GetUserSubscriptionsAsync(userId);
        
        if (subscriptions.StatusCode != 200)
            return StatusCode(subscriptions.StatusCode, subscriptions);
            
        var subscriptionList = subscriptions.data as IEnumerable<SubscriptionDto>;
        if (subscriptionList == null || !subscriptionList.Any())
            return Ok(new JsonModel { data = new List<object>(), Message = "No subscriptions found", StatusCode = 200 });
            
        var privilegeUsageList = new List<object>();
        foreach (var subscription in subscriptionList)
        {
            var subscriptionId = Guid.Parse(subscription.Id);
            var planId = Guid.Parse(subscription.PlanId);
            var planPrivileges = await _privilegeService.GetPrivilegesForPlanAsync(planId);
            foreach (var privilege in planPrivileges)
            {
                var remaining = await _privilegeService.GetRemainingPrivilegeAsync(subscriptionId, privilege.Name);
                privilegeUsageList.Add(new
                {
                    SubscriptionId = subscription.Id,
                    PlanName = subscription.PlanName,
                    PrivilegeName = privilege.Name,
                    Remaining = remaining
                });
            }
        }
        
        return Ok(new JsonModel { data = privilegeUsageList, Message = "Privilege usage retrieved successfully", StatusCode = 200 });
    }

    [HttpPost("privileges/use")]
    public async Task<ActionResult> UsePrivilege([FromBody] UsePrivilegeDto dto)
    {
        var userId = GetCurrentUserId();
        var used = await _privilegeService.UsePrivilegeAsync(dto.SubscriptionId, dto.PrivilegeName, dto.Amount);
        if (!used) return BadRequest("Privilege could not be used or limit reached.");
        return Ok();
    }
}

public class PurchaseSubscriptionDto
{
    public Guid PlanId { get; set; }
}

public class CancelSubscriptionDto
{
    public Guid SubscriptionId { get; set; }
}

public class UsePrivilegeDto
{
    public Guid SubscriptionId { get; set; }
    public string PrivilegeName { get; set; } = string.Empty;
    public int Amount { get; set; } = 1;
} 