using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IChatService
    {
        Task<JsonModel> CreateChatRoomAsync(CreateChatRoomDto createDto);
        Task<JsonModel> GetChatRoomAsync(string chatRoomId);
        Task<JsonModel> CreateDirectChatAsync(string patientId, string providerId);
        Task<JsonModel> CreateGroupChatAsync(string name, string description, List<string> participantIds, string createdBy);
        Task<JsonModel> GetChatRoomParticipantsAsync(string chatRoomId);
        Task<JsonModel> AddParticipantAsync(string chatRoomId, string userId, string role = "Member");
        Task<JsonModel> RemoveParticipantAsync(string chatRoomId, string userId);
        Task<JsonModel> SendMessageAsync(string chatRoomId, string senderId, string content, string messageType = "text");
        Task<JsonModel> GetChatRoomMessagesAsync(string chatRoomId, int page = 1, int pageSize = 50);
        Task<JsonModel> GetUnreadMessagesAsync(string userId, string chatRoomId);
        Task<JsonModel> MarkMessageAsReadAsync(string messageId, string userId);
        Task<JsonModel> UpdateChatRoomStatusAsync(string chatRoomId, string status);
        Task<JsonModel> UpdateParticipantRoleAsync(string chatRoomId, string userId, string newRole);
        Task<JsonModel> GetUserChatRoomsAsync(string userId);
        Task<JsonModel> ValidateChatAccessAsync(string userId, string chatRoomId);
    }
} 