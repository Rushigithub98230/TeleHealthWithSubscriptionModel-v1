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
    public async Task<ActionResult<JsonModel>> HandleWebhook()
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
                return BadRequest(new JsonModel { data = new object(), Message = "Invalid webhook payload", StatusCode = 400 });
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

            if (result.StatusCode == 200)
            {
                _logger.LogInformation("Successfully processed OpenTok webhook for session {SessionId}", webhook.SessionId);
                return Ok(new JsonModel { data = new object(), Message = "Webhook processed successfully", StatusCode = 200 });
            }
            else
            {
                _logger.LogError("Failed to process OpenTok webhook: {Error}", result.Message);
                return StatusCode(500, new JsonModel { data = new object(), Message = result.Message, StatusCode = 500 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OpenTok webhook");
            return StatusCode(500, new JsonModel { data = new object(), Message = "Internal server error", StatusCode = 500 });
        }
    }

    [HttpGet("health")]
    public async Task<ActionResult<JsonModel>> HealthCheck()
    {
        try
        {
            var result = await _openTokService.IsServiceHealthyAsync();
            
            if (result.StatusCode == 200)
            {
                return Ok(new JsonModel { data = new object(), Message = "OpenTok service is operational", StatusCode = 200 });
            }
            else
            {
                return StatusCode(503, new JsonModel { data = new object(), Message = "OpenTok service is not operational", StatusCode = 503 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking OpenTok service health");
            return StatusCode(503, new JsonModel { data = new object(), Message = "Failed to check service health", StatusCode = 503 });
        }
    }

    [HttpPost("session/{sessionId}/token")]
    public async Task<ActionResult<JsonModel>> GenerateToken(string sessionId, [FromBody] TokenRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.UserName))
            {
                return BadRequest(new JsonModel { data = new object(), Message = "UserId and UserName are required", StatusCode = 400 });
            }

            var result = await _openTokService.GenerateTokenAsync(sessionId, request.UserId, request.UserName, request.Role);

            if (result.StatusCode == 200)
            {
                return Ok(new JsonModel { data = new { token = result.data }, Message = "Token generated successfully", StatusCode = 200 });
            }
            else
            {
                return BadRequest(new JsonModel { data = new object(), Message = result.Message, StatusCode = 400 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token for session {SessionId}", sessionId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Internal server error", StatusCode = 500 });
        }
    }

    [HttpPost("session")]
    public async Task<ActionResult<JsonModel>> CreateSession([FromBody] CreateSessionRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SessionName))
            {
                return BadRequest(new JsonModel { data = new object(), Message = "SessionName is required", StatusCode = 400 });
            }

            var result = await _openTokService.CreateSessionAsync(request.SessionName, request.IsArchived);

            if (result.StatusCode == 200)
            {
                return Ok(new JsonModel { data = result.data, Message = "Session created successfully", StatusCode = 200 });
            }
            else
            {
                return BadRequest(new JsonModel { data = new object(), Message = result.Message, StatusCode = 400 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating OpenTok session");
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to create session", StatusCode = 500 });
        }
    }

    [HttpPost("session/{sessionId}/recording")]
    public async Task<ActionResult<JsonModel>> StartRecording(string sessionId, [FromBody] StartRecordingRequest request)
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

            if (result.StatusCode == 200)
            {
                return Ok(new JsonModel { data = result.data, Message = "Recording started successfully", StatusCode = 200 });
            }
            else
            {
                return BadRequest(new JsonModel { data = new object(), Message = result.Message, StatusCode = 400 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting OpenTok recording for session {SessionId}", sessionId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to start recording", StatusCode = 500 });
        }
    }

    [HttpDelete("recording/{recordingId}")]
    public async Task<ActionResult<JsonModel>> StopRecording(string recordingId)
    {
        try
        {
            var result = await _openTokService.StopRecordingAsync(recordingId);

            if (result.StatusCode == 200)
            {
                return Ok(new JsonModel { data = new object(), Message = "Recording stopped successfully", StatusCode = 200 });
            }
            else
            {
                return BadRequest(new JsonModel { data = new object(), Message = result.Message, StatusCode = 400 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping OpenTok recording {RecordingId}", recordingId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to stop recording", StatusCode = 500 });
        }
    }

    [HttpGet("recording/{recordingId}/url")]
    public async Task<ActionResult<JsonModel>> GetRecordingUrl(string recordingId)
    {
        try
        {
            var result = await _openTokService.GetRecordingUrlAsync(recordingId);

            if (result.StatusCode == 200)
            {
                return Ok(new JsonModel { data = new { url = result.data }, Message = "Recording URL retrieved successfully", StatusCode = 200 });
            }
            else
            {
                return BadRequest(new JsonModel { data = new object(), Message = result.Message, StatusCode = 400 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OpenTok recording URL {RecordingId}", recordingId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to get recording URL", StatusCode = 500 });
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