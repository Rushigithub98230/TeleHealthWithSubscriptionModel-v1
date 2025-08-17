using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Services;

public class PrivilegeService : IPrivilegeService
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
        
        // Check if subscription is active
        if (!subscription.IsActive || subscription.IsDeleted || 
            subscription.Status != "Active" && subscription.Status != "Trial")
        {
            return null;
        }
        
        var planPrivileges = await _planPrivilegeRepo.GetByPlanIdAsync(subscription.SubscriptionPlanId);
        return planPrivileges.FirstOrDefault(pp => pp.Privilege.Name == privilegeName);
    }

    // Check if a user has a privilege and how much is left
    public async Task<int> GetRemainingPrivilegeAsync(Guid subscriptionId, string privilegeName, TokenModel tokenModel)
    {
        try
        {
            var planPrivilege = await GetPlanPrivilegeAsync(subscriptionId, privilegeName);
            if (planPrivilege == null) return 0;
            
            // Check if privilege is disabled
            if (planPrivilege.Value == 0) return 0;
            
            // Check if privilege is unlimited
            if (planPrivilege.Value == -1) return int.MaxValue;
            
            var usage = (await _usageRepo.GetBySubscriptionIdAsync(subscriptionId))
                .FirstOrDefault(u => u.SubscriptionPlanPrivilegeId == planPrivilege.Id);
            var used = usage?.UsedValue ?? 0;
            var remaining = Math.Max(0, planPrivilege.Value - used);
            
            _logger.LogInformation("Remaining privilege '{PrivilegeName}' for subscription {SubscriptionId} by user {UserId}: {Remaining}", 
                privilegeName, subscriptionId, tokenModel.UserID, remaining);
            return remaining;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting remaining privilege '{PrivilegeName}' for subscription {SubscriptionId} by user {UserId}", 
                privilegeName, subscriptionId, tokenModel.UserID);
            return 0;
        }
    }

    // Use a privilege (e.g., book a consult)
    public async Task<bool> UsePrivilegeAsync(Guid subscriptionId, string privilegeName, int amount, TokenModel tokenModel)
    {
        try
        {
            // Validate input parameters
            if (amount <= 0) return false;
            
            var planPrivilege = await GetPlanPrivilegeAsync(subscriptionId, privilegeName);
            if (planPrivilege == null) return false;
            
            // Check if privilege is disabled
            if (planPrivilege.Value == 0) return false;
            
            // Check if privilege is unlimited
            if (planPrivilege.Value == -1)
            {
                            // For unlimited privileges, we can always use them
            var unlimitedUsage = (await _usageRepo.GetBySubscriptionIdAsync(subscriptionId))
                .FirstOrDefault(u => u.SubscriptionPlanPrivilegeId == planPrivilege.Id);
            if (unlimitedUsage == null)
            {
                unlimitedUsage = new UserSubscriptionPrivilegeUsage
                {
                    SubscriptionId = subscriptionId,
                    SubscriptionPlanPrivilegeId = planPrivilege.Id,
                    UsedValue = amount
                };
                await _usageRepo.AddAsync(unlimitedUsage);
            }
            else
            {
                unlimitedUsage.UsedValue += amount;
                await _usageRepo.UpdateAsync(unlimitedUsage);
            }
            
            _logger.LogInformation("Unlimited privilege '{PrivilegeName}' used for subscription {SubscriptionId} by user {UserId}: amount {Amount}", 
                privilegeName, subscriptionId, tokenModel.UserID, amount);
            return true;
        }
        
        // For limited privileges, check remaining amount
        var remaining = await GetRemainingPrivilegeAsync(subscriptionId, privilegeName, tokenModel);
        if (remaining < amount) return false;
        
                var limitedUsage = (await _usageRepo.GetBySubscriptionIdAsync(subscriptionId))
            .FirstOrDefault(u => u.SubscriptionPlanPrivilegeId == planPrivilege.Id);
        if (limitedUsage == null)
        {
            limitedUsage = new UserSubscriptionPrivilegeUsage
            {
                SubscriptionId = subscriptionId,
                SubscriptionPlanPrivilegeId = planPrivilege.Id,
                UsedValue = amount
            };
            await _usageRepo.AddAsync(limitedUsage);
        }
        else
        {
            limitedUsage.UsedValue += amount;
            await _usageRepo.UpdateAsync(limitedUsage);
        }
            
            _logger.LogInformation("Privilege '{PrivilegeName}' used for subscription {SubscriptionId} by user {UserId}: amount {Amount}", 
                privilegeName, subscriptionId, tokenModel.UserID, amount);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error using privilege '{PrivilegeName}' for subscription {SubscriptionId} by user {UserId}", 
                privilegeName, subscriptionId, tokenModel.UserID);
            return false;
        }
    }

    // Get all privileges for a plan
    public async Task<IEnumerable<Privilege>> GetPrivilegesForPlanAsync(Guid planId, TokenModel tokenModel)
    {
        try
        {
            var planPrivileges = await _planPrivilegeRepo.GetByPlanIdAsync(planId);
            var privileges = planPrivileges.Select(pp => pp.Privilege);
            
            _logger.LogInformation("Privileges retrieved for plan {PlanId} by user {UserId}: {PrivilegeCount} privileges", 
                planId, tokenModel.UserID, privileges.Count());
            return privileges;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting privileges for plan {PlanId} by user {UserId}", planId, tokenModel.UserID);
            return Enumerable.Empty<Privilege>();
        }
    }

    // Get all privileges
    public async Task<JsonModel> GetAllPrivilegesAsync(TokenModel tokenModel)
    {
        try
        {
            var privileges = await _privilegeRepo.GetAllAsync();
            
            _logger.LogInformation("All privileges retrieved by user {UserId}: {PrivilegeCount} privileges", 
                tokenModel.UserID, privileges.Count());
            return new JsonModel 
            { 
                data = privileges, 
                Message = "All privileges retrieved successfully", 
                StatusCode = 200 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all privileges by user {UserId}", tokenModel.UserID);
            return new JsonModel 
            { 
                data = new object(), 
                Message = "Error retrieving privileges", 
                StatusCode = 500 
            };
        }
    }
} 