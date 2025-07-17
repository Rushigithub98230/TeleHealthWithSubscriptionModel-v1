using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.Services;
using System.Security.Claims;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatRoomController : ControllerBase
{
    private readonly IMessagingService _messagingService;
    private readonly ChatRoomService _chatRoomService;
    private readonly ILogger<ChatRoomController> _logger;

    public ChatRoomController(
        IMessagingService messagingService,
        ChatRoomService chatRoomService,
        ILogger<ChatRoomController> logger)
    {
        _messagingService = messagingService;
        _chatRoomService = chatRoomService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ChatRoomDto>>> CreateChatRoom([FromBody] CreateChatRoomDto createDto)
    {
        try
        {
            var result = await _messagingService.CreateChatRoomAsync(createDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chat room");
            return StatusCode(500, ApiResponse<ChatRoomDto>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpPost("patient-provider")]
    public async Task<ActionResult<ApiResponse<ChatRoomDto>>> CreatePatientProviderChatRoom(
        [FromBody] CreatePatientProviderChatRoomDto createDto)
    {
        try
        {
            var result = await _chatRoomService.CreatePatientProviderChatRoomAsync(
                createDto.PatientId.ToString(), 
                createDto.ProviderId.ToString(), 
                createDto.SubscriptionId?.ToString());

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient-provider chat room");
            return StatusCode(500, ApiResponse<ChatRoomDto>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpPost("group")]
    public async Task<ActionResult<ApiResponse<ChatRoomDto>>> CreateGroupChatRoom([FromBody] CreateGroupChatRoomDto createDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var result = await _chatRoomService.CreateGroupChatRoomAsync(
                createDto.Name,
                createDto.Description,
                createDto.ParticipantIds.Select(id => id.ToString()).ToList(),
                userId.ToString());

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group chat room");
            return StatusCode(500, ApiResponse<ChatRoomDto>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpPost("direct")]
    public async Task<ActionResult<ApiResponse<ChatRoomDto>>> CreateDirectChatRoom([FromBody] CreateDirectChatRoomDto createDto)
    {
        try
        {
            var result = await _chatRoomService.CreateGroupChatRoomAsync(
                createDto.Name ?? "Direct Chat",
                null,
                new List<string> { createDto.User1Id.ToString(), createDto.User2Id.ToString() },
                createDto.User1Id.ToString());

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating direct chat room");
            return StatusCode(500, ApiResponse<ChatRoomDto>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpPost("support")]
    public async Task<ActionResult<ApiResponse<ChatRoomDto>>> CreateSupportChatRoom([FromBody] CreateSupportChatRoomDto createDto)
    {
        try
        {
            var result = await _chatRoomService.CreateGroupChatRoomAsync(
                "Support Chat",
                createDto.Issue,
                new List<string> { createDto.UserId.ToString() },
                createDto.UserId.ToString());

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating support chat room");
            return StatusCode(500, ApiResponse<ChatRoomDto>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpGet("{chatRoomId}")]
    public async Task<ActionResult<ApiResponse<ChatRoomDto>>> GetChatRoom(Guid chatRoomId)
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

            var result = await _messagingService.GetChatRoomAsync(chatRoomId.ToString());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat room {ChatRoomId}", chatRoomId);
            return StatusCode(500, ApiResponse<ChatRoomDto>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ChatRoomDto>>>> GetUserChatRooms()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var result = await _messagingService.GetUserChatRoomsAsync(userId.ToString());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user chat rooms");
            return StatusCode(500, ApiResponse<IEnumerable<ChatRoomDto>>.ErrorResponse("Internal server error", 500));
        }
    }

    // [HttpGet("with-unread")]
    // public async Task<ActionResult<ApiResponse<IEnumerable<ChatRoomDto>>>> GetUserChatRoomsWithUnreadCount()
    // {
    //     try
    //     {
    //         var userId = GetCurrentUserId();
    //         if (userId == Guid.Empty)
    //             return Unauthorized();

    //         var result = await _messagingService.GetUserChatRoomsWithUnreadCountAsync(userId.ToString());
    //         return Ok(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error getting user chat rooms with unread count");
    //         return StatusCode(500, ApiResponse<IEnumerable<ChatRoomDto>>.ErrorResponse("Internal server error", 500));
    //     }
    // }

    [HttpPut("{chatRoomId}")]
    public async Task<ActionResult<ApiResponse<ChatRoomDto>>> UpdateChatRoom(Guid chatRoomId, [FromBody] UpdateChatRoomDto updateDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var result = await _messagingService.UpdateChatRoomAsync(chatRoomId.ToString(), updateDto);
            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            var chatRoomResult = await _messagingService.GetChatRoomAsync(chatRoomId.ToString());
            return Ok(chatRoomResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating chat room {ChatRoomId}", chatRoomId);
            return StatusCode(500, ApiResponse<ChatRoomDto>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpDelete("{chatRoomId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteChatRoom(Guid chatRoomId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var result = await _messagingService.DeleteChatRoomAsync(chatRoomId.ToString());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chat room {ChatRoomId}", chatRoomId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpPost("{chatRoomId}/participants")]
    public async Task<ActionResult<ApiResponse<ChatRoomParticipantDto>>> AddParticipant(
        Guid chatRoomId, 
        [FromBody] ChatRoomAddParticipantDto addDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            Console.WriteLine($"[DEBUG] chatRoomId: {chatRoomId} ({chatRoomId.GetType()})");
            Console.WriteLine($"[DEBUG] addDto.UserId: {addDto.UserId} ({addDto.UserId.GetType()})");
            Console.WriteLine($"[DEBUG] addDto.Role: {addDto.Role} ({addDto.Role.GetType()})");
            _logger.LogInformation($"[DEBUG] chatRoomId: {chatRoomId} ({chatRoomId.GetType()})");
            _logger.LogInformation($"[DEBUG] addDto.UserId: {addDto.UserId} ({addDto.UserId.GetType()})");
            _logger.LogInformation($"[DEBUG] addDto.Role: {addDto.Role} ({addDto.Role.GetType()})");
            var result = await _messagingService.AddParticipantAsync(chatRoomId.ToString(), addDto.UserId.ToString(), addDto.Role);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding participant to chat room {ChatRoomId}", chatRoomId);
            return StatusCode(500, ApiResponse<ChatRoomParticipantDto>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpDelete("{chatRoomId}/participants/{participantId}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveParticipant(Guid chatRoomId, Guid participantId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var result = await _messagingService.RemoveParticipantAsync(chatRoomId.ToString(), participantId.ToString());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing participant from chat room {ChatRoomId}", chatRoomId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpGet("{chatRoomId}/participants")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ChatRoomParticipantDto>>>> GetChatRoomParticipants(Guid chatRoomId)
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

            var result = await _messagingService.GetChatRoomParticipantsAsync(chatRoomId.ToString());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting participants for chat room {ChatRoomId}", chatRoomId);
            return StatusCode(500, ApiResponse<IEnumerable<ChatRoomParticipantDto>>.ErrorResponse("Internal server error", 500));
        }
    }

    [HttpPut("{chatRoomId}/participants/{participantId}/role")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateParticipantRole(
        Guid chatRoomId, 
        Guid participantId, 
        [FromBody] UpdateParticipantRoleDto updateDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var result = await _messagingService.UpdateParticipantRoleAsync(chatRoomId.ToString(), participantId.ToString(), updateDto.NewRole);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating participant role in chat room {ChatRoomId}", chatRoomId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Internal server error", 500));
        }
    }

    // [HttpPost("{chatRoomId}/leave")]
    // public async Task<ActionResult<ApiResponse<bool>>> LeaveChatRoom(Guid chatRoomId)
    // {
    //     try
    //     {
    //         var userId = GetCurrentUserId();
    //         if (userId == Guid.Empty)
    //             return Unauthorized();

    //         var result = await _messagingService.LeaveChatRoomAsync(chatRoomId.ToString(), userId.ToString());
    //         return Ok(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error leaving chat room {ChatRoomId}", chatRoomId);
    //         return StatusCode(500, ApiResponse<bool>.ErrorResponse("Internal server error", 500));
    //     }
    // }

    // [HttpPost("{chatRoomId}/invite")]
    // public async Task<ActionResult<ApiResponse<bool>>> InviteUserToChatRoom(
    //     Guid chatRoomId, 
    //     [FromBody] InviteUserDto inviteDto)
    // {
    //     try
    //     {
    //         var userId = GetCurrentUserId();
    //         if (userId == Guid.Empty)
    //             return Unauthorized();

    //         var result = await _messagingService.InviteUserToChatRoomAsync(chatRoomId.ToString(), userId.ToString(), inviteDto.InviteeId.ToString());
    //         return Ok(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error inviting user to chat room {ChatRoomId}", chatRoomId);
    //         return StatusCode(500, ApiResponse<bool>.ErrorResponse("Internal server error", 500));
    //     }
    // }

    // [HttpGet("{chatRoomId}/analytics")]
    // public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> GetChatRoomAnalytics(
    //     Guid chatRoomId,
    //     [FromQuery] DateTime fromDate,
    //     [FromQuery] DateTime toDate)
    // {
    //     try
    //     {
    //         var userId = GetCurrentUserId();
    //         if (userId == Guid.Empty)
    //             return Unauthorized();

    //         var result = await _messagingService.GetChatRoomAnalyticsAsync(chatRoomId.ToString(), userId.ToString(), fromDate, toDate);
    //         return Ok(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error getting analytics for chat room {ChatRoomId}", chatRoomId);
    //         return StatusCode(500, ApiResponse<Dictionary<string, object>>.ErrorResponse("Internal server error", 500));
    //     }
    // }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Guid.Empty;

        return userId;
    }
}

// Additional DTOs for specific operations
public class CreatePatientProviderChatRoomDto
{
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid? SubscriptionId { get; set; }
}

public class CreateGroupChatRoomDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<Guid> ParticipantIds { get; set; } = new();
}

public class CreateDirectChatRoomDto
{
    public Guid User1Id { get; set; }
    public Guid User2Id { get; set; }
    public string? Name { get; set; }
}

public class CreateSupportChatRoomDto
{
    public Guid UserId { get; set; }
    public string? Issue { get; set; }
}

public class ChatRoomAddParticipantDto
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = "Member";
}

public class UpdateParticipantRoleDto
{
    public string NewRole { get; set; } = "Member";
}

public class InviteUserDto
{
    public Guid InviteeId { get; set; }
} 