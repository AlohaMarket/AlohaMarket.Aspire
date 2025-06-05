using Aloha.NotificationService.Models.Entities;

namespace Aloha.NotificationService.Data
{
    public interface IMessageRepository : IRepository<Message>
    {
        Task<IEnumerable<Message>> GetMessagesByConversationIdAsync(string conversationId, int page = 1, int pageSize = 50);
        Task<Message?> GetLastMessageByConversationIdAsync(string conversationId);
        Task<bool> MarkMessagesAsReadAsync(string userId, string[] messageIds);
        Task<long> GetUnreadMessageCountAsync(string userId, string conversationId);
        Task<bool> DeleteMessageAsync(string messageId, string userId);
        Task<Message?> EditMessageAsync(string messageId, string newContent, string userId);
        Task<IEnumerable<Message>> SearchMessagesAsync(string conversationId, string searchTerm);
    }
} 