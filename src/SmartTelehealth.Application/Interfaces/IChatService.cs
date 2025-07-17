using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IChatService
    {
        Task<ApiResponse<ChatRoomDto>> CreateChatRoomAsync(CreateChatRoomDto createDto);
        Task<ApiResponse<ChatRoomDto>> GetChatRoomAsync(string chatRoomId);
        Task<ApiResponse<ChatRoomDto>> CreateDirectChatAsync(string patientId, string providerId);
        Task<ApiResponse<ChatRoomDto>> CreateGroupChatAsync(string name, string description, List<string> participantIds, string createdBy);
        Task<ApiResponse<IEnumerable<ChatRoomParticipantDto>>> GetChatRoomParticipantsAsync(string chatRoomId);
        Task<ApiResponse<bool>> AddParticipantAsync(string chatRoomId, string userId, string role = "Member");
        Task<ApiResponse<bool>> RemoveParticipantAsync(string chatRoomId, string userId);
        Task<ApiResponse<MessageDto>> SendMessageAsync(string chatRoomId, string senderId, string content, string messageType = "text");
        Task<ApiResponse<IEnumerable<MessageDto>>> GetChatRoomMessagesAsync(string chatRoomId, int page = 1, int pageSize = 50);
        Task<ApiResponse<IEnumerable<MessageDto>>> GetUnreadMessagesAsync(string userId, string chatRoomId);
        Task<ApiResponse<bool>> MarkMessageAsReadAsync(string messageId, string userId);
        Task<ApiResponse<bool>> UpdateChatRoomStatusAsync(string chatRoomId, string status);
        Task<ApiResponse<bool>> UpdateParticipantRoleAsync(string chatRoomId, string userId, string newRole);
        Task<ApiResponse<IEnumerable<ChatRoomDto>>> GetUserChatRoomsAsync(string userId);
        Task<ApiResponse<bool>> ValidateChatAccessAsync(string userId, string chatRoomId);
    }
} 