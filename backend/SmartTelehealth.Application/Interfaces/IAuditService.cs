using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IAuditService
    {
        Task<JsonModel> GetAuditLogByIdAsync(Guid id, TokenModel tokenModel);
        Task<JsonModel> CreateAuditLogAsync(CreateAuditLogDto createDto, TokenModel tokenModel);
        Task<JsonModel> GetUserAuditLogsAsync(string userId, TokenModel tokenModel);
        Task<JsonModel> SearchAuditLogsAsync(AuditLogSearchDto searchDto, TokenModel tokenModel);
        Task<JsonModel> GetRecentAuditLogsAsync(int count, TokenModel tokenModel);
        Task<JsonModel> GetAuditLogsAsync(string? action, string? userId, DateTime? startDate, DateTime? endDate, int page, int pageSize, TokenModel tokenModel);
        Task<JsonModel> GetUserAuditLogCountAsync(string userId, TokenModel tokenModel);
        
        // Business-specific audit methods
        Task LogUserActionAsync(string userId, string action, string entityType, string? entityId, string? description, TokenModel tokenModel);
        Task LogDataChangeAsync(string userId, string entityType, string entityId, string? oldValues, string? newValues, TokenModel tokenModel);
        Task LogSecurityEventAsync(string userId, string action, string? description, string? ipAddress, TokenModel tokenModel);
        Task LogPaymentEventAsync(string userId, string action, string? entityId, string? status, string? errorMessage, TokenModel tokenModel);
        Task LogSubscriptionEventAsync(string userId, string action, string? subscriptionId, string? status, TokenModel tokenModel);
        Task LogConsultationEventAsync(string userId, string action, string? consultationId, string? status, TokenModel tokenModel);
        Task LogActionAsync(string entity, string action, string entityId, string description, TokenModel tokenModel);
    }
} 