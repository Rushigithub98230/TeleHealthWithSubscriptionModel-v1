using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Text.Json;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OpenTokWebhookController : ControllerBase
{
    private readonly IOpenTokService _openTokService;
    private readonly ILogger<OpenTokWebhookController> _logger;

    public OpenTokWebhookController(IOpenTokService openTokService, ILogger<OpenTokWebhookController> logger)
    {
        _openTokService = openTokService;
        _logger = logger;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook()
    {
        try
        {
            // Read the webhook payload
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();

            _logger.LogInformation("Received OpenTok webhook: {Payload}", payload);

            // Parse the webhook data
            var webhookData = JsonSerializer.Deserialize<Dictionary<string, object>>(payload);
            
            if (webhookData == null)
            {
                return BadRequest("Invalid webhook payload");
            }

            // Extract webhook information
            var webhook = new OpenTokWebhookDto
            {
                EventType = webhookData.ContainsKey("eventType") ? webhookData["eventType"].ToString() : string.Empty,
                SessionId = webhookData.ContainsKey("sessionId") ? webhookData["sessionId"].ToString() : string.Empty,
                ConnectionId = webhookData.ContainsKey("connectionId") ? webhookData["connectionId"]?.ToString() : null,
                StreamId = webhookData.ContainsKey("streamId") ? webhookData["streamId"]?.ToString() : null,
                RecordingId = webhookData.ContainsKey("recordingId") ? webhookData["recordingId"]?.ToString() : null,
                BroadcastId = webhookData.ContainsKey("broadcastId") ? webhookData["broadcastId"]?.ToString() : null,
                Timestamp = DateTime.UtcNow,
                Data = webhookData
            };

            // Process the webhook
            var result = await _openTokService.HandleWebhookAsync(webhook);

            if (result.Success)
            {
                _logger.LogInformation("Successfully processed OpenTok webhook for session {SessionId}", webhook.SessionId);
                return Ok(new { success = true, message = "Webhook processed successfully" });
            }
            else
            {
                _logger.LogError("Failed to process OpenTok webhook: {Error}", result.Message);
                return StatusCode(500, new { success = false, message = result.Message });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OpenTok webhook");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    [HttpGet("health")]
    public async Task<IActionResult> HealthCheck()
    {
        try
        {
            var result = await _openTokService.IsServiceHealthyAsync();
            
            if (result.Success)
            {
                return Ok(new { status = "healthy", message = "OpenTok service is operational" });
            }
            else
            {
                return StatusCode(503, new { status = "unhealthy", message = "OpenTok service is not operational" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking OpenTok service health");
            return StatusCode(503, new { status = "error", message = "Failed to check service health" });
        }
    }

    [HttpPost("session/{sessionId}/token")]
    public async Task<IActionResult> GenerateToken(string sessionId, [FromBody] TokenRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.UserName))
            {
                return BadRequest("UserId and UserName are required");
            }

            var result = await _openTokService.GenerateTokenAsync(sessionId, request.UserId, request.UserName, request.Role);

            if (result.Success)
            {
                return Ok(new { token = result.Data });
            }
            else
            {
                return BadRequest(new { error = result.Message });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating OpenTok token for session {SessionId}", sessionId);
            return StatusCode(500, new { error = "Failed to generate token" });
        }
    }

    [HttpPost("session")]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SessionName))
            {
                return BadRequest("SessionName is required");
            }

            var result = await _openTokService.CreateSessionAsync(request.SessionName, request.IsArchived);

            if (result.Success)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(new { error = result.Message });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating OpenTok session");
            return StatusCode(500, new { error = "Failed to create session" });
        }
    }

    [HttpPost("session/{sessionId}/recording")]
    public async Task<IActionResult> StartRecording(string sessionId, [FromBody] StartRecordingRequest request)
    {
        try
        {
            var options = new OpenTokRecordingOptions
            {
                Name = request.Name ?? $"Recording_{DateTime.UtcNow:yyyyMMdd_HHmmss}",
                HasAudio = request.HasAudio,
                HasVideo = request.HasVideo,
                OutputMode = request.OutputMode,
                Resolution = request.Resolution,
                Layout = request.Layout,
                MaxDuration = request.MaxDuration,
                Storage = request.Storage
            };

            var result = await _openTokService.StartRecordingAsync(sessionId, options);

            if (result.Success)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(new { error = result.Message });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting OpenTok recording for session {SessionId}", sessionId);
            return StatusCode(500, new { error = "Failed to start recording" });
        }
    }

    [HttpDelete("recording/{recordingId}")]
    public async Task<IActionResult> StopRecording(string recordingId)
    {
        try
        {
            var result = await _openTokService.StopRecordingAsync(recordingId);

            if (result.Success)
            {
                return Ok(new { success = true, message = "Recording stopped successfully" });
            }
            else
            {
                return BadRequest(new { error = result.Message });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping OpenTok recording {RecordingId}", recordingId);
            return StatusCode(500, new { error = "Failed to stop recording" });
        }
    }

    [HttpGet("recording/{recordingId}/url")]
    public async Task<IActionResult> GetRecordingUrl(string recordingId)
    {
        try
        {
            var result = await _openTokService.GetRecordingUrlAsync(recordingId);

            if (result.Success)
            {
                return Ok(new { url = result.Data });
            }
            else
            {
                return BadRequest(new { error = result.Message });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OpenTok recording URL {RecordingId}", recordingId);
            return StatusCode(500, new { error = "Failed to get recording URL" });
        }
    }
}

public class TokenRequest
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public OpenTokRole Role { get; set; } = OpenTokRole.Publisher;
}

public class CreateSessionRequest
{
    public string SessionName { get; set; } = string.Empty;
    public bool IsArchived { get; set; } = true;
}

public class StartRecordingRequest
{
    public string? Name { get; set; }
    public bool HasAudio { get; set; } = true;
    public bool HasVideo { get; set; } = true;
    public OpenTokRecordingOutputMode OutputMode { get; set; } = OpenTokRecordingOutputMode.Composed;
    public string? Resolution { get; set; } = "1280x720";
    public string? Layout { get; set; }
    public int? MaxDuration { get; set; }
    public string? Storage { get; set; } = "cloud";
} 