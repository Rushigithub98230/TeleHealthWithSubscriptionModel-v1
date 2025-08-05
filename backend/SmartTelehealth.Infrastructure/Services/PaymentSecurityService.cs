using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace SmartTelehealth.Infrastructure.Services
{
    public class PaymentSecurityService : IPaymentSecurityService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<PaymentSecurityService> _logger;
        private readonly IAuditService _auditService;
        private readonly Dictionary<string, int> _paymentAttempts = new();
        private readonly object _lockObject = new object();

        public PaymentSecurityService(
            IMemoryCache cache,
            ILogger<PaymentSecurityService> logger,
            IAuditService auditService)
        {
            _cache = cache;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<bool> ValidatePaymentRequestAsync(string userId, string ipAddress, decimal amount)
        {
            try
            {
                // Check rate limiting
                if (!await CheckRateLimitAsync(userId, ipAddress))
                {
                    await _auditService.LogSecurityEventAsync(userId, "PaymentRateLimitExceeded", 
                        $"Rate limit exceeded for user {userId} from IP {ipAddress}");
                    return false;
                }

                // Check for suspicious activity
                if (await DetectSuspiciousActivityAsync(userId, ipAddress, amount))
                {
                    await _auditService.LogSecurityEventAsync(userId, "SuspiciousPaymentDetected", 
                        $"Suspicious payment activity detected for user {userId} from IP {ipAddress}");
                    return false;
                }

                // Check amount limits
                if (!await ValidateAmountLimitsAsync(userId, amount))
                {
                    await _auditService.LogSecurityEventAsync(userId, "PaymentAmountLimitExceeded", 
                        $"Amount limit exceeded for user {userId}: {amount}");
                    return false;
                }

                // Log successful validation
                await _auditService.LogSecurityEventAsync(userId, "PaymentRequestValidated", 
                    $"Payment request validated for user {userId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating payment request for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> CheckRateLimitAsync(string userId, string ipAddress)
        {
            var userKey = $"payment_attempts_user_{userId}";
            var ipKey = $"payment_attempts_ip_{ipAddress}";

            var userAttempts = await GetAttemptsAsync(userKey);
            var ipAttempts = await GetAttemptsAsync(ipKey);

            // Allow max 5 attempts per user per hour
            if (userAttempts >= 5)
            {
                _logger.LogWarning("Rate limit exceeded for user {UserId}: {Attempts} attempts", userId, userAttempts);
                return false;
            }

            // Allow max 10 attempts per IP per hour
            if (ipAttempts >= 10)
            {
                _logger.LogWarning("Rate limit exceeded for IP {IpAddress}: {Attempts} attempts", ipAddress, ipAttempts);
                return false;
            }

            // Increment counters
            await IncrementAttemptsAsync(userKey);
            await IncrementAttemptsAsync(ipKey);

            return true;
        }

        public async Task<bool> DetectSuspiciousActivityAsync(string userId, string ipAddress, decimal amount)
        {
            try
            {
                // Check for unusual payment patterns
                var userPaymentHistory = await GetUserPaymentHistoryAsync(userId);
                
                // Detect unusual amounts
                if (amount > 1000 && userPaymentHistory.AverageAmount < 100)
                {
                    _logger.LogWarning("Unusual payment amount detected for user {UserId}: {Amount}", userId, amount);
                    return true;
                }

                // Detect rapid successive payments
                var recentPayments = userPaymentHistory.RecentPayments;
                if (recentPayments.Count >= 3 && 
                    recentPayments.All(p => p.Timestamp > DateTime.UtcNow.AddMinutes(-5)))
                {
                    _logger.LogWarning("Rapid successive payments detected for user {UserId}", userId);
                    return true;
                }

                // Check for geographic anomalies
                if (await IsGeographicAnomalyAsync(userId, ipAddress))
                {
                    _logger.LogWarning("Geographic anomaly detected for user {UserId} from IP {IpAddress}", userId, ipAddress);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting suspicious activity for user {UserId}", userId);
                return false; // Fail open for now
            }
        }

        public async Task<bool> ValidateAmountLimitsAsync(string userId, decimal amount)
        {
            try
            {
                // Get user's payment history
                var userPaymentHistory = await GetUserPaymentHistoryAsync(userId);
                
                // Check daily limit
                var todayPayments = userPaymentHistory.RecentPayments
                    .Where(p => p.Timestamp.Date == DateTime.UtcNow.Date)
                    .Sum(p => p.Amount);

                if (todayPayments + amount > 5000) // $5000 daily limit
                {
                    _logger.LogWarning("Daily payment limit exceeded for user {UserId}: {TotalAmount}", userId, todayPayments + amount);
                    return false;
                }

                // Check single payment limit
                if (amount > 2000) // $2000 single payment limit
                {
                    _logger.LogWarning("Single payment limit exceeded for user {UserId}: {Amount}", userId, amount);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating amount limits for user {UserId}", userId);
                return false;
            }
        }

        public async Task LogPaymentAttemptAsync(string userId, string ipAddress, decimal amount, bool success, string? errorMessage = null)
        {
            try
            {
                var paymentAttempt = new PaymentAttemptLog
                {
                    UserId = userId,
                    IpAddress = ipAddress,
                    Amount = amount,
                    Success = success,
                    ErrorMessage = errorMessage,
                    Timestamp = DateTime.UtcNow
                };

                // Store in cache for quick access
                // var cacheKey = $"payment_attempt_{userId}_{DateTime.UtcNow.Ticks}";
                // _cache.Set(cacheKey, paymentAttempt, TimeSpan.FromHours(24));

                // Log to audit service
                await _auditService.LogSecurityEventAsync(userId, 
                    success ? "PaymentAttemptSuccess" : "PaymentAttemptFailed",
                    $"Payment attempt {(success ? "succeeded" : "failed")} for user {userId} from IP {ipAddress}",
                    ipAddress);

                _logger.LogInformation("Payment attempt logged: User={UserId}, IP={IpAddress}, Amount={Amount}, Success={Success}",
                    userId, ipAddress, amount, success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging payment attempt for user {UserId}", userId);
            }
        }

        public async Task<PaymentSecurityReportDto> GenerateSecurityReportAsync(string userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var attempts = await GetUserPaymentAttemptsAsync(userId, startDate, endDate);
                
                var report = new PaymentSecurityReportDto
                {
                    UserId = userId,
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalPaymentAttempts = attempts.Count,
                    SuccessfulPayments = attempts.Count(a => a.Success),
                    FailedPayments = attempts.Count(a => !a.Success),
                    SuspiciousActivities = attempts.Count(a => a.Amount > 1000), // Simple heuristic
                    RateLimitViolations = 0, // Would be calculated from actual rate limiting data
                    AverageAmount = attempts.Any() ? attempts.Average(a => a.Amount) : 0,
                    RiskScore = CalculateRiskScore(attempts).ToString()
                };

                await _auditService.LogSecurityEventAsync(userId, "SecurityReportGenerated", 
                    $"Security report generated for user {userId} from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating security report for user {UserId}", userId);
                return new PaymentSecurityReportDto
                {
                    UserId = userId,
                    StartDate = startDate,
                    EndDate = endDate,
                    RiskScore = "100" // High risk if error occurs
                };
            }
        }

        // Private helper methods
        private async Task<int> GetAttemptsAsync(string key)
        {
            var cachedValue = _cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return "0";
            });
            
            return int.TryParse(cachedValue, out var attempts) ? attempts : 0;
        }

        private async Task IncrementAttemptsAsync(string key)
        {
            lock (_lockObject)
            {
                var cachedValue = _cache.GetOrCreate(key, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                    return "0";
                });

                var attempts = int.TryParse(cachedValue, out var currentAttempts) ? currentAttempts : 0;
                _cache.Set(key, (attempts + 1).ToString(), TimeSpan.FromHours(1));
            }
        }

        private async Task<UserPaymentHistoryDto> GetUserPaymentHistoryAsync(string userId)
        {
            // This would typically query the database
            // For now, return mock data
            return new UserPaymentHistoryDto
            {
                UserId = userId,
                AverageAmount = 150.00m,
                RecentPayments = new List<PaymentHistoryItemDto>
                {
                    new PaymentHistoryItemDto { Amount = 100.00m, Timestamp = DateTime.UtcNow.AddHours(-2) },
                    new PaymentHistoryItemDto { Amount = 200.00m, Timestamp = DateTime.UtcNow.AddHours(-1) }
                }
            };
        }

        private async Task<bool> IsGeographicAnomalyAsync(string userId, string ipAddress)
        {
            // This would typically use a geolocation service
            // For now, return false (no anomaly detected)
            return false;
        }

        private async Task<List<PaymentAttemptLog>> GetUserPaymentAttemptsAsync(string userId, DateTime startDate, DateTime endDate)
        {
            // This would typically query the database
            // For now, return mock data
            return new List<PaymentAttemptLog>
            {
                new PaymentAttemptLog { UserId = userId, Amount = 100.00m, Success = true, Timestamp = DateTime.UtcNow.AddHours(-1) },
                new PaymentAttemptLog { UserId = userId, Amount = 200.00m, Success = false, Timestamp = DateTime.UtcNow.AddHours(-2) }
            };
        }

        private int CalculateRiskScore(List<PaymentAttemptLog> attempts)
        {
            if (!attempts.Any()) return 0;

            var score = 0;
            
            // Failed attempts increase risk
            score += attempts.Count(a => !a.Success) * 10;
            
            // High amounts increase risk
            score += attempts.Count(a => a.Amount > 500) * 5;
            
            // Rapid attempts increase risk
            var rapidAttempts = attempts.Where(a => a.Timestamp > DateTime.UtcNow.AddMinutes(-5)).Count();
            score += rapidAttempts * 15;

            return Math.Min(score, 100); // Cap at 100
        }
    }

    // DTOs for payment security
    public class PaymentAttemptLog
    {
        public string UserId { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class UserPaymentHistoryDto
    {
        public string UserId { get; set; } = string.Empty;
        public decimal AverageAmount { get; set; }
        public List<PaymentHistoryItemDto> RecentPayments { get; set; } = new();
    }

    public class PaymentHistoryItemDto
    {
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

namespace SmartTelehealth.Application.Interfaces
{
    public interface IPaymentSecurityService
    {
        Task<bool> ValidatePaymentRequestAsync(string userId, string ipAddress, decimal amount);
        Task<bool> CheckRateLimitAsync(string userId, string ipAddress);
        Task<bool> DetectSuspiciousActivityAsync(string userId, string ipAddress, decimal amount);
        Task<bool> ValidateAmountLimitsAsync(string userId, decimal amount);
        Task LogPaymentAttemptAsync(string userId, string ipAddress, decimal amount, bool success, string? errorMessage = null);
        Task<PaymentSecurityReportDto> GenerateSecurityReportAsync(string userId, DateTime startDate, DateTime endDate);
    }
} 