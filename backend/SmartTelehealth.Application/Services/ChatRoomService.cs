using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;
using AutoMapper;

namespace SmartTelehealth.Application.Services;

public class ChatRoomService : IChatRoomService
{
    private readonly IChatStorageService _chatStorageService;
    private readonly ILogger<ChatRoomService> _logger;

    public ChatRoomService(IChatStorageService chatStorageService, ILogger<ChatRoomService> logger)
    {
        _chatStorageService = chatStorageService;
        _logger = logger;
    }

    public async Task<ApiResponse<ChatRoomDto>> CreateChatRoomAsync(CreateChatRoomDto createDto)
    {
        try
        {
            var chatRoom = await _chatStorageService.CreateChatRoomAsync(createDto);
            return ApiResponse<ChatRoomDto>.SuccessResponse(chatRoom, "Chat room created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chat room");
            return ApiResponse<ChatRoomDto>.ErrorResponse("Failed to create chat room");
        }
    }

    public async Task<ApiResponse<ChatRoomDto>> CreatePatientProviderChatRoomAsync(string patientId, string providerId, string? subscriptionId = null)
    {
        try
        {
            var createDto = new CreateChatRoomDto
            {
                Name = $"Patient-Provider Chat",
                Description = "Direct consultation chat between patient and provider",
                Type = "Direct",
                CreatedBy = patientId,
                ParticipantIds = new List<string> { patientId, providerId }
            };

            var chatRoom = await _chatStorageService.CreateChatRoomAsync(createDto);
            return ApiResponse<ChatRoomDto>.SuccessResponse(chatRoom, "Patient-provider chat room created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient-provider chat room");
            return ApiResponse<ChatRoomDto>.ErrorResponse("Failed to create patient-provider chat room");
        }
    }

    public async Task<ApiResponse<ChatRoomDto>> CreateGroupChatRoomAsync(string name, string? description, List<string> participantIds, string creatorId)
    {
        try
        {
            var createDto = new CreateChatRoomDto
            {
                Name = name,
                Description = description ?? "Group chat room",
                Type = "Group",
                CreatedBy = creatorId,
                ParticipantIds = participantIds
            };

            var chatRoom = await _chatStorageService.CreateChatRoomAsync(createDto);
            return ApiResponse<ChatRoomDto>.SuccessResponse(chatRoom, "Group chat room created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group chat room");
            return ApiResponse<ChatRoomDto>.ErrorResponse("Failed to create group chat room");
        }
    }

    public async Task<ApiResponse<ChatRoomDto>> GetChatRoomAsync(string chatRoomId)
    {
        try
        {
            var chatRoom = await _chatStorageService.GetChatRoomAsync(chatRoomId);
            if (chatRoom == null)
                return ApiResponse<ChatRoomDto>.ErrorResponse("Chat room not found");

            return ApiResponse<ChatRoomDto>.SuccessResponse(chatRoom, "Chat room retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat room {ChatRoomId}", chatRoomId);
            return ApiResponse<ChatRoomDto>.ErrorResponse("Failed to get chat room");
        }
    }

    public async Task<ApiResponse<IEnumerable<ChatRoomDto>>> GetUserChatRoomsAsync(string userId)
    {
        try
        {
            var chatRooms = await _chatStorageService.GetUserChatRoomsAsync(userId);
            return ApiResponse<IEnumerable<ChatRoomDto>>.SuccessResponse(chatRooms, "User chat rooms retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user chat rooms {UserId}", userId);
            return ApiResponse<IEnumerable<ChatRoomDto>>.ErrorResponse("Failed to get user chat rooms");
        }
    }

    public async Task<ApiResponse<IEnumerable<MessageDto>>> GetUnreadMessagesAsync(string userId, string chatRoomId)
    {
        try
        {
            var messages = await _chatStorageService.GetUnreadMessagesAsync(userId, chatRoomId);
            return ApiResponse<IEnumerable<MessageDto>>.SuccessResponse(messages, "Unread messages retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread messages");
            return ApiResponse<IEnumerable<MessageDto>>.ErrorResponse("Failed to get unread messages");
        }
    }

    public async Task<ApiResponse<ChatRoomDto>> UpdateChatRoomAsync(string chatRoomId, UpdateChatRoomDto updateDto)
    {
        try
        {
            var chatRoom = await _chatStorageService.UpdateChatRoomAsync(chatRoomId, updateDto);
            if (chatRoom == null)
                return ApiResponse<ChatRoomDto>.ErrorResponse("Chat room not found");

            return ApiResponse<ChatRoomDto>.SuccessResponse(chatRoom, "Chat room updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating chat room {ChatRoomId}", chatRoomId);
            return ApiResponse<ChatRoomDto>.ErrorResponse("Failed to update chat room");
        }
    }

    public async Task<ApiResponse<bool>> RemoveParticipantAsync(string chatRoomId, string userId)
    {
        try
        {
            var result = await _chatStorageService.RemoveParticipantAsync(chatRoomId, userId);
            return ApiResponse<bool>.SuccessResponse(result, "Participant removed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing participant from chat room");
            return ApiResponse<bool>.ErrorResponse("Failed to remove participant");
        }
    }

    public async Task<ApiResponse<IEnumerable<ChatRoomParticipantDto>>> GetChatRoomParticipantsAsync(string chatRoomId)
    {
        try
        {
            var participants = await _chatStorageService.GetChatRoomParticipantsAsync(chatRoomId);
            return ApiResponse<IEnumerable<ChatRoomParticipantDto>>.SuccessResponse(participants, "Chat room participants retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat room participants {ChatRoomId}", chatRoomId);
            return ApiResponse<IEnumerable<ChatRoomParticipantDto>>.ErrorResponse("Failed to get chat room participants");
        }
    }

    public async Task<ApiResponse<bool>> AddParticipantAsync(string chatRoomId, string userId, string role = "Member")
    {
        try
        {
            var result = await _chatStorageService.AddParticipantAsync(chatRoomId, userId, role);
            return ApiResponse<bool>.SuccessResponse(result, "Participant added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding participant to chat room");
            return ApiResponse<bool>.ErrorResponse("Failed to add participant");
        }
    }

    public async Task<ApiResponse<bool>> UpdateParticipantRoleAsync(string chatRoomId, string userId, string newRole)
    {
        try
        {
            var result = await _chatStorageService.UpdateParticipantRoleAsync(chatRoomId, userId, newRole);
            return ApiResponse<bool>.SuccessResponse(result, "Participant role updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating participant role");
            return ApiResponse<bool>.ErrorResponse("Failed to update participant role");
        }
    }

    public async Task<ApiResponse<bool>> ValidateChatAccessAsync(string userId, string chatRoomId)
    {
        try
        {
            var result = await _chatStorageService.ValidateChatAccessAsync(userId, chatRoomId);
            return ApiResponse<bool>.SuccessResponse(result, "Chat access validated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating chat access");
            return ApiResponse<bool>.ErrorResponse("Failed to validate chat access");
        }
    }

    public async Task<ApiResponse<ChatRoomDto>> GetChatRoomAsync(string chatRoomId, string userId)
    {
        try
        {
            // Validate access first
            var accessResult = await _chatStorageService.ValidateChatAccessAsync(userId, chatRoomId);
            if (!accessResult)
                return ApiResponse<ChatRoomDto>.ErrorResponse("Access denied");

            var chatRoom = await _chatStorageService.GetChatRoomAsync(chatRoomId);
            if (chatRoom == null)
                return ApiResponse<ChatRoomDto>.ErrorResponse("Chat room not found");

            return ApiResponse<ChatRoomDto>.SuccessResponse(chatRoom, "Chat room retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat room {ChatRoomId} for user {UserId}", chatRoomId, userId);
            return ApiResponse<ChatRoomDto>.ErrorResponse("Failed to get chat room");
        }
    }

    public async Task<ApiResponse<IEnumerable<MessageDto>>> GetChatRoomMessagesAsync(string chatRoomId, string userId)
    {
        try
        {
            // Validate access first
            var accessResult = await _chatStorageService.ValidateChatAccessAsync(userId, chatRoomId);
            if (!accessResult)
                return ApiResponse<IEnumerable<MessageDto>>.ErrorResponse("Access denied");

            var messages = await _chatStorageService.GetChatRoomMessagesAsync(chatRoomId);
            return ApiResponse<IEnumerable<MessageDto>>.SuccessResponse(messages, "Chat room messages retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat room messages");
            return ApiResponse<IEnumerable<MessageDto>>.ErrorResponse("Failed to get chat room messages");
        }
    }
} 