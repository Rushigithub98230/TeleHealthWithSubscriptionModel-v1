using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Services;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/provider/user")]
[Authorize(Roles = "Admin,Provider")]
public class ProviderPrivilegesController : ControllerBase
{
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly PrivilegeService _privilegeService;

    public ProviderPrivilegesController(
        ISubscriptionRepository subscriptionRepo,
        PrivilegeService privilegeService)
    {
        _subscriptionRepo = subscriptionRepo;
        _privilegeService = privilegeService;
    }

    [HttpGet("{userId}/privileges")]
    public async Task<ActionResult<IEnumerable<UserPrivilegeUsageDto>>> GetUserPrivileges(Guid userId)
    {
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

    [HttpGet("{userId}/privileges/{privilegeName}")]
    public async Task<ActionResult<UserPrivilegeUsageDto>> CheckUserPrivilege(Guid userId, string privilegeName)
    {
        var subs = await _subscriptionRepo.GetByUserIdAsync(userId);
        foreach (var sub in subs)
        {
            var remaining = await _privilegeService.GetRemainingPrivilegeAsync(sub.Id, privilegeName);
            if (remaining > 0)
            {
                return Ok(new UserPrivilegeUsageDto
                {
                    SubscriptionId = sub.Id,
                    PrivilegeName = privilegeName,
                    Remaining = remaining
                });
            }
        }
        return NotFound($"User does not have privilege: {privilegeName}");
    }
} 