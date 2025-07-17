using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IMessageReactionRepository
{
    Task<MessageReaction?> GetByIdAsync(Guid id);
    Task<IEnumerable<MessageReaction>> GetByMessageIdAsync(Guid messageId);
    Task<IEnumerable<MessageReaction>> GetByUserIdAsync(Guid userId);
    Task<MessageReaction?> GetByMessageAndUserAsync(Guid messageId, Guid userId);
    Task<MessageReaction> CreateAsync(MessageReaction reaction);
    Task<MessageReaction> UpdateAsync(MessageReaction reaction);
    Task<bool> RemoveReactionAsync(Guid messageId, string emoji, Guid userId);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetReactionCountAsync(Guid messageId);
    Task<bool> HasUserReactedAsync(Guid messageId, Guid userId);
} 