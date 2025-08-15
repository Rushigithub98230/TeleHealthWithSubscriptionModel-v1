using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IVideoCallSubscriptionService
{
    /// <summary>
    /// Check if user has access to video calls based on their subscription
    /// </summary>
    Task<JsonModel> CheckVideoCallAccessAsync(int userId, Guid? consultationId = null);

    /// <summary>
    /// Create a video call session for a consultation
    /// </summary>
    Task<JsonModel> CreateVideoCallSessionAsync(int userId, Guid consultationId, string sessionName);

    /// <summary>
    /// Generate a token for joining a video call session
    /// </summary>
    Task<JsonModel> GenerateVideoCallTokenAsync(int userId, string sessionId, OpenTokRole role);

    /// <summary>
    /// Process billing for a video call consultation
    /// </summary>
    Task<JsonModel> ProcessVideoCallBillingAsync(int userId, Guid consultationId, int durationMinutes);

    /// <summary>
    /// Get video call usage statistics for a user
    /// </summary>
    Task<JsonModel> GetVideoCallUsageAsync(int userId);
} 