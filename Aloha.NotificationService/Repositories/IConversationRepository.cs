using Aloha.NotificationService.Models.Entities;

namespace Aloha.NotificationService.Repositories
{
    public interface IConversationRepository : IRepository<Conversation>
    {
        Task<IEnumerable<Conversation>> GetConversationsByUserIdAsync(string userId);
        Task<Conversation?> GetConversationByParticipantsAsync(string[] userIds, string? productId = null);
        Task<bool> IsUserInConversationAsync(string userId, string conversationId);
        Task<bool> UpdateLastMessageAsync(string conversationId, DateTime lastMessageAt);
        Task<bool> AddParticipantAsync(string conversationId, ConversationParticipant participant);
        Task<bool> RemoveParticipantAsync(string conversationId, string userId);
        Task<bool> UpdateParticipantOnlineStatusAsync(string conversationId, string userId, bool isOnline);
        Task<bool> UpdateParticipantLastReadAsync(string conversationId, string userId, DateTime lastReadAt);
        Task<IEnumerable<ConversationParticipant>> GetConversationParticipantsAsync(string conversationId);
    }
}