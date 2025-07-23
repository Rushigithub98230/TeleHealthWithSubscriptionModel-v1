using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Services;

public class VideoCallSubscriptionService : IVideoCallSubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IConsultationRepository _consultationRepository;
    private readonly IOpenTokService _openTokService;
    private readonly IBillingService _billingService;
    private readonly ILogger<VideoCallSubscriptionService> _logger;
    private readonly PrivilegeService _privilegeService;

    public VideoCallSubscriptionService(
        ISubscriptionRepository subscriptionRepository,
        IConsultationRepository consultationRepository,
        IOpenTokService openTokService,
        IBillingService billingService,
        ILogger<VideoCallSubscriptionService> logger,
        PrivilegeService privilegeService)
    {
        _subscriptionRepository = subscriptionRepository;
        _consultationRepository = consultationRepository;
        _openTokService = openTokService;
        _billingService = billingService;
        _logger = logger;
        _privilegeService = privilegeService;
    }

    public async Task<ApiResponse<VideoCallAccessDto>> CheckVideoCallAccessAsync(Guid userId, Guid? consultationId = null)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(userId);
            if (subscription == null)
            {
                return ApiResponse<VideoCallAccessDto>.ErrorResponse("No active subscription found", 403);
            }
            // Check if subscription plan includes video calls (privilege)
            var videoCallRemaining = await _privilegeService.GetRemainingPrivilegeAsync(subscription.Id, "VideoCall");
            if (videoCallRemaining <= 0)
            {
                return ApiResponse<VideoCallAccessDto>.ErrorResponse("Video calls not included in current plan or limit reached", 403);
            }
            // Check consultation limits (privilege)
            var consultationRemaining = await _privilegeService.GetRemainingPrivilegeAsync(subscription.Id, "Teleconsultation");
            if (consultationRemaining == 0)
            {
                return ApiResponse<VideoCallAccessDto>.ErrorResponse("Consultation limit reached for current billing period", 403);
            }
            // Check if this is a one-time consultation
            if (consultationId.HasValue)
            {
                var consultation = await _consultationRepository.GetByIdAsync(consultationId.Value);
                if (consultation != null && consultation.IsOneTime)
                {
                    return await CheckOneTimeVideoCallAccessAsync(consultation);
                }
            }
            var accessDto = new VideoCallAccessDto
            {
                HasAccess = true,
                SubscriptionId = subscription.Id,
                PlanName = subscription.SubscriptionPlan.Name,
                // Remove ConsultationsUsed, ConsultationLimit, RemainingConsultations
                // Use privilege system for privilege usage reporting if needed
                MaxDurationMinutes = 60, // Default duration
                CanRecord = await _privilegeService.GetRemainingPrivilegeAsync(subscription.Id, "PrioritySupport") > 0,
                CanBroadcast = false // Only for premium plans, add as privilege if needed
            };
            return ApiResponse<VideoCallAccessDto>.SuccessResponse(accessDto, "Video call access granted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking video call access for user: {UserId}", userId);
            return ApiResponse<VideoCallAccessDto>.ErrorResponse("Failed to check video call access", 500);
        }
    }

    public async Task<ApiResponse<OpenTokSessionDto>> CreateVideoCallSessionAsync(Guid userId, Guid consultationId, string sessionName)
    {
        try
        {
            var accessResult = await CheckVideoCallAccessAsync(userId, consultationId);
            if (!accessResult.Success)
            {
                return ApiResponse<OpenTokSessionDto>.ErrorResponse(accessResult.Message, accessResult.StatusCode);
            }
            var consultation = await _consultationRepository.GetByIdAsync(consultationId);
            if (consultation == null)
            {
                return ApiResponse<OpenTokSessionDto>.ErrorResponse("Consultation not found", 404);
            }
            var sessionResult = await _openTokService.CreateSessionAsync(sessionName, false);
            if (!sessionResult.Success)
            {
                return ApiResponse<OpenTokSessionDto>.ErrorResponse("Failed to create video session", 500);
            }
            consultation.MeetingUrl = $"/video-call/{sessionResult.Data.SessionId}";
            consultation.MeetingId = sessionResult.Data.SessionId;
            await _consultationRepository.UpdateAsync(consultation);
            // Increment consultation usage (privilege)
            var subscription = await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(userId);
            if (subscription != null && !consultation.IsOneTime)
            {
                await _privilegeService.UsePrivilegeAsync(subscription.Id, "Teleconsultation");
            }
            _logger.LogInformation("Created video call session for consultation: {ConsultationId}, session: {SessionId}", consultationId, sessionResult.Data.SessionId);
            return sessionResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating video call session for user: {UserId}, consultation: {ConsultationId}", userId, consultationId);
            return ApiResponse<OpenTokSessionDto>.ErrorResponse("Failed to create video call session", 500);
        }
    }

    public async Task<ApiResponse<string>> GenerateVideoCallTokenAsync(Guid userId, string sessionId, OpenTokRole role)
    {
        try
        {
            // Check access
            var accessResult = await CheckVideoCallAccessAsync(userId);
            if (!accessResult.Success)
            {
                return ApiResponse<string>.ErrorResponse(accessResult.Message, accessResult.StatusCode);
            }

            // Generate token
            var tokenResult = await _openTokService.GenerateTokenAsync(sessionId, userId.ToString(), "User", role);
            
            if (!tokenResult.Success)
            {
                return ApiResponse<string>.ErrorResponse("Failed to generate video call token", 500);
            }

            _logger.LogInformation("Generated video call token for user: {UserId}, session: {SessionId}", userId, sessionId);
            return tokenResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating video call token for user: {UserId}, session: {SessionId}", userId, sessionId);
            return ApiResponse<string>.ErrorResponse("Failed to generate video call token", 500);
        }
    }

    public async Task<ApiResponse<VideoCallBillingDto>> ProcessVideoCallBillingAsync(Guid userId, Guid consultationId, int durationMinutes)
    {
        try
        {
            var consultation = await _consultationRepository.GetByIdAsync(consultationId);
            if (consultation == null)
            {
                return ApiResponse<VideoCallBillingDto>.ErrorResponse("Consultation not found", 404);
            }
            var subscription = await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(userId);
            decimal billingAmount = 0;
            if (consultation.IsOneTime)
            {
                billingAmount = consultation.Fee;
            }
            else if (subscription != null)
            {
                // Use privilege system to determine if included
                var consultationRemaining = await _privilegeService.GetRemainingPrivilegeAsync(subscription.Id, "Teleconsultation");
                if (consultationRemaining > 0)
                {
                    billingAmount = 0; // Included in subscription
                }
                else
                {
                    // Additional consultation fee (could be a privilege value or plan property)
                    billingAmount = 0; // Set as needed
                }
            }
            if (billingAmount > 0)
            {
                var billingDto = new CreateBillingRecordDto
                {
                    UserId = userId.ToString(),
                    SubscriptionId = subscription?.Id.ToString() ?? string.Empty,
                    ConsultationId = consultationId.ToString(),
                    Amount = billingAmount,
                    Type = BillingRecord.BillingType.Consultation.ToString(),
                    Description = $"Video consultation - {durationMinutes} minutes",
                    DueDate = DateTime.UtcNow.AddDays(30)
                };
                var billingResult = await _billingService.CreateBillingRecordAsync(billingDto);
                if (!billingResult.Success)
                {
                    return ApiResponse<VideoCallBillingDto>.ErrorResponse("Failed to create billing record", 500);
                }
                var billingResponse = new VideoCallBillingDto
                {
                    BillingRecordId = Guid.TryParse(billingResult.Data.Id, out var billId) ? billId : (Guid?)null,
                    Amount = billingAmount,
                    DurationMinutes = durationMinutes,
                    IsIncludedInSubscription = billingAmount == 0,
                    ConsultationId = consultationId,
                    Description = $"Video consultation - {durationMinutes} minutes"
                };
                return ApiResponse<VideoCallBillingDto>.SuccessResponse(billingResponse, "Billing processed successfully");
            }
            var noBillingResponse = new VideoCallBillingDto
            {
                Amount = 0,
                DurationMinutes = durationMinutes,
                IsIncludedInSubscription = true,
                ConsultationId = consultationId
            };
            return ApiResponse<VideoCallBillingDto>.SuccessResponse(noBillingResponse, "No billing required");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing video call billing for user: {UserId}, consultation: {ConsultationId}", userId, consultationId);
            return ApiResponse<VideoCallBillingDto>.ErrorResponse("Failed to process video call billing", 500);
        }
    }

    public async Task<ApiResponse<VideoCallUsageDto>> GetVideoCallUsageAsync(Guid userId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(userId);
            if (subscription == null)
            {
                return ApiResponse<VideoCallUsageDto>.ErrorResponse("No active subscription found", 404);
            }

            // Get consultations for current billing period
            var billingStartDate = subscription.NextBillingDate.AddMonths(-1);
            var consultations = await _consultationRepository.GetByUserIdAsync(userId);
            var currentPeriodConsultations = consultations
                .Where(c => c.CreatedAt >= billingStartDate && c.Status == Consultation.ConsultationStatus.Completed)
                .ToList();

            var usageDto = new VideoCallUsageDto
            {
                SubscriptionId = subscription.Id,
                PlanName = subscription.SubscriptionPlan.Name,
                // Remove ConsultationsUsed, ConsultationLimit, RemainingConsultations from VideoCallUsageDto
                // Use privilege system for reporting if needed
                CurrentBillingPeriodStart = billingStartDate,
                CurrentBillingPeriodEnd = subscription.NextBillingDate,
                TotalVideoCallsThisPeriod = currentPeriodConsultations.Count,
                TotalDurationThisPeriod = currentPeriodConsultations.Sum(c => c.DurationMinutes),
                AverageDurationMinutes = currentPeriodConsultations.Any() ? 
                    currentPeriodConsultations.Average(c => c.DurationMinutes) : 0
            };

            return ApiResponse<VideoCallUsageDto>.SuccessResponse(usageDto, "Usage data retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video call usage for user: {UserId}", userId);
            return ApiResponse<VideoCallUsageDto>.ErrorResponse("Failed to get usage data", 500);
        }
    }

    private async Task<ApiResponse<VideoCallAccessDto>> CheckOneTimeVideoCallAccessAsync(Consultation consultation)
    {
        try
        {
            // Check if consultation is within allowed time window
            var timeWindow = TimeSpan.FromHours(2); // 2 hours before/after scheduled time
            var now = DateTime.UtcNow;
            var scheduledTime = consultation.ScheduledAt;
            
            if (now < scheduledTime.Subtract(timeWindow) || now > scheduledTime.Add(timeWindow))
            {
                return ApiResponse<VideoCallAccessDto>.ErrorResponse("Video call access outside allowed time window", 403);
            }

            // Check if consultation is paid
            if (consultation.Fee > 0)
            {
                // Check if payment is processed
                // This would typically check against billing records
                var isPaid = true; // Placeholder - implement actual payment check
                
                if (!isPaid)
                {
                    return ApiResponse<VideoCallAccessDto>.ErrorResponse("Consultation payment required", 402);
                }
            }

            var accessDto = new VideoCallAccessDto
            {
                HasAccess = true,
                ConsultationId = consultation.Id,
                PlanName = "One-time Consultation",
                // Remove ConsultationsUsed, ConsultationLimit, RemainingConsultations
                // Use privilege system for privilege usage reporting if needed
                MaxDurationMinutes = consultation.DurationMinutes,
                CanRecord = false,
                CanBroadcast = false,
                IsOneTime = true
            };

            return ApiResponse<VideoCallAccessDto>.SuccessResponse(accessDto, "One-time video call access granted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking one-time video call access for consultation: {ConsultationId}", consultation.Id);
            return ApiResponse<VideoCallAccessDto>.ErrorResponse("Failed to check one-time video call access", 500);
        }
    }
} 