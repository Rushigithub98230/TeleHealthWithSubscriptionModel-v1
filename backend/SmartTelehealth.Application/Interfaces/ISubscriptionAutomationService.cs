using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface ISubscriptionAutomationService
{
    Task<JsonModel> TriggerBillingAsync();
    Task<JsonModel> RenewSubscriptionAsync(string subscriptionId);
    Task<JsonModel> ChangePlanAsync(string subscriptionId, ChangePlanRequest request);
    Task<JsonModel> ProcessAutomatedRenewalsAsync();
    Task<JsonModel> ProcessExpiredSubscriptionsAsync();
    
    // Automation status and logging methods
    Task<JsonModel> GetAutomationStatusAsync();
    Task<JsonModel> GetAutomationLogsAsync(int page = 1, int pageSize = 50);
}
