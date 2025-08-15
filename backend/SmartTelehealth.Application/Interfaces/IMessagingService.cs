using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IMessagingService
    {
        Task<JsonModel> SendMessageAsync(CreateMessageDto createDto, string senderId);
        Task<JsonModel> GetMessageAsync(string messageId);
        Task<JsonModel> GetChatRoomMessagesAsync(string chatRoomId, int page = 1, int pageSize = 50);
        Task<JsonModel> UpdateMessageAsync(string messageId, UpdateMessageDto updateDto);
        Task<JsonModel> DeleteMessageAsync(string messageId);
        Task<JsonModel> GetChatRoomAsync(string chatRoomId);
        Task<IEnumerable<ChatRoomDto>> GetUserChatRoomsAsync(string userId);
        Task<JsonModel> UpdateChatRoomAsync(string chatRoomId, UpdateChatRoomDto updateDto);
        Task<JsonModel> DeleteChatRoomAsync(string chatRoomId);
        Task<JsonModel> AddParticipantAsync(string chatRoomId, string userId, string role = "Member");
        Task<JsonModel> RemoveParticipantAsync(string chatRoomId, string userId);
        Task<JsonModel> GetChatRoomParticipantsAsync(string chatRoomId);
        Task<JsonModel> UpdateParticipantRoleAsync(string chatRoomId, string userId, string newRole);
        Task<JsonModel> MarkMessageAsReadAsync(string messageId, string userId);
        Task<JsonModel> AddReactionAsync(string messageId, string userId, string reactionType);
        Task<JsonModel> RemoveReactionAsync(string messageId, string userId, string reactionType);
        Task<JsonModel> GetMessageReactionsAsync(string messageId);
        Task<JsonModel> SearchMessagesAsync(string chatRoomId, string searchTerm);
        Task<JsonModel> ValidateChatRoomAccessAsync(string chatRoomId, string userId);
        Task<JsonModel> GetUnreadMessagesAsync(string chatRoomId, string userId);
        Task<JsonModel> SendNotificationToUserAsync(string userId, string title, string message, string? data = null);
        Task<JsonModel> SendNotificationToChatRoomAsync(string chatRoomId, string title, string message);
        Task<JsonModel> SendTypingIndicatorAsync(string chatRoomId, string userId, bool isTyping);
        Task<JsonModel> UploadMessageAttachmentAsync(byte[] fileData, string fileName, string contentType);
        Task<JsonModel> DownloadMessageAttachmentAsync(string attachmentId);
        Task<JsonModel> DeleteMessageAttachmentAsync(string attachmentId);
        Task<JsonModel> EncryptMessageAsync(string message, string key);
        Task<JsonModel> DecryptMessageAsync(string encryptedMessage, string key);
        
        // Missing methods from controllers
        Task<JsonModel> CreateChatRoomAsync(CreateChatRoomDto createChatRoomDto);
    }
} 