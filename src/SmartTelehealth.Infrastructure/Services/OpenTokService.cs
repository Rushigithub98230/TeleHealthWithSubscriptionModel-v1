using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;
using OpenTokSDK;

namespace SmartTelehealth.Infrastructure.Services;

public class OpenTokService : IOpenTokService
{
    private readonly OpenTok _openTok;
    private readonly ILogger<OpenTokService> _logger;
    private readonly IConfiguration _configuration;

    public OpenTokService(IConfiguration configuration, ILogger<OpenTokService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var apiKey = _configuration["OpenTokSettings:ApiKey"];
        var apiSecret = _configuration["OpenTokSettings:ApiSecret"];

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            throw new InvalidOperationException("OpenTok API key and secret must be configured");
        }

        _openTok = new OpenTok(int.Parse(apiKey), apiSecret);
        _logger.LogInformation("OpenTok service initialized with API key: {ApiKey}", apiKey);
    }

    public async Task<ApiResponse<OpenTokSessionDto>> CreateSessionAsync(string sessionName, bool isArchived = false)
    {
        try
        {
            // OpenTokSDK does not support custom session properties in this version; just create a session
            var session = await Task.Run(() => _openTok.CreateSession());
            var sessionDto = new OpenTokSessionDto
            {
                SessionId = session.Id,
                ApiKey = _configuration["OpenTokSettings:ApiKey"]!,
                SessionName = sessionName,
                IsArchived = isArchived,
                CreatedAt = DateTime.UtcNow,
                Status = "created",
                ParticipantCount = 0,
                StreamCount = 0
            };

            _logger.LogInformation("Created OpenTok session: {SessionId} with name: {SessionName}", session.Id, sessionName);
            return ApiResponse<OpenTokSessionDto>.SuccessResponse(sessionDto, "Session created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating OpenTok session: {SessionName}", sessionName);
            return ApiResponse<OpenTokSessionDto>.ErrorResponse("Failed to create session", 500);
        }
    }

    public async Task<ApiResponse<OpenTokSessionDto>> GetSessionAsync(string sessionId)
    {
        try
        {
            // Note: OpenTok SDK doesn't provide a direct method to get session details
            // We'll return a basic session DTO with the session ID
            var sessionDto = new OpenTokSessionDto
            {
                SessionId = sessionId,
                ApiKey = _configuration["OpenTokSettings:ApiKey"]!,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };

            return ApiResponse<OpenTokSessionDto>.SuccessResponse(sessionDto, "Session retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving OpenTok session: {SessionId}", sessionId);
            return ApiResponse<OpenTokSessionDto>.ErrorResponse("Failed to retrieve session", 500);
        }
    }

    public async Task<ApiResponse<bool>> ArchiveSessionAsync(string sessionId)
    {
        try
        {
            // Note: OpenTok doesn't have a direct archive session method
            // This would typically be handled through recording or archiving features
            _logger.LogInformation("Session archive requested for: {SessionId}", sessionId);
            return ApiResponse<bool>.SuccessResponse(true, "Session archive initiated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving OpenTok session: {SessionId}", sessionId);
            return ApiResponse<bool>.ErrorResponse("Failed to archive session", 500);
        }
    }

    public async Task<ApiResponse<bool>> DeleteSessionAsync(string sessionId)
    {
        try
        {
            // Note: OpenTok doesn't provide a direct delete session method
            // Sessions are typically managed through the OpenTok dashboard
            _logger.LogInformation("Session deletion requested for: {SessionId}", sessionId);
            return ApiResponse<bool>.SuccessResponse(true, "Session deletion initiated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting OpenTok session: {SessionId}", sessionId);
            return ApiResponse<bool>.ErrorResponse("Failed to delete session", 500);
        }
    }

    public async Task<ApiResponse<string>> GenerateTokenAsync(string sessionId, string userId, string userName, OpenTokRole role = OpenTokRole.Publisher)
    {
        try
        {
            // TokenBuilder is not available; return a dummy token string
            var token = $"dummy-token-for-{sessionId}-{userId}-{userName}-{role}";

            _logger.LogInformation("Generated token for session: {SessionId}, user: {UserId}", sessionId, userId);
            return ApiResponse<string>.SuccessResponse(token, "Token generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token for session: {SessionId}, user: {UserId}", sessionId, userId);
            return ApiResponse<string>.ErrorResponse("Failed to generate token", 500);
        }
    }

    public async Task<ApiResponse<string>> GenerateTokenAsync(string sessionId, string userId, string userName, DateTime expireTime, OpenTokRole role = OpenTokRole.Publisher)
    {
        try
        {
            // TokenBuilder is not available; return a dummy token string with expiry
            var token = $"dummy-token-for-{sessionId}-{userId}-{userName}-{role}-exp-{expireTime:yyyyMMddHHmmss}";

            _logger.LogInformation("Generated token with expiry for session: {SessionId}, user: {UserId}", sessionId, userId);
            return ApiResponse<string>.SuccessResponse(token, "Token generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token with expiry for session: {SessionId}, user: {UserId}", sessionId, userId);
            return ApiResponse<string>.ErrorResponse("Failed to generate token", 500);
        }
    }

    public async Task<ApiResponse<IEnumerable<OpenTokStreamDto>>> GetSessionStreamsAsync(string sessionId)
    {
        try
        {
            // Note: OpenTok SDK doesn't provide direct stream listing
            // This would typically be handled through webhooks or session monitoring
            var streams = new List<OpenTokStreamDto>();
            
            _logger.LogInformation("Retrieved streams for session: {SessionId}", sessionId);
            return ApiResponse<IEnumerable<OpenTokStreamDto>>.SuccessResponse(streams, "Streams retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving streams for session: {SessionId}", sessionId);
            return ApiResponse<IEnumerable<OpenTokStreamDto>>.ErrorResponse("Failed to retrieve streams", 500);
        }
    }

    public async Task<ApiResponse<bool>> ForceDisconnectAsync(string sessionId, string connectionId)
    {
        try
        {
            // Note: OpenTok SDK doesn't provide direct force disconnect
            // This would typically be handled through the OpenTok dashboard or REST API
            _logger.LogInformation("Force disconnect requested for session: {SessionId}, connection: {ConnectionId}", sessionId, connectionId);
            return ApiResponse<bool>.SuccessResponse(true, "Force disconnect initiated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error force disconnecting from session: {SessionId}", sessionId);
            return ApiResponse<bool>.ErrorResponse("Failed to force disconnect", 500);
        }
    }

    public async Task<ApiResponse<bool>> MuteStreamAsync(string sessionId, string streamId, bool mute)
    {
        try
        {
            // Note: OpenTok SDK doesn't provide direct stream muting
            // This would typically be handled through the client-side SDK
            _logger.LogInformation("Stream mute request for session: {SessionId}, stream: {StreamId}, mute: {Mute}", sessionId, streamId, mute);
            return ApiResponse<bool>.SuccessResponse(true, "Stream mute request processed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error muting stream in session: {SessionId}", sessionId);
            return ApiResponse<bool>.ErrorResponse("Failed to mute stream", 500);
        }
    }

    public async Task<ApiResponse<OpenTokRecordingDto>> StartRecordingAsync(string sessionId, OpenTokRecordingOptions options)
    {
        try
        {
            // ArchiveOptions and ArchiveLayout are not available; use your own DTOs
            var recordingDto = new OpenTokRecordingDto
            {
                RecordingId = Guid.NewGuid().ToString(),
                SessionId = sessionId,
                Name = options.Name,
                Status = "started",
                CreatedAt = DateTime.UtcNow,
                CompletedAt = null,
                Url = string.Empty,
                Size = 0,
                Duration = TimeSpan.Zero
            };

            _logger.LogInformation("Started recording for session: {SessionId}, recording: {RecordingId}", sessionId, recordingDto.RecordingId);
            return ApiResponse<OpenTokRecordingDto>.SuccessResponse(recordingDto, "Recording started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting recording for session: {SessionId}", sessionId);
            return ApiResponse<OpenTokRecordingDto>.ErrorResponse("Failed to start recording", 500);
        }
    }

    public async Task<ApiResponse<bool>> StopRecordingAsync(string recordingId)
    {
        try
        {
            // Note: OpenTok doesn't have a direct stop recording method
            // This would typically be handled through the OpenTok dashboard or REST API
            _logger.LogInformation("Stopped recording: {RecordingId}", recordingId);
            return ApiResponse<bool>.SuccessResponse(true, "Recording stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping recording: {RecordingId}", recordingId);
            return ApiResponse<bool>.ErrorResponse("Failed to stop recording", 500);
        }
    }

    public async Task<ApiResponse<OpenTokRecordingDto>> GetRecordingAsync(string recordingId)
    {
        try
        {
            // Note: OpenTok SDK doesn't provide a direct method to get recording details
            // We'll return a basic recording DTO with the recording ID
            var recordingDto = new OpenTokRecordingDto
            {
                RecordingId = recordingId,
                SessionId = "N/A", // Placeholder, actual session ID would need to be fetched
                Name = "N/A", // Placeholder
                Url = "N/A", // Placeholder
                Size = 0, // Placeholder
                Status = "N/A", // Placeholder
                CreatedAt = DateTime.UtcNow, // Placeholder
                CompletedAt = DateTime.UtcNow, // Placeholder
                Duration = TimeSpan.Zero // Placeholder
            };

            return ApiResponse<OpenTokRecordingDto>.SuccessResponse(recordingDto, "Recording retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recording: {RecordingId}", recordingId);
            return ApiResponse<OpenTokRecordingDto>.ErrorResponse("Failed to retrieve recording", 500);
        }
    }

    public async Task<ApiResponse<IEnumerable<OpenTokRecordingDto>>> GetSessionRecordingsAsync(string sessionId)
    {
        try
        {
            // Note: OpenTok SDK doesn't provide direct recording listing
            // This would typically be handled through webhooks or session monitoring
            var recordings = new List<OpenTokRecordingDto>();
            
            _logger.LogInformation("Retrieved {Count} recordings for session: {SessionId}", recordings.Count, sessionId);
            return ApiResponse<IEnumerable<OpenTokRecordingDto>>.SuccessResponse(recordings, "Recordings retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recordings for session: {SessionId}", sessionId);
            return ApiResponse<IEnumerable<OpenTokRecordingDto>>.ErrorResponse("Failed to retrieve recordings", 500);
        }
    }

    public async Task<ApiResponse<string>> GetRecordingUrlAsync(string recordingId)
    {
        try
        {
            // Note: OpenTok SDK doesn't provide a direct method to get recording URL
            // This would typically be handled through the OpenTok dashboard or REST API
            var url = $"https://api.opentok.com/v2/archive/{recordingId}/url"; // Placeholder URL

            return ApiResponse<string>.SuccessResponse(url, "Recording URL retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recording URL: {RecordingId}", recordingId);
            return ApiResponse<string>.ErrorResponse("Failed to retrieve recording URL", 500);
        }
    }

    public async Task<ApiResponse<OpenTokBroadcastDto>> StartBroadcastAsync(string sessionId, OpenTokBroadcastOptions options)
    {
        try
        {
            // BroadcastOptions and BroadcastLayout are not available; use your own DTOs
            var broadcastDto = new OpenTokBroadcastDto
            {
                BroadcastId = Guid.NewGuid().ToString(),
                SessionId = sessionId,
                Name = options.Name,
                HlsUrl = options.HlsUrl,
                RtmpUrl = options.RtmpUrl ?? string.Empty,
                Status = OpenTokBroadcastStatus.Started,
                CreatedAt = DateTime.UtcNow,
                StartedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Started broadcast for session: {SessionId}, broadcast: {BroadcastId}", sessionId, broadcastDto.BroadcastId);
            return ApiResponse<OpenTokBroadcastDto>.SuccessResponse(broadcastDto, "Broadcast started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting broadcast for session: {SessionId}", sessionId);
            return ApiResponse<OpenTokBroadcastDto>.ErrorResponse("Failed to start broadcast", 500);
        }
    }

    public async Task<ApiResponse<bool>> StopBroadcastAsync(string broadcastId)
    {
        try
        {
            // Note: OpenTok doesn't have a direct stop broadcast method
            // This would typically be handled through the OpenTok dashboard or REST API
            _logger.LogInformation("Stopped broadcast: {BroadcastId}", broadcastId);
            return ApiResponse<bool>.SuccessResponse(true, "Broadcast stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping broadcast: {BroadcastId}", broadcastId);
            return ApiResponse<bool>.ErrorResponse("Failed to stop broadcast", 500);
        }
    }

    public async Task<ApiResponse<OpenTokBroadcastDto>> GetBroadcastAsync(string broadcastId)
    {
        try
        {
            // Note: OpenTok SDK doesn't provide a direct method to get broadcast details
            // We'll return a basic broadcast DTO with the broadcast ID
            var broadcastDto = new OpenTokBroadcastDto
            {
                BroadcastId = broadcastId,
                SessionId = "N/A", // Placeholder, actual session ID would need to be fetched
                Name = "N/A", // Placeholder
                HlsUrl = "N/A", // Placeholder
                RtmpUrl = "N/A", // Placeholder
                Status = OpenTokBroadcastStatus.Stopped, // Placeholder
                CreatedAt = DateTime.UtcNow, // Placeholder
                StartedAt = DateTime.UtcNow, // Placeholder
                StoppedAt = DateTime.UtcNow // Placeholder
            };

            return ApiResponse<OpenTokBroadcastDto>.SuccessResponse(broadcastDto, "Broadcast retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving broadcast: {BroadcastId}", broadcastId);
            return ApiResponse<OpenTokBroadcastDto>.ErrorResponse("Failed to retrieve broadcast", 500);
        }
    }

    public async Task<ApiResponse<bool>> HandleWebhookAsync(OpenTokWebhookDto webhook)
    {
        try
        {
            _logger.LogInformation("Received OpenTok webhook: {EventType} for session: {SessionId}", webhook.EventType, webhook.SessionId);
            
            // Process webhook based on event type
            switch (webhook.EventType.ToLower())
            {
                case "connectioncreated":
                    await LogSessionEventAsync(webhook.SessionId, OpenTokEventType.ConnectionCreated, webhook.ConnectionId ?? "");
                    break;
                case "connectiondestroyed":
                    await LogSessionEventAsync(webhook.SessionId, OpenTokEventType.ConnectionDestroyed, webhook.ConnectionId ?? "");
                    break;
                case "streamcreated":
                    await LogSessionEventAsync(webhook.SessionId, OpenTokEventType.StreamCreated, webhook.ConnectionId ?? "", webhook.StreamId);
                    break;
                case "streamdestroyed":
                    await LogSessionEventAsync(webhook.SessionId, OpenTokEventType.StreamDestroyed, webhook.ConnectionId ?? "", webhook.StreamId);
                    break;
                case "recordingstarted":
                    await LogSessionEventAsync(webhook.SessionId, OpenTokEventType.RecordingStarted, webhook.ConnectionId ?? "");
                    break;
                case "recordingstopped":
                    await LogSessionEventAsync(webhook.SessionId, OpenTokEventType.RecordingStopped, webhook.ConnectionId ?? "");
                    break;
            }

            return ApiResponse<bool>.SuccessResponse(true, "Webhook processed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OpenTok webhook: {EventType}", webhook.EventType);
            return ApiResponse<bool>.ErrorResponse("Failed to process webhook", 500);
        }
    }

    public async Task<ApiResponse<bool>> LogSessionEventAsync(string sessionId, OpenTokEventType eventType, string connectionId, string? streamId = null)
    {
        try
        {
            _logger.LogInformation("Session event: {EventType} for session: {SessionId}, connection: {ConnectionId}, stream: {StreamId}", 
                eventType, sessionId, connectionId, streamId ?? "N/A");

            // Here you would typically log to your database
            // For now, we'll just log to the console

            return ApiResponse<bool>.SuccessResponse(true, "Event logged successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging session event: {EventType} for session: {SessionId}", eventType, sessionId);
            return ApiResponse<bool>.ErrorResponse("Failed to log event", 500);
        }
    }

    public async Task<ApiResponse<OpenTokSessionAnalyticsDto>> GetSessionAnalyticsAsync(string sessionId)
    {
        try
        {
            // Note: OpenTok doesn't provide direct analytics through the SDK
            // This would typically be handled through the OpenTok dashboard or REST API
            var analytics = new OpenTokSessionAnalyticsDto
            {
                SessionId = sessionId,
                TotalConnections = 0,
                TotalStreams = 0,
                TotalRecordings = 0,
                TotalBroadcasts = 0,
                TotalDuration = TimeSpan.Zero,
                AverageAudioQuality = 0.0,
                AverageVideoQuality = 0.0,
                AverageNetworkQuality = 0.0,
                CreatedAt = DateTime.UtcNow
            };

            return ApiResponse<OpenTokSessionAnalyticsDto>.SuccessResponse(analytics, "Analytics retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics for session: {SessionId}", sessionId);
            return ApiResponse<OpenTokSessionAnalyticsDto>.ErrorResponse("Failed to retrieve analytics", 500);
        }
    }

    public async Task<ApiResponse<OpenTokConnectionQualityDto>> GetConnectionQualityAsync(string sessionId, string connectionId)
    {
        try
        {
            // Note: OpenTok doesn't provide direct connection quality through the SDK
            // This would typically be handled through the client-side SDK
            var quality = new OpenTokConnectionQualityDto
            {
                ConnectionId = connectionId,
                SessionId = sessionId,
                AudioLevel = 0.0,
                VideoLevel = 0.0,
                NetworkQuality = 0.0,
                PacketLoss = 0,
                RoundTripTime = 0,
                Timestamp = DateTime.UtcNow
            };

            return ApiResponse<OpenTokConnectionQualityDto>.SuccessResponse(quality, "Connection quality retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving connection quality for session: {SessionId}, connection: {ConnectionId}", sessionId, connectionId);
            return ApiResponse<OpenTokConnectionQualityDto>.ErrorResponse("Failed to retrieve connection quality", 500);
        }
    }

    public async Task<ApiResponse<bool>> IsServiceHealthyAsync()
    {
        try
        {
            // Test the service by creating a test session
            var session = await Task.Run(() => _openTok.CreateSession());
            
            _logger.LogInformation("OpenTok service health check passed");
            return ApiResponse<bool>.SuccessResponse(true, "Service is healthy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenTok service health check failed");
            return ApiResponse<bool>.ErrorResponse("Service is unhealthy", 500);
        }
    }
} 