using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IMessagingService
    {
        Task<ApiResponse<MessageDto>> SendMessageAsync(CreateMessageDto createDto, string senderId);
        Task<ApiResponse<MessageDto>> GetMessageAsync(string messageId);
        Task<ApiResponse<IEnumerable<MessageDto>>> GetChatRoomMessagesAsync(string chatRoomId, int page = 1, int pageSize = 50);
        Task<ApiResponse<bool>> UpdateMessageAsync(string messageId, UpdateMessageDto updateDto);
        Task<ApiResponse<bool>> DeleteMessageAsync(string messageId);
        Task<ApiResponse<ChatRoomDto>> GetChatRoomAsync(string chatRoomId);
        Task<IEnumerable<ChatRoomDto>> GetUserChatRoomsAsync(string userId);
        Task<ApiResponse<ChatRoomDto>> UpdateChatRoomAsync(string chatRoomId, UpdateChatRoomDto updateDto);
        Task<ApiResponse<bool>> DeleteChatRoomAsync(string chatRoomId);
        Task<ApiResponse<bool>> AddParticipantAsync(string chatRoomId, string userId, string role = "Member");
        Task<ApiResponse<bool>> RemoveParticipantAsync(string chatRoomId, string userId);
        Task<ApiResponse<IEnumerable<ChatRoomParticipantDto>>> GetChatRoomParticipantsAsync(string chatRoomId);
        Task<ApiResponse<bool>> UpdateParticipantRoleAsync(string chatRoomId, string userId, string newRole);
        Task<ApiResponse<bool>> MarkMessageAsReadAsync(string messageId, string userId);
        Task<ApiResponse<bool>> AddReactionAsync(string messageId, string userId, string reactionType);
        Task<ApiResponse<bool>> RemoveReactionAsync(string messageId, string userId, string reactionType);
        Task<ApiResponse<IEnumerable<MessageReactionDto>>> GetMessageReactionsAsync(string messageId);
        Task<ApiResponse<IEnumerable<MessageDto>>> SearchMessagesAsync(string chatRoomId, string searchTerm);
        Task<ApiResponse<bool>> ValidateChatRoomAccessAsync(string chatRoomId, string userId);
        Task<ApiResponse<IEnumerable<MessageDto>>> GetUnreadMessagesAsync(string chatRoomId, string userId);
        Task<ApiResponse<bool>> SendNotificationToUserAsync(string userId, string title, string message, string? data = null);
        Task<ApiResponse<bool>> SendNotificationToChatRoomAsync(string chatRoomId, string title, string message);
        Task<ApiResponse<bool>> SendTypingIndicatorAsync(string chatRoomId, string userId, bool isTyping);
        Task<ApiResponse<string>> UploadMessageAttachmentAsync(byte[] fileData, string fileName, string contentType);
        Task<ApiResponse<byte[]>> DownloadMessageAttachmentAsync(string attachmentId);
        Task<ApiResponse<bool>> DeleteMessageAttachmentAsync(string attachmentId);
        Task<ApiResponse<string>> EncryptMessageAsync(string message, string key);
        Task<ApiResponse<string>> DecryptMessageAsync(string encryptedMessage, string key);
        
        // Missing methods from controllers
        Task<ApiResponse<ChatRoomDto>> CreateChatRoomAsync(CreateChatRoomDto createChatRoomDto);
    }
} 