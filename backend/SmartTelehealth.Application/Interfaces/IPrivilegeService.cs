using SmartTelehealth.Core.Entities;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IPrivilegeService
{
    Task<int> GetRemainingPrivilegeAsync(Guid subscriptionId, string privilegeName);
    Task<bool> UsePrivilegeAsync(Guid subscriptionId, string privilegeName, int amount = 1);
    Task<IEnumerable<Privilege>> GetPrivilegesForPlanAsync(Guid planId);
    Task<JsonModel> GetAllPrivilegesAsync();
}
