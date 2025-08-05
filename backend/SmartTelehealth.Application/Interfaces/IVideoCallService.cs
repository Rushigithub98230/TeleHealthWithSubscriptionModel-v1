using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IVideoCallService
{
    Task<ApiResponse<VideoCallDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<IEnumerable<VideoCallDto>>> GetByUserIdAsync(Guid userId);
    Task<ApiResponse<VideoCallDto>> CreateAsync(CreateVideoCallDto createDto);
    Task<ApiResponse<VideoCallDto>> UpdateAsync(Guid id, UpdateVideoCallDto updateDto);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
    Task<ApiResponse<IEnumerable<VideoCallDto>>> GetAllAsync();
    
    // Video Call Management
    Task<ApiResponse<VideoCallDto>> InitiateVideoCallAsync(CreateVideoCallDto createDto);
    Task<ApiResponse<bool>> JoinVideoCallAsync(Guid callId, Guid userId);
    Task<ApiResponse<bool>> LeaveVideoCallAsync(Guid callId, Guid userId);
    Task<ApiResponse<bool>> EndVideoCallAsync(Guid callId, string? reason = null);
    Task<ApiResponse<bool>> RejectVideoCallAsync(Guid callId, string reason);
    
    // Video/Audio Controls
    Task<ApiResponse<bool>> ToggleVideoAsync(Guid callId, bool enabled);
    Task<ApiResponse<bool>> ToggleAudioAsync(Guid callId, bool enabled);
    Task<ApiResponse<bool>> StartScreenSharingAsync(Guid callId);
    Task<ApiResponse<bool>> StopScreenSharingAsync(Guid callId);
    
    // Call Quality and Participants
    Task<ApiResponse<bool>> UpdateCallQualityAsync(Guid callId, int audioQuality, int videoQuality, int networkQuality);
    Task<ApiResponse<IEnumerable<VideoCallParticipantDto>>> GetVideoCallParticipantsAsync(Guid callId);
    
    // Logging
    Task<ApiResponse<bool>> LogVideoCallEventAsync(Guid callId, LogVideoCallEventDto eventDto);
} 