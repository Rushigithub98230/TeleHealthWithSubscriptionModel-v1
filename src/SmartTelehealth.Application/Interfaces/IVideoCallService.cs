using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IVideoCallService
{
    // Video call management
    Task<ApiResponse<VideoCallDto>> InitiateVideoCallAsync(CreateVideoCallDto createDto);
    Task<ApiResponse<VideoCallDto>> GetVideoCallAsync(Guid callId);
    Task<ApiResponse<VideoCallDto>> UpdateVideoCallStatusAsync(Guid callId, UpdateVideoCallStatusDto updateDto);
    Task<ApiResponse<bool>> EndVideoCallAsync(Guid callId, string? reason = null);
    Task<ApiResponse<bool>> RejectVideoCallAsync(Guid callId, string reason);
    Task<ApiResponse<bool>> MissVideoCallAsync(Guid callId);

    // Participant management
    Task<ApiResponse<bool>> JoinVideoCallAsync(Guid callId, Guid userId);
    Task<ApiResponse<bool>> LeaveVideoCallAsync(Guid callId, Guid userId);
    Task<ApiResponse<bool>> UpdateParticipantSettingsAsync(Guid callId, Guid userId, UpdateParticipantSettingsDto settingsDto);
    Task<ApiResponse<IEnumerable<VideoCallParticipantDto>>> GetVideoCallParticipantsAsync(Guid callId);

    // Call events and monitoring
    Task<ApiResponse<bool>> LogVideoCallEventAsync(Guid callId, LogVideoCallEventDto eventDto);
    Task<ApiResponse<IEnumerable<VideoCallEventDto>>> GetVideoCallEventsAsync(Guid callId);
    Task<ApiResponse<bool>> UpdateCallQualityAsync(Guid callId, Guid userId, UpdateCallQualityDto qualityDto);

    // Recording functionality
    Task<ApiResponse<bool>> StartRecordingAsync(Guid callId);
    Task<ApiResponse<bool>> StopRecordingAsync(Guid callId);
    Task<ApiResponse<string>> GetRecordingUrlAsync(Guid callId);

    // Screen sharing
    Task<ApiResponse<bool>> StartScreenSharingAsync(Guid callId, Guid userId);
    Task<ApiResponse<bool>> StopScreenSharingAsync(Guid callId, Guid userId);

    // Media controls
    Task<ApiResponse<bool>> ToggleVideoAsync(Guid callId, Guid userId, bool enabled);
    Task<ApiResponse<bool>> ToggleAudioAsync(Guid callId, Guid userId, bool enabled);
    Task<ApiResponse<bool>> ToggleMuteAsync(Guid callId, Guid userId, bool muted);

    // Call history and analytics
    Task<ApiResponse<IEnumerable<VideoCallDto>>> GetUserVideoCallHistoryAsync(Guid userId);
    Task<ApiResponse<IEnumerable<VideoCallDto>>> GetChatRoomVideoCallHistoryAsync(Guid chatRoomId);
    Task<ApiResponse<VideoCallAnalyticsDto>> GetVideoCallAnalyticsAsync(Guid callId);

    // Security and compliance
    Task<ApiResponse<bool>> ValidateVideoCallAccessAsync(Guid callId, Guid userId);
    Task<ApiResponse<bool>> IsVideoCallHIPAACompliantAsync(Guid callId);
    Task<ApiResponse<bool>> EncryptVideoCallDataAsync(Guid callId);

    // Health check
    Task<ApiResponse<bool>> IsVideoCallServiceHealthyAsync();
} 