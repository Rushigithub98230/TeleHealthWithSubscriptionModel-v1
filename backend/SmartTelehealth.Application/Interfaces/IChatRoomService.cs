using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IChatRoomService
    {
        Task<JsonModel> CreateChatRoomAsync(CreateChatRoomDto createDto);
        Task<JsonModel> CreatePatientProviderChatRoomAsync(string patientId, string providerId, string? subscriptionId = null);
        Task<JsonModel> CreateGroupChatRoomAsync(string name, string? description, List<string> participantIds, string creatorId);
        Task<JsonModel> GetChatRoomAsync(string chatRoomId);
        Task<JsonModel> GetUserChatRoomsAsync(string userId);
        Task<JsonModel> GetUnreadMessagesAsync(string userId, string chatRoomId);
        Task<JsonModel> UpdateChatRoomAsync(string chatRoomId, UpdateChatRoomDto updateDto);
        Task<JsonModel> RemoveParticipantAsync(string chatRoomId, string userId);
        Task<JsonModel> GetChatRoomParticipantsAsync(string chatRoomId);
        Task<JsonModel> AddParticipantAsync(string chatRoomId, string userId, string role = "Member");
        Task<JsonModel> UpdateParticipantRoleAsync(string chatRoomId, string userId, string newRole);
        Task<JsonModel> ValidateChatAccessAsync(string userId, string chatRoomId);
        Task<JsonModel> GetChatRoomAsync(string chatRoomId, string userId);
        Task<JsonModel> GetChatRoomMessagesAsync(string chatRoomId, string userId);
    }
} 