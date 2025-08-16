using SmartTelehealth.Core.Entities;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IPrivilegeService
{
    Task<int> GetRemainingPrivilegeAsync(Guid subscriptionId, string privilegeName, TokenModel tokenModel);
    Task<bool> UsePrivilegeAsync(Guid subscriptionId, string privilegeName, int amount = 1, TokenModel tokenModel = null);
    Task<IEnumerable<Privilege>> GetPrivilegesForPlanAsync(Guid planId, TokenModel tokenModel);
    Task<JsonModel> GetAllPrivilegesAsync(TokenModel tokenModel);
}
