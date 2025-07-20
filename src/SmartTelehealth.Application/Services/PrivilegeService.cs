using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace SmartTelehealth.Application.Services;

public class PrivilegeService
{
    private readonly IPrivilegeRepository _privilegeRepo;
    private readonly ISubscriptionPlanPrivilegeRepository _planPrivilegeRepo;
    private readonly IUserSubscriptionPrivilegeUsageRepository _usageRepo;
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly ILogger<PrivilegeService> _logger;

    public PrivilegeService(
        IPrivilegeRepository privilegeRepo,
        ISubscriptionPlanPrivilegeRepository planPrivilegeRepo,
        IUserSubscriptionPrivilegeUsageRepository usageRepo,
        ISubscriptionRepository subscriptionRepo,
        ILogger<PrivilegeService> logger)
    {
        _privilegeRepo = privilegeRepo;
        _planPrivilegeRepo = planPrivilegeRepo;
        _usageRepo = usageRepo;
        _subscriptionRepo = subscriptionRepo;
        _logger = logger;
    }

    // Helper to get SubscriptionPlanPrivilege by subscription and privilege name
    private async Task<SubscriptionPlanPrivilege?> GetPlanPrivilegeAsync(Guid subscriptionId, string privilegeName)
    {
        // Fetch the subscription to get the planId
        var subscription = await _subscriptionRepo.GetByIdAsync(subscriptionId);
        if (subscription == null) return null;
        var planPrivileges = await _planPrivilegeRepo.GetByPlanIdAsync(subscription.SubscriptionPlanId);
        return planPrivileges.FirstOrDefault(pp => pp.Privilege.Name == privilegeName);
    }

    // Check if a user has a privilege and how much is left
    public async Task<int> GetRemainingPrivilegeAsync(Guid subscriptionId, string privilegeName)
    {
        var planPrivilege = await GetPlanPrivilegeAsync(subscriptionId, privilegeName);
        if (planPrivilege == null) return 0;
        var allowed = planPrivilege.Value;
        var usage = (await _usageRepo.GetBySubscriptionIdAsync(subscriptionId))
            .FirstOrDefault(u => u.SubscriptionPlanPrivilegeId == planPrivilege.Id);
        var used = usage?.UsedValue ?? 0;
        return allowed - used;
    }

    // Use a privilege (e.g., book a consult)
    public async Task<bool> UsePrivilegeAsync(Guid subscriptionId, string privilegeName, int amount = 1)
    {
        var planPrivilege = await GetPlanPrivilegeAsync(subscriptionId, privilegeName);
        if (planPrivilege == null) return false;
        var remaining = await GetRemainingPrivilegeAsync(subscriptionId, privilegeName);
        if (remaining < amount) return false;
        var usage = (await _usageRepo.GetBySubscriptionIdAsync(subscriptionId))
            .FirstOrDefault(u => u.SubscriptionPlanPrivilegeId == planPrivilege.Id);
        if (usage == null)
        {
            usage = new UserSubscriptionPrivilegeUsage
            {
                SubscriptionId = subscriptionId,
                SubscriptionPlanPrivilegeId = planPrivilege.Id,
                UsedValue = amount
            };
            await _usageRepo.AddAsync(usage);
        }
        else
        {
            usage.UsedValue += amount;
            await _usageRepo.UpdateAsync(usage);
        }
        return true;
    }

    // Get all privileges for a plan
    public async Task<IEnumerable<Privilege>> GetPrivilegesForPlanAsync(Guid planId)
    {
        var planPrivileges = await _planPrivilegeRepo.GetByPlanIdAsync(planId);
        return planPrivileges.Select(pp => pp.Privilege);
    }
} 