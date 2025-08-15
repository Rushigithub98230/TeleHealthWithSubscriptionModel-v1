using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IAuditService
    {
        Task<JsonModel> GetAuditLogByIdAsync(Guid id);
        Task<JsonModel> CreateAuditLogAsync(CreateAuditLogDto createDto);
        Task<JsonModel> GetUserAuditLogsAsync(string userId);
        Task<JsonModel> SearchAuditLogsAsync(AuditLogSearchDto searchDto);
        Task<JsonModel> GetRecentAuditLogsAsync(int count = 100);
        Task<JsonModel> GetAuditLogsAsync(string? action = null, string? userId = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 50);
        Task<JsonModel> GetUserAuditLogCountAsync(string userId);
        
        // Business-specific audit methods
        Task LogUserActionAsync(string userId, string action, string entityType, string? entityId = null, string? description = null);
        Task LogDataChangeAsync(string userId, string entityType, string entityId, string? oldValues = null, string? newValues = null);
        Task LogSecurityEventAsync(string userId, string action, string? description = null, string? ipAddress = null);
        Task LogPaymentEventAsync(string userId, string action, string? entityId = null, string? status = null, string? errorMessage = null);
        Task LogSubscriptionEventAsync(string userId, string action, string? subscriptionId = null, string? status = null);
        Task LogConsultationEventAsync(string userId, string action, string? consultationId = null, string? status = null);
        Task LogActionAsync(string entity, string action, string entityId, string description);
    }
} 