using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface ILocalChatStorageService : IChatStorageService
{
    Task<ApiResponse<ChatRoomDto>> CreateChatRoomAsync(CreateChatRoomDto createDto);
    Task<ApiResponse<ChatRoomDto>> UpdateChatRoomAsync(Guid chatRoomId, UpdateChatRoomDto updateDto);
    Task<ApiResponse<bool>> DeleteChatRoomAsync(Guid chatRoomId);
    Task<ApiResponse<MessageDto>> CreateMessageAsync(CreateMessageDto createDto);
    Task<ApiResponse<MessageDto>> UpdateMessageAsync(Guid messageId, UpdateMessageDto updateDto);
    Task<ApiResponse<bool>> DeleteMessageAsync(Guid messageId);
    Task<ApiResponse<MessageDto>> GetMessageByIdAsync(Guid messageId);
    Task<ApiResponse<IEnumerable<MessageDto>>> GetMessagesByChatRoomAsync(Guid chatRoomId);
} 