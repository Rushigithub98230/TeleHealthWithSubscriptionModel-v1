using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace SmartTelehealth.Application.Services;

public class ChatService : IChatService
{
    private readonly IChatStorageService _chatStorageService;
    private readonly ILogger<ChatService> _logger;

    public ChatService(IChatStorageService chatStorageService, ILogger<ChatService> logger)
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

    public async Task<ApiResponse<ChatRoomDto>> CreateDirectChatAsync(string patientId, string providerId)
    {
        try
        {
            var createDto = new CreateChatRoomDto
            {
                Name = $"Direct Chat - {patientId} & {providerId}",
                Description = "Direct chat between patient and provider",
                Type = "Direct",
                CreatedBy = patientId,
                ParticipantIds = new List<string> { patientId, providerId }
            };

            var chatRoom = await _chatStorageService.CreateChatRoomAsync(createDto);
            return ApiResponse<ChatRoomDto>.SuccessResponse(chatRoom, "Direct chat room created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating direct chat room");
            return ApiResponse<ChatRoomDto>.ErrorResponse("Failed to create direct chat room");
        }
    }

    public async Task<ApiResponse<ChatRoomDto>> CreateGroupChatAsync(string name, string description, List<string> participantIds, string createdBy)
    {
        try
        {
            var createDto = new CreateChatRoomDto
            {
                Name = name,
                Description = description,
                Type = "Group",
                CreatedBy = createdBy,
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

    public async Task<ApiResponse<MessageDto>> SendMessageAsync(string chatRoomId, string senderId, string content, string messageType = "text")
    {
        try
        {
            var createDto = new CreateMessageDto
            {
                ChatRoomId = chatRoomId,
                SenderId = senderId,
                Content = content,
                MessageType = messageType
            };

            var message = await _chatStorageService.StoreMessageAsync(createDto);
            return ApiResponse<MessageDto>.SuccessResponse(message, "Message sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return ApiResponse<MessageDto>.ErrorResponse("Failed to send message");
        }
    }

    public async Task<ApiResponse<IEnumerable<MessageDto>>> GetChatRoomMessagesAsync(string chatRoomId, int page = 1, int pageSize = 50)
    {
        try
        {
            var messages = await _chatStorageService.GetChatRoomMessagesAsync(chatRoomId, page, pageSize);
            return ApiResponse<IEnumerable<MessageDto>>.SuccessResponse(messages, "Chat room messages retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat room messages");
            return ApiResponse<IEnumerable<MessageDto>>.ErrorResponse("Failed to get chat room messages");
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

    public async Task<ApiResponse<bool>> MarkMessageAsReadAsync(string messageId, string userId)
    {
        try
        {
            var result = await _chatStorageService.MarkMessageAsReadAsync(messageId, userId);
            return ApiResponse<bool>.SuccessResponse(result, "Message marked as read successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message as read");
            return ApiResponse<bool>.ErrorResponse("Failed to mark message as read");
        }
    }

    public async Task<ApiResponse<bool>> UpdateChatRoomStatusAsync(string chatRoomId, string status)
    {
        try
        {
            var updateDto = new UpdateChatRoomDto
            {
                Status = status
            };

            var result = await _chatStorageService.UpdateChatRoomAsync(chatRoomId, updateDto);
            return ApiResponse<bool>.SuccessResponse(result != null, "Chat room status updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating chat room status");
            return ApiResponse<bool>.ErrorResponse("Failed to update chat room status");
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

    public async Task<ApiResponse<IEnumerable<ChatRoomDto>>> GetUserChatRoomsAsync(string userId)
    {
        try
        {
            var chatRooms = await _chatStorageService.GetUserChatRoomsAsync(userId);
            return ApiResponse<IEnumerable<ChatRoomDto>>.SuccessResponse(chatRooms, "User chat rooms retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user chat rooms");
            return ApiResponse<IEnumerable<ChatRoomDto>>.ErrorResponse("Failed to get user chat rooms");
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
} 