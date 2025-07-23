using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditService> _logger;

        public AuditService(
            IAuditLogRepository auditLogRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuditService> logger)
        {
            _auditLogRepository = auditLogRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ApiResponse<AuditLogDto>> GetAuditLogByIdAsync(Guid id)
        {
            try
            {
                var auditLog = await _auditLogRepository.GetByIdAsync(id);
                if (auditLog == null)
                    return ApiResponse<AuditLogDto>.ErrorResponse("Audit log not found", 404);
                
                var dto = _mapper.Map<AuditLogDto>(auditLog);
                return ApiResponse<AuditLogDto>.SuccessResponse(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit log {Id}", id);
                return ApiResponse<AuditLogDto>.ErrorResponse("An error occurred while retrieving the audit log", 500);
            }
        }

        public async Task<ApiResponse<AuditLogDto>> CreateAuditLogAsync(CreateAuditLogDto createDto)
        {
            try
            {
                var auditLog = _mapper.Map<AuditLog>(createDto);
                auditLog.Timestamp = DateTime.UtcNow;

                // Add HTTP context information
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    auditLog.IpAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                    auditLog.UserAgent = httpContext.Request.Headers["User-Agent"].ToString();
                }

                var createdLog = await _auditLogRepository.CreateAsync(auditLog);
                
                // Also log to file for debugging
                _logger.LogInformation("AUDIT: {Action} by {UserId} on {EntityType} {EntityId} - {Description}",
                    auditLog.Action, auditLog.UserId, auditLog.EntityType, auditLog.EntityId, auditLog.Description);

                var dto = _mapper.Map<AuditLogDto>(createdLog);
                return ApiResponse<AuditLogDto>.SuccessResponse(dto, "Audit log created successfully", 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create audit log: {Action} by {UserId}", createDto.Action, createDto.UserId);
                return ApiResponse<AuditLogDto>.ErrorResponse("An error occurred while creating the audit log", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<AuditLogDto>>> GetUserAuditLogsAsync(string userId)
        {
            try
            {
                var logs = await _auditLogRepository.GetByUserIdAsync(userId);
                var dtos = _mapper.Map<IEnumerable<AuditLogDto>>(logs);
                return ApiResponse<IEnumerable<AuditLogDto>>.SuccessResponse(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit logs for user {UserId}", userId);
                return ApiResponse<IEnumerable<AuditLogDto>>.ErrorResponse("An error occurred while retrieving audit logs", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<AuditLogDto>>> SearchAuditLogsAsync(AuditLogSearchDto searchDto)
        {
            try
            {
                var logs = await _auditLogRepository.SearchAsync(searchDto.SearchTerm ?? "");
                var dtos = _mapper.Map<IEnumerable<AuditLogDto>>(logs);
                return ApiResponse<IEnumerable<AuditLogDto>>.SuccessResponse(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching audit logs");
                return ApiResponse<IEnumerable<AuditLogDto>>.ErrorResponse("An error occurred while searching audit logs", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<AuditLogDto>>> GetRecentAuditLogsAsync(int count = 100)
        {
            try
            {
                var logs = await _auditLogRepository.GetRecentAsync(count);
                var dtos = _mapper.Map<IEnumerable<AuditLogDto>>(logs);
                return ApiResponse<IEnumerable<AuditLogDto>>.SuccessResponse(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent audit logs");
                return ApiResponse<IEnumerable<AuditLogDto>>.ErrorResponse("An error occurred while retrieving recent audit logs", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<AuditLogDto>>> GetAuditLogsAsync(string? action = null, string? userId = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 50)
        {
            try
            {
                IEnumerable<AuditLog> logs;
                if (!string.IsNullOrEmpty(userId))
                {
                    logs = await _auditLogRepository.GetByUserIdAsync(userId);
                }
                else if (!string.IsNullOrEmpty(action))
                {
                    logs = await _auditLogRepository.GetByActionAsync(action);
                }
                else if (startDate.HasValue && endDate.HasValue)
                {
                    logs = await _auditLogRepository.GetByDateRangeAsync(startDate.Value, endDate.Value);
                }
                else
                {
                    logs = await _auditLogRepository.GetRecentAsync(pageSize * page);
                }

                // Further filter in-memory if needed
                if (!string.IsNullOrEmpty(action))
                {
                    logs = logs.Where(l => l.Action.Contains(action, StringComparison.OrdinalIgnoreCase));
                }
                if (startDate.HasValue)
                {
                    logs = logs.Where(l => l.Timestamp >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    logs = logs.Where(l => l.Timestamp <= endDate.Value);
                }

                // Pagination
                logs = logs.OrderByDescending(l => l.Timestamp)
                             .Skip((page - 1) * pageSize)
                             .Take(pageSize);

                var dtos = _mapper.Map<IEnumerable<AuditLogDto>>(logs);
                return ApiResponse<IEnumerable<AuditLogDto>>.SuccessResponse(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs");
                return ApiResponse<IEnumerable<AuditLogDto>>.ErrorResponse("An error occurred while retrieving audit logs", 500);
            }
        }

        public async Task<ApiResponse<int>> GetUserAuditLogCountAsync(string userId)
        {
            try
            {
                var count = await _auditLogRepository.GetCountByUserIdAsync(userId);
                return ApiResponse<int>.SuccessResponse(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit log count for user {UserId}", userId);
                return ApiResponse<int>.ErrorResponse("An error occurred while retrieving audit log count", 500);
            }
        }

        public async Task LogUserActionAsync(string userId, string action, string entityType, string? entityId = null, string? description = null)
        {
            var createDto = new CreateAuditLogDto
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId ?? "",
                UserId = userId,
                Description = description ?? ""
            };

            await CreateAuditLogAsync(createDto);
        }

        public async Task LogDataChangeAsync(string userId, string entityType, string entityId, string? oldValues = null, string? newValues = null)
        {
            var createDto = new CreateAuditLogDto
            {
                Action = "DataChange",
                EntityType = entityType,
                EntityId = entityId,
                UserId = userId,
                Description = $"Data changed for {entityType} {entityId}",
                OldValues = oldValues,
                NewValues = newValues
            };

            await CreateAuditLogAsync(createDto);
        }

        public async Task LogSecurityEventAsync(string userId, string action, string? description = null, string? ipAddress = null)
        {
            var createDto = new CreateAuditLogDto
            {
                Action = action,
                EntityType = "Security",
                UserId = userId,
                Description = description ?? "",
                IpAddress = ipAddress ?? ""
            };

            await CreateAuditLogAsync(createDto);
        }

        public async Task LogPaymentEventAsync(string userId, string action, string? entityId = null, string? status = null, string? errorMessage = null)
        {
            var createDto = new CreateAuditLogDto
            {
                Action = action,
                EntityType = "Payment",
                EntityId = entityId ?? "",
                UserId = userId,
                Description = $"Payment {action} for user {userId}"
            };

            await CreateAuditLogAsync(createDto);
        }

        public async Task LogSubscriptionEventAsync(string userId, string action, string? subscriptionId = null, string? status = null)
        {
            var createDto = new CreateAuditLogDto
            {
                Action = action,
                EntityType = "Subscription",
                EntityId = subscriptionId ?? "",
                UserId = userId,
                Description = $"Subscription {action} for user {userId}"
            };

            await CreateAuditLogAsync(createDto);
        }

        public async Task LogConsultationEventAsync(string userId, string action, string? consultationId = null, string? status = null)
        {
            var createDto = new CreateAuditLogDto
            {
                Action = action,
                EntityType = "Consultation",
                EntityId = consultationId ?? "",
                UserId = userId,
                Description = $"Consultation {action} for user {userId}"
            };

            await CreateAuditLogAsync(createDto);
        }
    }
} 