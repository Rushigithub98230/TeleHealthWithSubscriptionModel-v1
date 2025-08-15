using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VideoCallController : ControllerBase
{
    private readonly IOpenTokService _openTokService;
    private readonly IConsultationService _consultationService;
    private readonly ILogger<VideoCallController> _logger;

    public VideoCallController(
        IOpenTokService openTokService,
        IConsultationService consultationService,
        ILogger<VideoCallController> logger)
    {
        _openTokService = openTokService;
        _consultationService = consultationService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new video call session
    /// </summary>
    [HttpPost("sessions")]
    public async Task<ActionResult<JsonModel> CreateSession([FromBody] CreateVideoSessionDto createDto)
    {
        try
        {
            var sessionResult = await _openTokService.CreateSessionAsync(createDto.SessionName, createDto.IsArchived);
            
            if (!sessionResult.Success)
                return StatusCode(sessionResult.StatusCode, sessionResult);

            // Update consultation with meeting URL if consultation ID is provided
            if (createDto.ConsultationId.HasValue)
            {
                var meetingUrl = $"/video-call/{sessionResult.Data.SessionId}";
                // You would typically update the consultation with the meeting URL here
                _logger.LogInformation("Created video session for consultation: {ConsultationId}", createDto.ConsultationId.Value);
            }

            return Ok(sessionResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating video session");
            return StatusCode(500, JsonModel.ErrorResponse("Failed to create video session", 500));
        }
    }

    /// <summary>
    /// Generate a token for joining a video session
    /// </summary>
    [HttpPost("sessions/{sessionId}/token")]
    public async Task<ActionResult<JsonModel> GenerateToken(
        string sessionId, 
        [FromBody] GenerateTokenDto generateDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();

            var tokenResult = await _openTokService.GenerateTokenAsync(
                sessionId, 
                userId.ToString(), 
                userName, 
                generateDto.Role);

            if (!tokenResult.Success)
                return StatusCode(tokenResult.StatusCode, tokenResult);

            return Ok(tokenResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token for session: {SessionId}", sessionId);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to generate token", 500));
        }
    }

    /// <summary>
    /// Get session information
    /// </summary>
    [HttpGet("sessions/{sessionId}")]
    public async Task<ActionResult<JsonModel> GetSession(string sessionId)
    {
        try
        {
            var sessionResult = await _openTokService.GetSessionAsync(sessionId);
            
            if (!sessionResult.Success)
                return StatusCode(sessionResult.StatusCode, sessionResult);

            return Ok(sessionResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session: {SessionId}", sessionId);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to retrieve session", 500));
        }
    }

    /// <summary>
    /// Start recording a video session
    /// </summary>
    [HttpPost("sessions/{sessionId}/recordings")]
    public async Task<ActionResult<JsonModel> StartRecording(
        string sessionId, 
        [FromBody] StartRecordingDto recordingDto)
    {
        try
        {
            var options = new OpenTokRecordingOptions
            {
                Name = recordingDto.Name,
                HasAudio = recordingDto.HasAudio,
                HasVideo = recordingDto.HasVideo,
                OutputMode = recordingDto.OutputMode,
                Resolution = recordingDto.Resolution,
                Layout = recordingDto.Layout,
                MaxDuration = recordingDto.MaxDuration
            };

            var recordingResult = await _openTokService.StartRecordingAsync(sessionId, options);
            
            if (!recordingResult.Success)
                return StatusCode(recordingResult.StatusCode, recordingResult);

            return Ok(recordingResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting recording for session: {SessionId}", sessionId);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to start recording", 500));
        }
    }

    /// <summary>
    /// Stop recording a video session
    /// </summary>
    [HttpPost("recordings/{recordingId}/stop")]
    public async Task<ActionResult<JsonModel> StopRecording(string recordingId)
    {
        try
        {
            var stopResult = await _openTokService.StopRecordingAsync(recordingId);
            
            if (!stopResult.Success)
                return StatusCode(stopResult.StatusCode, stopResult);

            return Ok(stopResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping recording: {RecordingId}", recordingId);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to stop recording", 500));
        }
    }

    /// <summary>
    /// Get session recordings
    /// </summary>
    [HttpGet("sessions/{sessionId}/recordings")]
    public async Task<ActionResult<JsonModel>> GetSessionRecordings(string sessionId)
    {
        try
        {
            var recordingsResult = await _openTokService.GetSessionRecordingsAsync(sessionId);
            
            if (!recordingsResult.Success)
                return StatusCode(recordingsResult.StatusCode, recordingsResult);

            return Ok(recordingsResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recordings for session: {SessionId}", sessionId);
            return StatusCode(500, JsonModel>.ErrorResponse("Failed to retrieve recordings", 500));
        }
    }

    /// <summary>
    /// Start broadcasting a video session
    /// </summary>
    [HttpPost("sessions/{sessionId}/broadcasts")]
    public async Task<ActionResult<JsonModel> StartBroadcast(
        string sessionId, 
        [FromBody] StartBroadcastDto broadcastDto)
    {
        try
        {
            var options = new OpenTokBroadcastOptions
            {
                Name = broadcastDto.Name,
                HlsUrl = broadcastDto.HlsUrl,
                RtmpUrl = broadcastDto.RtmpUrl,
                MaxDuration = broadcastDto.MaxDuration,
                Resolution = broadcastDto.Resolution,
                Layout = broadcastDto.Layout
            };

            var broadcastResult = await _openTokService.StartBroadcastAsync(sessionId, options);
            
            if (!broadcastResult.Success)
                return StatusCode(broadcastResult.StatusCode, broadcastResult);

            return Ok(broadcastResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting broadcast for session: {SessionId}", sessionId);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to start broadcast", 500));
        }
    }

    /// <summary>
    /// Stop broadcasting a video session
    /// </summary>
    [HttpPost("broadcasts/{broadcastId}/stop")]
    public async Task<ActionResult<JsonModel> StopBroadcast(string broadcastId)
    {
        try
        {
            var stopResult = await _openTokService.StopBroadcastAsync(broadcastId);
            
            if (!stopResult.Success)
                return StatusCode(stopResult.StatusCode, stopResult);

            return Ok(stopResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping broadcast: {BroadcastId}", broadcastId);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to stop broadcast", 500));
        }
    }

    /// <summary>
    /// Get session analytics
    /// </summary>
    [HttpGet("sessions/{sessionId}/analytics")]
    public async Task<ActionResult<JsonModel> GetSessionAnalytics(string sessionId)
    {
        try
        {
            var analyticsResult = await _openTokService.GetSessionAnalyticsAsync(sessionId);
            
            if (!analyticsResult.Success)
                return StatusCode(analyticsResult.StatusCode, analyticsResult);

            return Ok(analyticsResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics for session: {SessionId}", sessionId);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to retrieve analytics", 500));
        }
    }

    /// <summary>
    /// Health check for video service
    /// </summary>
    [HttpGet("health")]
    public async Task<ActionResult<JsonModel> HealthCheck()
    {
        try
        {
            var healthResult = await _openTokService.IsServiceHealthyAsync();
            return Ok(healthResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Video service health check failed");
            return StatusCode(500, JsonModel.ErrorResponse("Video service is unhealthy", 500));
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private string GetCurrentUserName()
    {
        var nameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
        return nameClaim ?? "Unknown User";
    }
}

// DTOs for video call operations
public class CreateVideoSessionDto
{
    public string SessionName { get; set; } = string.Empty;
    public bool IsArchived { get; set; } = false;
    public Guid? ConsultationId { get; set; }
}

public class GenerateTokenDto
{
    public OpenTokRole Role { get; set; } = OpenTokRole.Publisher;
    public DateTime? ExpireTime { get; set; }
}

public class StartRecordingDto
{
    public string Name { get; set; } = string.Empty;
    public bool HasAudio { get; set; } = true;
    public bool HasVideo { get; set; } = true;
    public OpenTokRecordingOutputMode OutputMode { get; set; } = OpenTokRecordingOutputMode.Composed;
    public string? Resolution { get; set; } = "1280x720";
    public string? Layout { get; set; }
    public int? MaxDuration { get; set; }
}

public class StartBroadcastDto
{
    public string Name { get; set; } = string.Empty;
    public string HlsUrl { get; set; } = string.Empty;
    public string? RtmpUrl { get; set; }
    public int? MaxDuration { get; set; }
    public string? Resolution { get; set; } = "1280x720";
    public string? Layout { get; set; }
} 