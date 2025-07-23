using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IChatRoomService
    {
        Task<ApiResponse<ChatRoomDto>> CreateChatRoomAsync(CreateChatRoomDto createDto);
        Task<ApiResponse<ChatRoomDto>> CreatePatientProviderChatRoomAsync(string patientId, string providerId, string? subscriptionId = null);
        Task<ApiResponse<ChatRoomDto>> CreateGroupChatRoomAsync(string name, string? description, List<string> participantIds, string creatorId);
        Task<ApiResponse<ChatRoomDto>> GetChatRoomAsync(string chatRoomId);
        Task<ApiResponse<IEnumerable<ChatRoomDto>>> GetUserChatRoomsAsync(string userId);
        Task<ApiResponse<IEnumerable<MessageDto>>> GetUnreadMessagesAsync(string userId, string chatRoomId);
        Task<ApiResponse<ChatRoomDto>> UpdateChatRoomAsync(string chatRoomId, UpdateChatRoomDto updateDto);
        Task<ApiResponse<bool>> RemoveParticipantAsync(string chatRoomId, string userId);
        Task<ApiResponse<IEnumerable<ChatRoomParticipantDto>>> GetChatRoomParticipantsAsync(string chatRoomId);
        Task<ApiResponse<bool>> AddParticipantAsync(string chatRoomId, string userId, string role = "Member");
        Task<ApiResponse<bool>> UpdateParticipantRoleAsync(string chatRoomId, string userId, string newRole);
        Task<ApiResponse<bool>> ValidateChatAccessAsync(string userId, string chatRoomId);
        Task<ApiResponse<ChatRoomDto>> GetChatRoomAsync(string chatRoomId, string userId);
        Task<ApiResponse<IEnumerable<MessageDto>>> GetChatRoomMessagesAsync(string chatRoomId, string userId);
    }
} 