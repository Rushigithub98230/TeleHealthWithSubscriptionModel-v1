using Microsoft.AspNetCore.Authorization;
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

    private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("subscriptions")]
    public async Task<ActionResult<IEnumerable<Subscription>>> GetUserSubscriptions()
    {
        var userId = GetCurrentUserId();
        var subs = await _subscriptionRepo.GetByUserIdAsync(userId);
        return Ok(subs);
    }

    [HttpPost("subscriptions")]
    public async Task<ActionResult> PurchaseSubscription([FromBody] PurchaseSubscriptionDto dto)
    {
        var userId = GetCurrentUserId();
        var createDto = new CreateSubscriptionDto
        {
            UserId = userId.ToString(),
            PlanId = dto.PlanId.ToString()
        };
        var result = await _subscriptionService.CreateSubscriptionAsync(createDto);
        if (!result.Success) return BadRequest(result.Message);
        return Ok(result);
    }

    [HttpPost("subscriptions/cancel")]
    public async Task<ActionResult> CancelSubscription([FromBody] CancelSubscriptionDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _subscriptionService.CancelSubscriptionAsync(dto.SubscriptionId.ToString());
        if (!result.Success) return BadRequest(result.Message);
        return Ok(result);
    }

    [HttpGet("privileges/usage")]
    public async Task<ActionResult<IEnumerable<UserPrivilegeUsageDto>>> GetPrivilegeUsage()
    {
        var userId = GetCurrentUserId();
        var subs = await _subscriptionRepo.GetByUserIdAsync(userId);
        var usageList = new List<UserPrivilegeUsageDto>();
        foreach (var sub in subs)
        {
            var planPrivileges = await _privilegeService.GetPrivilegesForPlanAsync(sub.SubscriptionPlanId);
            foreach (var priv in planPrivileges)
            {
                var remaining = await _privilegeService.GetRemainingPrivilegeAsync(sub.Id, priv.Name);
                usageList.Add(new UserPrivilegeUsageDto
                {
                    SubscriptionId = sub.Id,
                    PrivilegeName = priv.Name,
                    Remaining = remaining
                });
            }
        }
        return Ok(usageList);
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