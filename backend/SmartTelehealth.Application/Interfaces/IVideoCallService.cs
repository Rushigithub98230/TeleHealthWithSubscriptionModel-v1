using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IVideoCallService
{
    Task<JsonModel> GetByIdAsync(Guid id);
    Task<JsonModel> GetByUserIdAsync(int userId);
    Task<JsonModel> CreateAsync(CreateVideoCallDto createDto);
    Task<JsonModel> UpdateAsync(Guid id, UpdateVideoCallDto updateDto);
    Task<JsonModel> DeleteAsync(Guid id);
    Task<JsonModel> GetAllAsync();
    
    // Video Call Management
    Task<JsonModel> InitiateVideoCallAsync(CreateVideoCallDto createDto);
    Task<JsonModel> JoinVideoCallAsync(Guid callId, int userId);
    Task<JsonModel> LeaveVideoCallAsync(Guid callId, int userId);
    Task<JsonModel> EndVideoCallAsync(Guid callId, string? reason = null);
    Task<JsonModel> RejectVideoCallAsync(Guid callId, string reason);
    
    // Video/Audio Controls
    Task<JsonModel> ToggleVideoAsync(Guid callId, bool enabled);
    Task<JsonModel> ToggleAudioAsync(Guid callId, bool enabled);
    Task<JsonModel> StartScreenSharingAsync(Guid callId);
    Task<JsonModel> StopScreenSharingAsync(Guid callId);
    
    // Call Quality and Participants
    Task<JsonModel> UpdateCallQualityAsync(Guid callId, int audioQuality, int videoQuality, int networkQuality);
    Task<JsonModel> GetVideoCallParticipantsAsync(Guid callId);
    
    // Logging
    Task<JsonModel> LogVideoCallEventAsync(Guid callId, LogVideoCallEventDto eventDto);
} 