using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Services;

public class VideoCallService : IVideoCallService
{
    private readonly IVideoCallRepository _videoCallRepository;
    private readonly IOpenTokService _openTokService;
    private readonly ILogger<VideoCallService> _logger;

    public VideoCallService(
        IVideoCallRepository videoCallRepository,
        IOpenTokService openTokService,
        ILogger<VideoCallService> logger)
    {
        _videoCallRepository = videoCallRepository;
        _openTokService = openTokService;
        _logger = logger;
    }

    public async Task<ApiResponse<VideoCallDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(id);
            if (videoCall == null)
            {
                return ApiResponse<VideoCallDto>.ErrorResponse("Video call not found", 404);
            }

            var videoCallDto = MapToDto(videoCall);
            return ApiResponse<VideoCallDto>.SuccessResponse(videoCallDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video call with ID {VideoCallId}", id);
            return ApiResponse<VideoCallDto>.ErrorResponse("Error retrieving video call", 500);
        }
    }

    public async Task<ApiResponse<IEnumerable<VideoCallDto>>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            var videoCalls = await _videoCallRepository.GetByUserIdAsync(userId);
            var videoCallDtos = videoCalls.Select(MapToDto);
            return ApiResponse<IEnumerable<VideoCallDto>>.SuccessResponse(videoCallDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video calls for user {UserId}", userId);
            return ApiResponse<IEnumerable<VideoCallDto>>.ErrorResponse("Error retrieving video calls", 500);
        }
    }

    public async Task<ApiResponse<VideoCallDto>> CreateAsync(CreateVideoCallDto createDto)
    {
        try
        {
            var videoCall = new VideoCall
            {
                Id = Guid.NewGuid(),
                AppointmentId = createDto.AppointmentId,
                SessionId = createDto.SessionId ?? string.Empty,
                Token = createDto.Token ?? string.Empty,
                StartedAt = DateTime.UtcNow,
                Status = "Initiated",
                CreatedAt = DateTime.UtcNow
            };

            var createdVideoCall = await _videoCallRepository.CreateAsync(videoCall);
            var videoCallDto = MapToDto(createdVideoCall);
            return ApiResponse<VideoCallDto>.SuccessResponse(videoCallDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating video call");
            return ApiResponse<VideoCallDto>.ErrorResponse("Error creating video call", 500);
        }
    }

    public async Task<ApiResponse<VideoCallDto>> UpdateAsync(Guid id, UpdateVideoCallDto updateDto)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(id);
            if (videoCall == null)
            {
                return ApiResponse<VideoCallDto>.ErrorResponse("Video call not found", 404);
            }

            // Update properties
            videoCall.Status = updateDto.Status ?? videoCall.Status;
            videoCall.EndedAt = updateDto.EndedAt ?? videoCall.EndedAt;
            videoCall.RecordingUrl = updateDto.RecordingUrl ?? videoCall.RecordingUrl;

            var updatedVideoCall = await _videoCallRepository.UpdateAsync(videoCall);
            var videoCallDto = MapToDto(updatedVideoCall);
            return ApiResponse<VideoCallDto>.SuccessResponse(videoCallDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating video call with ID {VideoCallId}", id);
            return ApiResponse<VideoCallDto>.ErrorResponse("Error updating video call", 500);
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var result = await _videoCallRepository.DeleteAsync(id);
            return ApiResponse<bool>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting video call with ID {VideoCallId}", id);
            return ApiResponse<bool>.ErrorResponse("Error deleting video call", 500);
        }
    }

    public async Task<ApiResponse<IEnumerable<VideoCallDto>>> GetAllAsync()
    {
        try
        {
            var videoCalls = await _videoCallRepository.GetAllAsync();
            var videoCallDtos = videoCalls.Select(MapToDto);
            return ApiResponse<IEnumerable<VideoCallDto>>.SuccessResponse(videoCallDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all video calls");
            return ApiResponse<IEnumerable<VideoCallDto>>.ErrorResponse("Error retrieving video calls", 500);
        }
    }

    // Video Call Management
    public async Task<ApiResponse<VideoCallDto>> InitiateVideoCallAsync(CreateVideoCallDto createDto)
    {
        try
        {
            var result = await CreateAsync(createDto);
            if (result.Success)
            {
                // Create OpenTok session
                var sessionResult = await _openTokService.CreateSessionAsync($"Call_{result.Data.Id}", false);
                if (sessionResult.Success)
                {
                    result.Data.SessionId = sessionResult.Data.SessionId;
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating video call");
            return ApiResponse<VideoCallDto>.ErrorResponse("Error initiating video call", 500);
        }
    }

    public async Task<ApiResponse<bool>> JoinVideoCallAsync(Guid callId, Guid userId)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(callId);
            if (videoCall == null)
            {
                return ApiResponse<bool>.ErrorResponse("Video call not found", 404);
            }

            // Add participant logic here
            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining video call {CallId} by user {UserId}", callId, userId);
            return ApiResponse<bool>.ErrorResponse("Error joining video call", 500);
        }
    }

    public async Task<ApiResponse<bool>> LeaveVideoCallAsync(Guid callId, Guid userId)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(callId);
            if (videoCall == null)
            {
                return ApiResponse<bool>.ErrorResponse("Video call not found", 404);
            }

            // Remove participant logic here
            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving video call {CallId} by user {UserId}", callId, userId);
            return ApiResponse<bool>.ErrorResponse("Error leaving video call", 500);
        }
    }

    public async Task<ApiResponse<bool>> EndVideoCallAsync(Guid callId, string? reason = null)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(callId);
            if (videoCall == null)
            {
                return ApiResponse<bool>.ErrorResponse("Video call not found", 404);
            }

            videoCall.Status = "Ended";
            videoCall.EndedAt = DateTime.UtcNow;

            await _videoCallRepository.UpdateAsync(videoCall);
            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending video call {CallId}", callId);
            return ApiResponse<bool>.ErrorResponse("Error ending video call", 500);
        }
    }

    public async Task<ApiResponse<bool>> RejectVideoCallAsync(Guid callId, string reason)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(callId);
            if (videoCall == null)
            {
                return ApiResponse<bool>.ErrorResponse("Video call not found", 404);
            }

            videoCall.Status = "Rejected";
            videoCall.EndedAt = DateTime.UtcNow;

            await _videoCallRepository.UpdateAsync(videoCall);
            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting video call {CallId}", callId);
            return ApiResponse<bool>.ErrorResponse("Error rejecting video call", 500);
        }
    }

    // Video/Audio Controls
    public async Task<ApiResponse<bool>> ToggleVideoAsync(Guid callId, bool enabled)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(callId);
            if (videoCall == null)
            {
                return ApiResponse<bool>.ErrorResponse("Video call not found", 404);
            }

            // Video toggle logic would be handled by participants, not the call itself
            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling video for call {CallId}", callId);
            return ApiResponse<bool>.ErrorResponse("Error toggling video", 500);
        }
    }

    public async Task<ApiResponse<bool>> ToggleAudioAsync(Guid callId, bool enabled)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(callId);
            if (videoCall == null)
            {
                return ApiResponse<bool>.ErrorResponse("Video call not found", 404);
            }

            // Audio toggle logic would be handled by participants, not the call itself
            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling audio for call {CallId}", callId);
            return ApiResponse<bool>.ErrorResponse("Error toggling audio", 500);
        }
    }

    public async Task<ApiResponse<bool>> StartScreenSharingAsync(Guid callId)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(callId);
            if (videoCall == null)
            {
                return ApiResponse<bool>.ErrorResponse("Video call not found", 404);
            }

            // Screen sharing logic here
            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting screen sharing for call {CallId}", callId);
            return ApiResponse<bool>.ErrorResponse("Error starting screen sharing", 500);
        }
    }

    public async Task<ApiResponse<bool>> StopScreenSharingAsync(Guid callId)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(callId);
            if (videoCall == null)
            {
                return ApiResponse<bool>.ErrorResponse("Video call not found", 404);
            }

            // Stop screen sharing logic here
            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping screen sharing for call {CallId}", callId);
            return ApiResponse<bool>.ErrorResponse("Error stopping screen sharing", 500);
        }
    }

    // Call Quality and Participants
    public async Task<ApiResponse<bool>> UpdateCallQualityAsync(Guid callId, int audioQuality, int videoQuality, int networkQuality)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(callId);
            if (videoCall == null)
            {
                return ApiResponse<bool>.ErrorResponse("Video call not found", 404);
            }

            // Update call quality logic here
            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating call quality for call {CallId}", callId);
            return ApiResponse<bool>.ErrorResponse("Error updating call quality", 500);
        }
    }

    public async Task<ApiResponse<IEnumerable<VideoCallParticipantDto>>> GetVideoCallParticipantsAsync(Guid callId)
    {
        try
        {
            var videoCall = await _videoCallRepository.GetByIdAsync(callId);
            if (videoCall == null)
            {
                return ApiResponse<IEnumerable<VideoCallParticipantDto>>.ErrorResponse("Video call not found", 404);
            }

            // Get participants logic here
            var participants = new List<VideoCallParticipantDto>();
            return ApiResponse<IEnumerable<VideoCallParticipantDto>>.SuccessResponse(participants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting participants for call {CallId}", callId);
            return ApiResponse<IEnumerable<VideoCallParticipantDto>>.ErrorResponse("Error getting participants", 500);
        }
    }

    // Logging
    public async Task<ApiResponse<bool>> LogVideoCallEventAsync(Guid callId, LogVideoCallEventDto eventDto)
    {
        try
        {
            // Log video call event logic here
            _logger.LogInformation("Video call event: {EventType} for call {CallId} by user {UserId}", 
                eventDto.Type, callId, eventDto.UserId);
            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging video call event for call {CallId}", callId);
            return ApiResponse<bool>.ErrorResponse("Error logging video call event", 500);
        }
    }

    private VideoCallDto MapToDto(VideoCall videoCall)
    {
        return new VideoCallDto
        {
            Id = videoCall.Id,
            AppointmentId = videoCall.AppointmentId,
            SessionId = videoCall.SessionId,
            Token = videoCall.Token,
            StartedAt = videoCall.StartedAt,
            EndedAt = videoCall.EndedAt,
            Status = videoCall.Status,
            RecordingUrl = videoCall.RecordingUrl,
            CreatedAt = videoCall.CreatedAt,
            UpdatedAt = videoCall.UpdatedAt,
            IsDeleted = videoCall.IsDeleted
        };
    }
} 