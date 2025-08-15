using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using AutoMapper;
using System.Security.Cryptography;
using System.Text;

namespace SmartTelehealth.Infrastructure.Services;

public class ChatStorageService : IChatStorageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatRoomParticipantRepository _participantRepository;
    private readonly IMessageReactionRepository _reactionRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ChatStorageService> _logger;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;

    public ChatStorageService(
        IMessageRepository messageRepository,
        IChatRoomRepository chatRoomRepository,
        IChatRoomParticipantRepository participantRepository,
        IMessageReactionRepository reactionRepository,
        IUserRepository userRepository,
        ILogger<ChatStorageService> logger,
        IMapper mapper,
        IFileStorageService fileStorageService)
    {
        _messageRepository = messageRepository;
        _chatRoomRepository = chatRoomRepository;
        _participantRepository = participantRepository;
        _reactionRepository = reactionRepository;
        _userRepository = userRepository;
        _logger = logger;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
    }

    public async Task<ApiResponse<MessageDto>> StoreMessageAsync(CreateMessageDto createDto, string senderId)
    {
        try
        {
            // Validate chat room access
            var hasAccess = await ValidateChatAccessAsync(senderId, createDto.ChatRoomId);
            if (!hasAccess)
            {
                return ApiResponse<MessageDto>.ErrorResponse("Access denied to this chat room", 403);
            }

            // Create message entity
            var message = new Message
            {
                SenderId = int.Parse(senderId),
                ChatRoomId = Guid.Parse(createDto.ChatRoomId),
                Content = createDto.Content,
                Type = Enum.TryParse<Message.MessageType>(createDto.MessageType, out var mt) ? mt : Message.MessageType.Text,
                Status = Message.MessageStatus.Sent,
                IsEncrypted = true,
                CreatedDate = DateTime.UtcNow
            };
            if (!string.IsNullOrEmpty(createDto.ReplyToMessageId))
            {
                message.ReplyToMessageId = Guid.Parse(createDto.ReplyToMessageId);
            }
            // File attachment info (if present)
            if (!string.IsNullOrEmpty(createDto.AttachmentType) || createDto.AttachmentSize.HasValue)
            {
                message.FileType = createDto.AttachmentType;
                message.FileSize = createDto.AttachmentSize;
            }

            // Encrypt message content if needed
            if (message.IsEncrypted)
            {
                var chatRoomGuid = Guid.Parse(createDto.ChatRoomId);
                message.Content = await EncryptMessageAsync(message.Content, GetEncryptionKey(chatRoomGuid));
            }

            // Save message
            var savedMessage = await _messageRepository.CreateMessageAsync(message);

            // Update chat room last activity
            await UpdateChatRoomLastActivityAsync(Guid.Parse(createDto.ChatRoomId));

            var messageDto = await MapToMessageDtoAsync(savedMessage);
            return ApiResponse<MessageDto>.SuccessResponse(messageDto, "Message stored successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing message for chat room {ChatRoomId}", createDto.ChatRoomId);
            return ApiResponse<MessageDto>.ErrorResponse("Failed to store message", 500);
        }
    }

    public async Task<ApiResponse<MessageDto>> GetMessageAsync(Guid messageId)
    {
        try
        {
            var message = await _messageRepository.GetMessageByIdAsync(messageId);
            if (message == null)
            {
                return ApiResponse<MessageDto>.ErrorResponse("Message not found", 404);
            }

            var messageDto = await MapToMessageDtoAsync(message);
            return ApiResponse<MessageDto>.SuccessResponse(messageDto, "Message retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message {MessageId}", messageId);
            return ApiResponse<MessageDto>.ErrorResponse("Failed to retrieve message", 500);
        }
    }

    public async Task<ApiResponse<IEnumerable<MessageDto>>> GetChatRoomMessagesAsync(Guid chatRoomId, int skip = 0, int take = 50)
    {
        try
        {
            var messages = await _messageRepository.GetMessagesByChatRoomAsync(chatRoomId, skip, take);
            var messageDtos = new List<MessageDto>();

            foreach (var message in messages)
            {
                var messageDto = await MapToMessageDtoAsync(message);
                messageDtos.Add(messageDto);
            }

            return ApiResponse<IEnumerable<MessageDto>>.SuccessResponse(messageDtos, "Messages retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages for chat room {ChatRoomId}", chatRoomId);
            return ApiResponse<IEnumerable<MessageDto>>.ErrorResponse("Failed to retrieve messages", 500);
        }
    }

    public async Task<ApiResponse<MessageDto>> UpdateMessageAsync(Guid messageId, UpdateMessageDto updateDto, int userId)
    {
        try
        {
            var message = await _messageRepository.GetMessageByIdAsync(messageId);
            if (message == null)
            {
                return ApiResponse<MessageDto>.ErrorResponse("Message not found", 404);
            }

            // Check if user can edit this message
            if (message.SenderId != userId)
            {
                return ApiResponse<MessageDto>.ErrorResponse("You can only edit your own messages", 403);
            }

            // Update message content
            if (updateDto.Content != null)
                message.Content = updateDto.Content;
            if (updateDto.AttachmentType != null)
                message.FileType = updateDto.AttachmentType;
            if (updateDto.AttachmentSize.HasValue)
                message.FileSize = updateDto.AttachmentSize;
            if (updateDto.IsEdited.HasValue)
                message.UpdatedDate = updateDto.IsEdited.Value ? (updateDto.EditedAt ?? DateTime.UtcNow) : message.UpdatedDate;

            // Re-encrypt if needed
            if (message.IsEncrypted)
            {
                message.Content = await EncryptMessageAsync(message.Content, GetEncryptionKey(message.ChatRoomId));
            }

            var updatedMessage = await _messageRepository.UpdateMessageAsync(message);
            var messageDto = await MapToMessageDtoAsync(updatedMessage);

            return ApiResponse<MessageDto>.SuccessResponse(messageDto, "Message updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating message {MessageId}", messageId);
            return ApiResponse<MessageDto>.ErrorResponse("Failed to update message", 500);
        }
    }

    public async Task<ApiResponse<bool>> DeleteMessageAsync(Guid messageId, int userId)
    {
        try
        {
            var message = await _messageRepository.GetMessageByIdAsync(messageId);
            if (message == null)
            {
                return ApiResponse<bool>.ErrorResponse("Message not found", 404);
            }

            // Check if user can delete this message
            if (message.SenderId != userId)
            {
                return ApiResponse<bool>.ErrorResponse("You can only delete your own messages", 403);
            }

            var result = await _messageRepository.DeleteMessageAsync(messageId);
            return ApiResponse<bool>.SuccessResponse(result, "Message deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
            return ApiResponse<bool>.ErrorResponse("Failed to delete message", 500);
        }
    }

    public async Task<ApiResponse<bool>> SoftDeleteMessageAsync(Guid messageId, int userId)
    {
        try
        {
            var message = await _messageRepository.GetMessageByIdAsync(messageId);
            if (message == null)
            {
                return ApiResponse<bool>.ErrorResponse("Message not found", 404);
            }

            // Check if user can delete this message
            if (message.SenderId != userId)
            {
                return ApiResponse<bool>.ErrorResponse("You can only delete your own messages", 403);
            }

            var result = await _messageRepository.SoftDeleteMessageAsync(messageId);
            return ApiResponse<bool>.SuccessResponse(result, "Message soft deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting message {MessageId}", messageId);
            return ApiResponse<bool>.ErrorResponse("Failed to soft delete message", 500);
        }
    }

    public Task<ChatRoomDto> CreateChatRoomAsync(CreateChatRoomDto createDto) => throw new NotImplementedException();
    public Task<ChatRoomDto?> GetChatRoomAsync(string chatRoomId) => throw new NotImplementedException();
    public Task<ChatRoomDto?> UpdateChatRoomAsync(string chatRoomId, UpdateChatRoomDto updateDto) => throw new NotImplementedException();
    public Task<bool> DeleteChatRoomAsync(string chatRoomId) => throw new NotImplementedException();
    public Task<IEnumerable<ChatRoomDto>> GetUserChatRoomsAsync(string userId) => throw new NotImplementedException();
    public Task<bool> AddParticipantAsync(string chatRoomId, string userId, string role = "Member") => throw new NotImplementedException();
    public Task<bool> RemoveParticipantAsync(string chatRoomId, string userId) => throw new NotImplementedException();
    public Task<IEnumerable<ChatRoomParticipantDto>> GetChatRoomParticipantsAsync(string chatRoomId) => throw new NotImplementedException();
    public Task<bool> UpdateParticipantRoleAsync(string chatRoomId, string userId, string newRole) => throw new NotImplementedException();
    public Task<MessageDto> StoreMessageAsync(CreateMessageDto createDto) => throw new NotImplementedException();
    public Task<MessageDto?> GetMessageAsync(string messageId) => throw new NotImplementedException();
    public Task<IEnumerable<MessageDto>> GetChatRoomMessagesAsync(string chatRoomId, int page = 1, int pageSize = 50) => throw new NotImplementedException();
    public Task<bool> UpdateMessageAsync(string messageId, UpdateMessageDto updateDto) => throw new NotImplementedException();
    public Task<bool> DeleteMessageAsync(string messageId) => throw new NotImplementedException();
    public Task<IEnumerable<MessageDto>> GetUnreadMessagesAsync(string userId, string chatRoomId) => throw new NotImplementedException();
    public Task<bool> MarkMessageAsReadAsync(string messageId, string userId) => throw new NotImplementedException();
    public Task<bool> AddReactionAsync(string messageId, string userId, string reactionType) => throw new NotImplementedException();
    public Task<bool> RemoveReactionAsync(string messageId, string userId, string reactionType) => throw new NotImplementedException();
    public Task<IEnumerable<MessageReactionDto>> GetMessageReactionsAsync(string messageId) => throw new NotImplementedException();
    public Task<string> UploadMessageAttachmentAsync(string messageId, Stream fileStream, string fileName, string contentType) => throw new NotImplementedException();
    public Task<Stream> DownloadMessageAttachmentAsync(string attachmentId) => throw new NotImplementedException();
    public Task<bool> DeleteMessageAttachmentAsync(string attachmentId) => throw new NotImplementedException();
    public Task<IEnumerable<MessageDto>> SearchMessagesAsync(string chatRoomId, string searchTerm) => throw new NotImplementedException();
    public Task<bool> ValidateChatAccessAsync(string userId, string chatRoomId) => throw new NotImplementedException();
    public Task<ChatStatisticsDto> GetChatStatisticsAsync(string chatRoomId) => throw new NotImplementedException();

    // Private helper methods
    private async Task<MessageDto> MapToMessageDtoAsync(Message message)
    {
        var messageDto = _mapper.Map<MessageDto>(message);
        // Get sender information
        var sender = await _userRepository.GetByIdAsync(message.SenderId);
        if (sender != null)
        {
            messageDto.SenderName = $"{sender.FirstName} {sender.LastName}".Trim();
        }
        // Get chat room information
        var chatRoom = await _chatRoomRepository.GetByIdAsync(message.ChatRoomId);
        if (chatRoom != null)
        {
            messageDto.ChatRoomName = chatRoom.Name;
        }
        // Decrypt message content if needed
        if (message.IsEncrypted && !string.IsNullOrEmpty(message.Content))
        {
            try
            {
                messageDto.Content = await DecryptMessageAsync(message.Content, GetEncryptionKey(message.ChatRoomId));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to decrypt message {MessageId}", message.Id);
                messageDto.Content = "[Encrypted Message]";
            }
        }
        // Map attachment info
        messageDto.AttachmentIds = message.Attachments.Select(a => a.Id.ToString()).ToList();
        // Map reactions
        var reactions = await _reactionRepository.GetByMessageIdAsync(message.Id);
        messageDto.Reactions = reactions.Select(r => _mapper.Map<MessageReactionDto>(r)).ToList();
        // Remove ReactionCount/ReplyCount (not in DTO)
        // Map reply info
        var replies = await _messageRepository.GetRepliesAsync(message.Id);
        if (replies.Any())
        {
            var reply = replies.First();
            messageDto.ReplyToMessageId = reply.Id.ToString();
            messageDto.ReplyToMessageContent = reply.Content;
        }
        return messageDto;
    }

    private async Task<ChatRoomDto> MapToChatRoomDtoAsync(ChatRoom chatRoom)
    {
        var chatRoomDto = _mapper.Map<ChatRoomDto>(chatRoom);

        // Get participant count
        var participants = await _participantRepository.GetByChatRoomIdAsync(chatRoom.Id);
        chatRoomDto.ParticipantCount = participants.Count(p => p.Status == ChatRoomParticipant.ParticipantStatus.Active);

        // Get last message info
        var lastMessage = await _messageRepository.GetByChatRoomIdAsync(chatRoom.Id);
        var lastMessageList = lastMessage.ToList();
        if (lastMessageList.Any())
        {
            chatRoomDto.LastMessageAt = lastMessageList.Max(m => m.CreatedDate);
        }

        return chatRoomDto;
    }

    private async Task<ChatRoomParticipantDto> MapToParticipantDtoAsync(ChatRoomParticipant participant)
    {
        var participantDto = _mapper.Map<ChatRoomParticipantDto>(participant);

        // Get user information
        var user = await _userRepository.GetByIdAsync(participant.UserId);
        if (user != null)
        {
            participantDto.UserName = $"{user.FirstName} {user.LastName}".Trim();
        }

        return participantDto;
    }

    private async Task<MessageReactionDto> MapToReactionDtoAsync(MessageReaction reaction)
    {
        var reactionDto = _mapper.Map<MessageReactionDto>(reaction);
        // Remove UserName (not in DTO)
        return reactionDto;
    }

    private async Task UpdateChatRoomLastActivityAsync(Guid chatRoomId)
    {
        try
        {
            var chatRoom = await _chatRoomRepository.GetByIdAsync(chatRoomId);
            if (chatRoom != null)
            {
                chatRoom.LastActivityAt = DateTime.UtcNow;
                await _chatRoomRepository.UpdateAsync(chatRoom);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last activity for chat room {ChatRoomId}", chatRoomId);
        }
    }

    private string GetEncryptionKey(Guid chatRoomId)
    {
        // In a real implementation, you would use a proper key management system
        // For now, we'll use a simple hash of the chat room ID
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(chatRoomId.ToString()));
        return Convert.ToBase64String(hash);
    }

    private async Task<string> EncryptMessageAsync(string message, string key)
    {
        // Simple encryption for demo purposes
        // In production, use proper encryption libraries
        var keyBytes = Convert.FromBase64String(key);
        var messageBytes = Encoding.UTF8.GetBytes(message);
        
        for (int i = 0; i < messageBytes.Length; i++)
        {
            messageBytes[i] = (byte)(messageBytes[i] ^ keyBytes[i % keyBytes.Length]);
        }
        
        return Convert.ToBase64String(messageBytes);
    }

    private async Task<string> DecryptMessageAsync(string encryptedMessage, string key)
    {
        // Simple decryption for demo purposes
        // In production, use proper encryption libraries
        var keyBytes = Convert.FromBase64String(key);
        var messageBytes = Convert.FromBase64String(encryptedMessage);
        
        for (int i = 0; i < messageBytes.Length; i++)
        {
            messageBytes[i] = (byte)(messageBytes[i] ^ keyBytes[i % keyBytes.Length]);
        }
        
        return Encoding.UTF8.GetString(messageBytes);
    }
} 