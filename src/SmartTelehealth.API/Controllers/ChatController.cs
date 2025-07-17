using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.Services;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IMessagingService _messagingService;
    private readonly ChatService _chatService;
    private readonly ChatRoomService _chatRoomService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IMessagingService messagingService,
        ChatService chatService,
        ChatRoomService chatRoomService,
        ILogger<ChatController> logger)
    {
        _messagingService = messagingService;
        _chatService = chatService;
        _chatRoomService = chatRoomService;
        _logger = logger;
    }

    [HttpPost("messages")]
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

    [HttpPost("messages/with-notification")]
    public async Task<ActionResult<ApiResponse<bool>>> SendMessageWithNotification([FromBody] CreateMessageDto createDto)
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
            _logger.LogError(ex, "Error sending message with notification");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpGet("messages/{messageId}")]
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

    [HttpGet("rooms/{chatRoomId}/messages")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MessageDto>>>> GetChatRoomMessages(
        Guid chatRoomId, 
        [FromQuery] int skip = 0, 
        [FromQuery] int take = 50)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            // Validate access
            var accessResult = await _messagingService.ValidateChatRoomAccessAsync(chatRoomId.ToString(), userId.ToString());
            if (!accessResult.Success)
                return StatusCode(accessResult.StatusCode, accessResult);

            var result = await _messagingService.GetChatRoomMessagesAsync(chatRoomId.ToString(), skip, take);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages for chat room {ChatRoomId}", chatRoomId);
            return StatusCode(500, ApiResponse<IEnumerable<MessageDto>>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpPut("messages/{messageId}")]
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

    [HttpDelete("messages/{messageId}")]
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

    [HttpPost("messages/{messageId}/read")]
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

    [HttpPost("messages/{messageId}/reactions")]
    public async Task<ActionResult<ApiResponse<MessageReactionDto>>> AddReaction(Guid messageId, [FromBody] AddReactionDto addReactionDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var result = await _messagingService.AddReactionAsync(messageId.ToString(), addReactionDto.Emoji, userId.ToString());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding reaction to message {MessageId}", messageId);
            return StatusCode(500, ApiResponse<MessageReactionDto>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpDelete("messages/{messageId}/reactions/{emoji}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveReaction(Guid messageId, string emoji)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var result = await _messagingService.RemoveReactionAsync(messageId.ToString(), emoji, userId.ToString());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing reaction from message {MessageId}", messageId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpGet("messages/{messageId}/reactions")]
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

    [HttpGet("rooms/{chatRoomId}/search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MessageDto>>>> SearchMessages(Guid chatRoomId, [FromQuery] string searchTerm)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            // Validate access
            var accessResult = await _messagingService.ValidateChatRoomAccessAsync(chatRoomId.ToString(), userId.ToString());
            if (!accessResult.Success)
                return StatusCode(accessResult.StatusCode, accessResult);

            var result = await _messagingService.SearchMessagesAsync(chatRoomId.ToString(), searchTerm);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching messages in chat room {ChatRoomId}", chatRoomId);
            return StatusCode(500, ApiResponse<IEnumerable<MessageDto>>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpGet("rooms/{chatRoomId}/unread")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MessageDto>>>> GetUnreadMessages(Guid chatRoomId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            // Validate access
            var accessResult = await _messagingService.ValidateChatRoomAccessAsync(chatRoomId.ToString(), userId.ToString());
            if (!accessResult.Success)
                return StatusCode(accessResult.StatusCode, accessResult);

            var result = await _messagingService.GetUnreadMessagesAsync(chatRoomId.ToString(), userId.ToString());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread messages for chat room {ChatRoomId}", chatRoomId);
            return StatusCode(500, ApiResponse<IEnumerable<MessageDto>>.ErrorResponse("Internal server error", 500));
        }
    }

    // [HttpGet("rooms/{chatRoomId}/history")]
    // public async Task<ActionResult<ApiResponse<IEnumerable<MessageDto>>>> GetChatHistory(Guid chatRoomId, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    // {
    //     try
    //     {
    //         var userId = GetCurrentUserId();
    //         if (userId == Guid.Empty)
    //             return Unauthorized();

    //         var result = await _messagingService.GetChatHistoryAsync(chatRoomId.ToString(), userId.ToString(), fromDate, toDate);
    //         return Ok(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error getting chat history for chat room {ChatRoomId}", chatRoomId);
    //         return StatusCode(500, ApiResponse<IEnumerable<MessageDto>>.ErrorResponse("Internal server error", 500));
    //     }
    // }

    // [HttpGet("rooms/{chatRoomId}/statistics")]
    // public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> GetChatRoomStatistics(Guid chatRoomId)
    // {
    //     try
    //     {
    //         var userId = GetCurrentUserId();
    //         if (userId == Guid.Empty)
    //             return Unauthorized();

    //         var result = await _messagingService.GetChatRoomStatisticsAsync(chatRoomId.ToString(), userId.ToString());
    //         return Ok(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error getting statistics for chat room {ChatRoomId}", chatRoomId);
    //         return StatusCode(500, ApiResponse<Dictionary<string, object>>.ErrorResponse("Internal server error", 500));
    //     }
    // }

    // [HttpPost("rooms/{chatRoomId}/archive")]
    // public async Task<ActionResult<ApiResponse<bool>>> ArchiveChatRoom(Guid chatRoomId)
    // {
    //     try
    //     {
    //         var userId = GetCurrentUserId();
    //         if (userId == Guid.Empty)
    //             return Unauthorized();

    //         var result = await _messagingService.ArchiveChatRoomAsync(chatRoomId.ToString(), userId.ToString());
    //         return Ok(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error archiving chat room {ChatRoomId}", chatRoomId);
    //         return StatusCode(500, ApiResponse<bool>.ErrorResponse("Internal server error", 500));
    //     }
    // }

    // [HttpPost("rooms/{chatRoomId}/participants/{participantId}/mute")]
    // public async Task<ActionResult<ApiResponse<bool>>> MuteParticipant(Guid chatRoomId, Guid participantId, [FromQuery] DateTime? muteUntil = null, [FromQuery] string? reason = null)
    // {
    //     try
    //     {
    //         var userId = GetCurrentUserId();
    //         if (userId == Guid.Empty)
    //             return Unauthorized();

    //         var result = await _messagingService.MuteParticipantAsync(chatRoomId.ToString(), participantId.ToString(), userId.ToString(), muteUntil, reason);
    //         return Ok(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error muting participant in chat room {ChatRoomId}", chatRoomId);
    //         return StatusCode(500, ApiResponse<bool>.ErrorResponse("Internal server error", 500));
    //     }
    // }

    // [HttpPost("rooms/{chatRoomId}/participants/{participantId}/unmute")]
    // public async Task<ActionResult<ApiResponse<bool>>> UnmuteParticipant(Guid chatRoomId, Guid participantId)
    // {
    //     try
    //     {
    //         var userId = GetCurrentUserId();
    //         if (userId == Guid.Empty)
    //             return Unauthorized();

    //         var result = await _messagingService.UnmuteParticipantAsync(chatRoomId.ToString(), participantId.ToString(), userId.ToString());
    //         return Ok(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error unmuting participant in chat room {ChatRoomId}", chatRoomId);
    //         return StatusCode(500, ApiResponse<bool>.ErrorResponse("Internal server error", 500));
    //     }
    // }

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

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Guid.Empty;

        return userId;
    }
} 