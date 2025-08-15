using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IVideoCallSubscriptionService
{
    /// <summary>
    /// Check if user has access to video calls based on their subscription
    /// </summary>
    Task<ApiResponse<VideoCallAccessDto>> CheckVideoCallAccessAsync(int userId, Guid? consultationId = null);

    /// <summary>
    /// Create a video call session for a consultation
    /// </summary>
    Task<ApiResponse<OpenTokSessionDto>> CreateVideoCallSessionAsync(int userId, Guid consultationId, string sessionName);

    /// <summary>
    /// Generate a token for joining a video call session
    /// </summary>
    Task<ApiResponse<string>> GenerateVideoCallTokenAsync(int userId, string sessionId, OpenTokRole role);

    /// <summary>
    /// Process billing for a video call consultation
    /// </summary>
    Task<ApiResponse<VideoCallBillingDto>> ProcessVideoCallBillingAsync(int userId, Guid consultationId, int durationMinutes);

    /// <summary>
    /// Get video call usage statistics for a user
    /// </summary>
    Task<ApiResponse<VideoCallUsageDto>> GetVideoCallUsageAsync(int userId);
} 