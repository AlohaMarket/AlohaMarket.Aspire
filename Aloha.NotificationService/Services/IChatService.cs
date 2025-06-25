using Aloha.NotificationService.Models.Entities;
using Aloha.NotificationService.Models.DTOs;

namespace Aloha.NotificationService.Services
{
    public interface IChatService
    {
        Task<UserDto?> GetUser(string userId);
        Task SetUserOnlineStatus(string userId, bool isOnline);

        // Conversation management
        Task<bool> IsUserInConversation(string userId, string conversationId);
        Task<List<UserDto>> GetConversationParticipants(string conversationId);
        Task<Conversation> CreateOrGetConversation(string[] userIds);
        Task<IEnumerable<Conversation>> GetUserConversations(string userId);

        // Message management
        Task<Message> CreateMessage(CreateMessageDto dto);
        Task<Message?> GetMessage(string messageId);
        Task<Message?> EditMessage(string messageId, string newContent);
        Task DeleteMessage(string messageId);
        Task MarkMessagesAsRead(string userId, string[] messageIds);
        Task<IEnumerable<Message>> GetConversationMessages(string conversationId, int page = 1, int pageSize = 50);
        Task<long> GetUnreadMessageCount(string userId, string conversationId);
    }
}
