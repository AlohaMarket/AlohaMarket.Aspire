using Aloha.NotificationService.Models.Entities;
using Aloha.NotificationService.Models.DTOs;
using Aloha.NotificationService.Repositories;

namespace Aloha.NotificationService.Services
{
    public class ChatService : IChatService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly ILogger<ChatService> _logger;
        
        // Simple in-memory storage for testing user data
        private static readonly Dictionary<string, UserDto> _mockUsers = new();

        public ChatService(
            IMessageRepository messageRepository,
            IConversationRepository conversationRepository,
            ILogger<ChatService> logger)
        {
            _messageRepository = messageRepository;
            _conversationRepository = conversationRepository;
            _logger = logger;
        }

        public async Task<UserDto?> GetUser(string userId)
       {
            // For testing, create mock user data if doesn't exist
            if (!_mockUsers.ContainsKey(userId))
            {
                var mockUser = new UserDto
                {
                    Id = userId,
                    Name = $"User {userId}",
                    Email = $"user{userId}@test.com",
                    Avatar = $"https://api.dicebear.com/7.x/avataaars/svg?seed={userId}",
                    IsOnline = false
                };
                _mockUsers[userId] = mockUser;
            }
            
            return await Task.FromResult(_mockUsers[userId]);
        }

        public async Task SetUserOnlineStatus(string userId, bool isOnline)
        {
            // Update mock user online status
            var user = await GetUser(userId);
            if (user != null)
            {
                user.IsOnline = isOnline;
                _mockUsers[userId] = user;
            }

            // Update in all conversations where user is participant
            var conversations = await _conversationRepository.GetConversationsByUserIdAsync(userId);
            foreach (var conversation in conversations)
            {
                await _conversationRepository.UpdateParticipantOnlineStatusAsync(conversation.Id, userId, isOnline);
            }
        }

        public async Task<bool> IsUserInConversation(string userId, string conversationId)
        {
            return await _conversationRepository.IsUserInConversationAsync(userId, conversationId);
        }

        public async Task<List<UserDto>> GetConversationParticipants(string conversationId)
        {
            var participants = await _conversationRepository.GetConversationParticipantsAsync(conversationId);
            var users = new List<UserDto>();
            
            foreach (var participant in participants)
            {
                var user = await GetUser(participant.UserId);
                if (user != null)
                {
                    users.Add(user);
                }
            }
            
            return users;
        }

        public async Task<Message> CreateMessage(CreateMessageDto dto)
        {
            // Get or create sender info
            var sender = await GetUser(dto.SenderId);
            if (sender == null)
            {
                throw new ArgumentException($"Invalid sender ID: {dto.SenderId}");
            }

            var message = new Message
            {
                ConversationId = dto.ConversationId,
                SenderId = dto.SenderId,
                SenderName = sender.Name,
                SenderAvatar = sender.Avatar,
                Content = dto.Content,
                MessageType = dto.MessageType,
                Timestamp = DateTime.UtcNow,
                IsRead = false,
                IsEdited = false
            };

            var createdMessage = await _messageRepository.CreateAsync(message);

            // Update conversation's last message time
            await _conversationRepository.UpdateLastMessageAsync(dto.ConversationId, DateTime.UtcNow);

            return createdMessage;
        }

        public async Task<Message?> GetMessage(string messageId)
        {
            return await _messageRepository.GetByIdAsync(messageId);
        }

        public async Task<Message?> EditMessage(string messageId, string newContent)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message == null) return null;

            return await _messageRepository.EditMessageAsync(messageId, newContent, message.SenderId);
        }

        public async Task DeleteMessage(string messageId)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message != null)
            {
                await _messageRepository.DeleteMessageAsync(messageId, message.SenderId);
            }
        }

        public async Task MarkMessagesAsRead(string userId, string[] messageIds)
        {
            await _messageRepository.MarkMessagesAsReadAsync(userId, messageIds);
        }

        public async Task<Conversation> CreateOrGetConversation(string[] userIds)
        {
            // Check if conversation already exists
            var existingConversation = await _conversationRepository.GetConversationByParticipantsAsync(userIds, null);
            if (existingConversation != null)
            {
                return existingConversation;
            }

            // Get user info for all participants
            var users = new List<UserDto>();
            foreach (var userId in userIds)
            {
                var user = await GetUser(userId);
                if (user != null)
                {
                    users.Add(user);
                }
            }

            // Create new conversation
            var conversation = new Conversation
            {
                ConversationType = "chat", // Simple chat conversation
                ProductId = null,
                LastMessageAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Participants = users.Select(u => new ConversationParticipant
                {
                    UserId = u.Id,
                    UserName = u.Name,
                    UserEmail = u.Email,
                    UserAvatar = u.Avatar,
                    JoinedAt = DateTime.UtcNow,
                    LastReadAt = DateTime.UtcNow,
                    IsOnline = u.IsOnline
                }).ToList(),
                ProductContext = null // No product context needed
            };

            return await _conversationRepository.CreateAsync(conversation);
        }

        public async Task<IEnumerable<Conversation>> GetUserConversations(string userId)
        {
            return await _conversationRepository.GetConversationsByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Message>> GetConversationMessages(string conversationId, int page = 1, int pageSize = 50)
        {
            return await _messageRepository.GetMessagesByConversationIdAsync(conversationId, page, pageSize);
        }

        public async Task<long> GetUnreadMessageCount(string userId, string conversationId)
        {
            return await _messageRepository.GetUnreadMessageCountAsync(userId, conversationId);
        }
    }
}
