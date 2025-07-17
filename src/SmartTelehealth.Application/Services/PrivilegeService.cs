using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace SmartTelehealth.Application.Services;

public class PrivilegeService
{
    private readonly IPrivilegeRepository _privilegeRepo;
    private readonly ISubscriptionPlanPrivilegeRepository _planPrivilegeRepo;
    private readonly IUserSubscriptionPrivilegeUsageRepository _usageRepo;
    private readonly ILogger<PrivilegeService> _logger;

    public PrivilegeService(
        IPrivilegeRepository privilegeRepo,
        ISubscriptionPlanPrivilegeRepository planPrivilegeRepo,
        IUserSubscriptionPrivilegeUsageRepository usageRepo,
        ILogger<PrivilegeService> logger)
    {
        _privilegeRepo = privilegeRepo;
        _planPrivilegeRepo = planPrivilegeRepo;
        _usageRepo = usageRepo;
        _logger = logger;
    }

    // Check if a user has a privilege and how much is left
    public async Task<int> GetRemainingPrivilegeAsync(Guid subscriptionId, string privilegeName)
    {
        var planPrivilege = (await _planPrivilegeRepo.GetByPlanIdAsync(subscriptionId))
            .FirstOrDefault(p => p.Privilege.Name == privilegeName);
        if (planPrivilege == null) return 0;
        if (!int.TryParse(planPrivilege.Value, out var allowed)) allowed = 0;
        var usage = (await _usageRepo.GetBySubscriptionIdAsync(subscriptionId))
            .FirstOrDefault(u => u.Privilege.Name == privilegeName);
        var used = usage != null && int.TryParse(usage.UsedValue, out var u) ? u : 0;
        return allowed - used;
    }

    // Use a privilege (e.g., book a consult)
    public async Task<bool> UsePrivilegeAsync(Guid subscriptionId, string privilegeName, int amount = 1)
    {
        var remaining = await GetRemainingPrivilegeAsync(subscriptionId, privilegeName);
        if (remaining < amount) return false;
        var usage = (await _usageRepo.GetBySubscriptionIdAsync(subscriptionId))
            .FirstOrDefault(u => u.Privilege.Name == privilegeName);
        if (usage == null)
        {
            var privilege = (await _privilegeRepo.GetAllAsync()).FirstOrDefault(p => p.Name == privilegeName);
            if (privilege == null) return false;
            usage = new UserSubscriptionPrivilegeUsage
            {
                SubscriptionId = subscriptionId,
                PrivilegeId = privilege.Id,
                UsedValue = amount.ToString()
            };
            await _usageRepo.AddAsync(usage);
        }
        else
        {
            if (!int.TryParse(usage.UsedValue, out var used)) used = 0;
            usage.UsedValue = (used + amount).ToString();
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