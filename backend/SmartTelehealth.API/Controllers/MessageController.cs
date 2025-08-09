using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessageController : ControllerBase
{
    private readonly IMessagingService _messagingService;
    private readonly ILogger<MessageController> _logger;

    public MessageController(
        IMessagingService messagingService,
        ILogger<MessageController> logger)
    {
        _messagingService = messagingService;
        _logger = logger;
    }

    [HttpGet("{messageId}")]
    public async Task<ActionResult<ApiResponse<MessageDto>>> GetMessage(Guid messageId)
    {
        try
        {
            var result = await _messagingService.GetMessageAsync(messageId.ToString());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message {MessageId}", messageId);
            return StatusCode(500, ApiResponse<MessageDto>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MessageDto>>> SendMessage([FromBody] CreateMessageDto createDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var result = await _messagingService.SendMessageAsync(createDto, userId.ToString());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return StatusCode(500, ApiResponse<MessageDto>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpPut("{messageId}")]
    public async Task<ActionResult<ApiResponse<MessageDto>>> UpdateMessage(Guid messageId, [FromBody] UpdateMessageDto updateDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var result = await _messagingService.UpdateMessageAsync(messageId.ToString(), updateDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating message {MessageId}", messageId);
            return StatusCode(500, ApiResponse<MessageDto>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpDelete("{messageId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMessage(Guid messageId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var result = await _messagingService.DeleteMessageAsync(messageId.ToString());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpPost("{messageId}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkMessageAsRead(Guid messageId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var result = await _messagingService.MarkMessageAsReadAsync(messageId.ToString(), userId.ToString());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message {MessageId} as read", messageId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpPost("{messageId}/reactions")]
    public async Task<ActionResult<ApiResponse<MessageReactionDto>>> AddReaction(Guid messageId, [FromBody] AddReactionDto addReactionDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var result = await _messagingService.AddReactionAsync(messageId.ToString(), userId.ToString(), addReactionDto.Emoji);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding reaction to message {MessageId}", messageId);
            return StatusCode(500, ApiResponse<MessageReactionDto>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpDelete("{messageId}/reactions/{emoji}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveReaction(Guid messageId, string emoji)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var result = await _messagingService.RemoveReactionAsync(messageId.ToString(), userId.ToString(), emoji);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing reaction from message {MessageId}", messageId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpGet("{messageId}/reactions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MessageReactionDto>>>> GetMessageReactions(Guid messageId)
    {
        try
        {
            var result = await _messagingService.GetMessageReactionsAsync(messageId.ToString());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reactions for message {MessageId}", messageId);
            return StatusCode(500, ApiResponse<IEnumerable<MessageReactionDto>>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpPost("encrypt")]
    public async Task<ActionResult<ApiResponse<string>>> EncryptMessage([FromBody] EncryptMessageDto encryptDto)
    {
        try
        {
            var result = await _messagingService.EncryptMessageAsync(encryptDto.Message, encryptDto.Key);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting message");
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpPost("decrypt")]
    public async Task<ActionResult<ApiResponse<string>>> DecryptMessage([FromBody] DecryptMessageDto decryptDto)
    {
        try
        {
            var result = await _messagingService.DecryptMessageAsync(decryptDto.EncryptedMessage, decryptDto.Key);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting message");
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpPost("attachments/upload")]
    public async Task<ActionResult<ApiResponse<string>>> UploadAttachment(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<string>.ErrorResponse("No file provided", 400));

            if (file.Length > 10 * 1024 * 1024) // 10MB limit
                return BadRequest(ApiResponse<string>.ErrorResponse("File size exceeds 10MB limit", 400));

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileData = memoryStream.ToArray();

            var result = await _messagingService.UploadMessageAttachmentAsync(fileData, file.FileName, file.ContentType);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading attachment {FileName}", file?.FileName);
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpGet("attachments/download")]
    public async Task<ActionResult> DownloadAttachment([FromQuery] string filePath)
    {
        try
        {
            var result = await _messagingService.DownloadMessageAttachmentAsync(filePath);
            if (!result.Success)
                return NotFound();

            // Extract filename from path
            var fileName = Path.GetFileName(filePath);
            return File(result.Data, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {FilePath}", filePath);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("attachments")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAttachment([FromQuery] string filePath)
    {
        try
        {
            var result = await _messagingService.DeleteMessageAttachmentAsync(filePath);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting attachment {FilePath}", filePath);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Internal server error", 500));
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Guid.Empty;

        return userId;
    }
}

// Additional DTOs for encryption operations
public class EncryptMessageDto
{
    public string Message { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}

public class DecryptMessageDto
{
    public string EncryptedMessage { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
} 