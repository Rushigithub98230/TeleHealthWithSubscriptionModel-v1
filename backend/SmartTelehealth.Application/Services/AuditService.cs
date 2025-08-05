using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartTelehealth.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditService> _logger;
        private readonly string _encryptionKey;

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
            _encryptionKey = Environment.GetEnvironmentVariable("AUDIT_ENCRYPTION_KEY") ?? "default-encryption-key-change-in-production";
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

                // Encrypt sensitive data for payment events
                if (auditLog.EntityType == "Payment")
                {
                    auditLog.Description = EncryptSensitiveData(auditLog.Description);
                    if (!string.IsNullOrEmpty(auditLog.EntityId))
                    {
                        auditLog.EntityId = EncryptSensitiveData(auditLog.EntityId);
                    }
                }

                var createdLog = await _auditLogRepository.CreateAsync(auditLog);
                
                // Log to file with sanitized data
                _logger.LogInformation("AUDIT: {Action} by {UserId} on {EntityType} - {SanitizedDescription}",
                    auditLog.Action, auditLog.UserId, auditLog.EntityType, SanitizeDescription(auditLog.Description));

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
            // Sanitize payment data for logging
            var sanitizedEntityId = SanitizePaymentData(entityId);
            var sanitizedErrorMessage = SanitizePaymentData(errorMessage);
            
            var createDto = new CreateAuditLogDto
            {
                Action = action,
                EntityType = "Payment",
                EntityId = sanitizedEntityId ?? "",
                UserId = userId,
                Description = $"Payment {action} for user {userId}",
                Status = status,
                ErrorMessage = sanitizedErrorMessage
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

        public async Task LogActionAsync(string entity, string action, string entityId, string description)
        {
            var createDto = new CreateAuditLogDto
            {
                Action = action,
                EntityType = entity,
                EntityId = entityId,
                UserId = GetCurrentUserId(), // fallback to empty if not available
                Description = description
            };
            await CreateAuditLogAsync(createDto);
        }

        // Helper to get userId from context (if available)
        private string GetCurrentUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
            return userId ?? string.Empty;
        }

        // PCI-Compliant Payment Data Sanitization
        private string? SanitizePaymentData(string? data)
        {
            if (string.IsNullOrEmpty(data)) return data;

            // Remove or mask sensitive payment information
            var sanitized = data
                .Replace(Regex.Replace(data, @"\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b", "****-****-****-****"), "") // Credit card numbers
                .Replace(Regex.Replace(data, @"\b\d{3}-\d{2}-\d{4}\b", "***-**-****"), "") // SSN
                .Replace(Regex.Replace(data, @"\b\d{3}\d{2}\d{4}\b", "******"), ""); // SSN without dashes

            return sanitized;
        }

        // Encrypt sensitive data for storage
        private string EncryptSensitiveData(string data)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
                aes.IV = new byte[16];

                using var encryptor = aes.CreateEncryptor();
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var encryptedBytes = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
                
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting sensitive data");
                return "[ENCRYPTION_ERROR]";
            }
        }

        // Decrypt sensitive data for retrieval
        private string DecryptSensitiveData(string encryptedData)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
                aes.IV = new byte[16];

                using var decryptor = aes.CreateDecryptor();
                var encryptedBytes = Convert.FromBase64String(encryptedData);
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting sensitive data");
                return "[DECRYPTION_ERROR]";
            }
        }

        // Sanitize description for file logging
        private string SanitizeDescription(string description)
        {
            if (string.IsNullOrEmpty(description)) return description;

            // Remove sensitive patterns from log output
            return Regex.Replace(description, 
                @"(?:payment|card|account|routing|swift|iban|bic)\s*[:=]\s*\S+", 
                "[REDACTED]", 
                RegexOptions.IgnoreCase);
        }
    }
} 