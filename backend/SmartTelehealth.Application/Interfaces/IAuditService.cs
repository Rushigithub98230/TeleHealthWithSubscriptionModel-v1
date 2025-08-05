using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IAuditService
    {
        Task<ApiResponse<AuditLogDto>> GetAuditLogByIdAsync(Guid id);
        Task<ApiResponse<AuditLogDto>> CreateAuditLogAsync(CreateAuditLogDto createDto);
        Task<ApiResponse<IEnumerable<AuditLogDto>>> GetUserAuditLogsAsync(string userId);
        Task<ApiResponse<IEnumerable<AuditLogDto>>> SearchAuditLogsAsync(AuditLogSearchDto searchDto);
        Task<ApiResponse<IEnumerable<AuditLogDto>>> GetRecentAuditLogsAsync(int count = 100);
        Task<ApiResponse<IEnumerable<AuditLogDto>>> GetAuditLogsAsync(string? action = null, string? userId = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 50);
        Task<ApiResponse<int>> GetUserAuditLogCountAsync(string userId);
        
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