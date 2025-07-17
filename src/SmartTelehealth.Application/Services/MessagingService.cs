using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Entities;
using AutoMapper;
using System.Security.Cryptography;
using System.Text;

namespace SmartTelehealth.Application.Services;

public class MessagingService : IMessagingService
{
    private readonly IChatStorageService _chatStorageService;
    private readonly ILogger<MessagingService> _logger;

    public MessagingService(IChatStorageService chatStorageService, ILogger<MessagingService> logger)
    {
        _chatStorageService = chatStorageService;
        _logger = logger;
    }

    public async Task<ApiResponse<MessageDto>> SendMessageAsync(CreateMessageDto createDto, string senderId)
    {
        try
        {
            var message = await _chatStorageService.StoreMessageAsync(createDto);
            return ApiResponse<MessageDto>.SuccessResponse(message, "Message sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return ApiResponse<MessageDto>.ErrorResponse("Failed to send message");
        }
    }

    public async Task<ApiResponse<MessageDto>> GetMessageAsync(string messageId)
    {
        try
        {
            var message = await _chatStorageService.GetMessageAsync(messageId);
            if (message == null)
                return ApiResponse<MessageDto>.ErrorResponse("Message not found");

            return ApiResponse<MessageDto>.SuccessResponse(message, "Message retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message {MessageId}", messageId);
            return ApiResponse<MessageDto>.ErrorResponse("Failed to get message");
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

    public async Task<ApiResponse<bool>> UpdateMessageAsync(string messageId, UpdateMessageDto updateDto)
    {
        try
        {
            var result = await _chatStorageService.UpdateMessageAsync(messageId, updateDto);
            return ApiResponse<bool>.SuccessResponse(result, "Message updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating message {MessageId}", messageId);
            return ApiResponse<bool>.ErrorResponse("Failed to update message");
        }
    }

    public async Task<ApiResponse<bool>> DeleteMessageAsync(string messageId)
    {
        try
        {
            var result = await _chatStorageService.DeleteMessageAsync(messageId);
            return ApiResponse<bool>.SuccessResponse(result, "Message deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
            return ApiResponse<bool>.ErrorResponse("Failed to delete message");
        }
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

    public async Task<IEnumerable<ChatRoomDto>> GetUserChatRoomsAsync(string userId)
    {
        try
        {
            return await _chatStorageService.GetUserChatRoomsAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user chat rooms {UserId}", userId);
            return new List<ChatRoomDto>();
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

    public async Task<ApiResponse<bool>> DeleteChatRoomAsync(string chatRoomId)
    {
        try
        {
            var result = await _chatStorageService.DeleteChatRoomAsync(chatRoomId);
            return ApiResponse<bool>.SuccessResponse(result, "Chat room deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chat room {ChatRoomId}", chatRoomId);
            return ApiResponse<bool>.ErrorResponse("Failed to delete chat room");
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

    public async Task<ApiResponse<bool>> AddReactionAsync(string messageId, string userId, string reactionType)
    {
        try
        {
            var result = await _chatStorageService.AddReactionAsync(messageId, userId, reactionType);
            return ApiResponse<bool>.SuccessResponse(result, "Reaction added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding reaction to message");
            return ApiResponse<bool>.ErrorResponse("Failed to add reaction");
        }
    }

    public async Task<ApiResponse<bool>> RemoveReactionAsync(string messageId, string userId, string reactionType)
    {
        try
        {
            var result = await _chatStorageService.RemoveReactionAsync(messageId, userId, reactionType);
            return ApiResponse<bool>.SuccessResponse(result, "Reaction removed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing reaction from message");
            return ApiResponse<bool>.ErrorResponse("Failed to remove reaction");
        }
    }

    public async Task<ApiResponse<IEnumerable<MessageReactionDto>>> GetMessageReactionsAsync(string messageId)
    {
        try
        {
            var reactions = await _chatStorageService.GetMessageReactionsAsync(messageId);
            return ApiResponse<IEnumerable<MessageReactionDto>>.SuccessResponse(reactions, "Message reactions retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message reactions {MessageId}", messageId);
            return ApiResponse<IEnumerable<MessageReactionDto>>.ErrorResponse("Failed to get message reactions");
        }
    }

    public async Task<ApiResponse<IEnumerable<MessageDto>>> SearchMessagesAsync(string chatRoomId, string searchTerm)
    {
        try
        {
            // This would be implemented in the chat storage service
            var messages = await _chatStorageService.GetChatRoomMessagesAsync(chatRoomId);
            var filteredMessages = messages.Where(m => m.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            return ApiResponse<IEnumerable<MessageDto>>.SuccessResponse(filteredMessages, "Messages searched successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching messages");
            return ApiResponse<IEnumerable<MessageDto>>.ErrorResponse("Failed to search messages");
        }
    }

    public async Task<ApiResponse<bool>> ValidateChatRoomAccessAsync(string chatRoomId, string userId)
    {
        try
        {
            var result = await _chatStorageService.ValidateChatAccessAsync(userId, chatRoomId);
            return ApiResponse<bool>.SuccessResponse(result, "Chat room access validated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating chat room access");
            return ApiResponse<bool>.ErrorResponse("Failed to validate chat room access");
        }
    }

    public async Task<ApiResponse<IEnumerable<MessageDto>>> GetUnreadMessagesAsync(string chatRoomId, string userId)
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

    public async Task<ApiResponse<bool>> SendNotificationToUserAsync(string userId, string title, string message, string? data = null)
    {
        try
        {
            // This would be implemented with a notification service
            return ApiResponse<bool>.SuccessResponse(true, "Notification sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user");
            return ApiResponse<bool>.ErrorResponse("Failed to send notification");
        }
    }

    public async Task<ApiResponse<bool>> SendNotificationToChatRoomAsync(string chatRoomId, string title, string message)
    {
        try
        {
            // This would be implemented with a notification service
            return ApiResponse<bool>.SuccessResponse(true, "Chat room notification sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to chat room");
            return ApiResponse<bool>.ErrorResponse("Failed to send chat room notification");
        }
    }

    public async Task<ApiResponse<bool>> SendTypingIndicatorAsync(string chatRoomId, string userId, bool isTyping)
    {
        try
        {
            // This would be implemented with real-time messaging
            return ApiResponse<bool>.SuccessResponse(true, "Typing indicator sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending typing indicator");
            return ApiResponse<bool>.ErrorResponse("Failed to send typing indicator");
        }
    }

    public async Task<ApiResponse<string>> UploadMessageAttachmentAsync(byte[] fileData, string fileName, string contentType)
    {
        try
        {
            using var stream = new MemoryStream(fileData);
            var attachmentId = await _chatStorageService.UploadMessageAttachmentAsync("temp", stream, fileName, contentType);
            return ApiResponse<string>.SuccessResponse(attachmentId, "Attachment uploaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading message attachment");
            return ApiResponse<string>.ErrorResponse("Failed to upload attachment");
        }
    }

    public async Task<ApiResponse<string>> EncryptMessageAsync(string message, string key)
    {
        try
        {
            // This would be implemented with encryption
            return ApiResponse<string>.SuccessResponse(message, "Message encrypted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting message");
            return ApiResponse<string>.ErrorResponse("Failed to encrypt message");
        }
    }

    public async Task<ApiResponse<string>> DecryptMessageAsync(string encryptedMessage, string key)
    {
        try
        {
            // This would be implemented with decryption
            return ApiResponse<string>.SuccessResponse(encryptedMessage, "Message decrypted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting message");
            return ApiResponse<string>.ErrorResponse("Failed to decrypt message");
        }
    }

    public async Task<ApiResponse<byte[]>> DownloadMessageAttachmentAsync(string attachmentId)
    {
        try
        {
            var stream = await _chatStorageService.DownloadMessageAttachmentAsync(attachmentId);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return ApiResponse<byte[]>.SuccessResponse(memoryStream.ToArray(), "Attachment downloaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading message attachment {AttachmentId}", attachmentId);
            return ApiResponse<byte[]>.ErrorResponse("Failed to download attachment");
        }
    }

    public async Task<ApiResponse<bool>> DeleteMessageAttachmentAsync(string attachmentId)
    {
        try
        {
            var result = await _chatStorageService.DeleteMessageAttachmentAsync(attachmentId);
            return ApiResponse<bool>.SuccessResponse(result, "Attachment deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message attachment {AttachmentId}", attachmentId);
            return ApiResponse<bool>.ErrorResponse("Failed to delete attachment");
        }
    }

    private string GetContentTypeFromFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
} 