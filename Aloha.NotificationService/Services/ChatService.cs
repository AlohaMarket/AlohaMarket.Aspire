using Aloha.NotificationService.Data;
using Aloha.NotificationService.Models.Entities;
using Aloha.NotificationService.Models.DTOs;
using Aloha.NotificationService.Hubs;

namespace Aloha.NotificationService.Services
{
    public class ChatService : IChatService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            IMessageRepository messageRepository,
            IConversationRepository conversationRepository,
            IUserService userService,
            IProductService productService,
            ILogger<ChatService> logger)
        {
            _messageRepository = messageRepository;
            _conversationRepository = conversationRepository;
            _userService = userService;
            _productService = productService;
            _logger = logger;
        }

        public async Task<UserDto?> GetUser(string userId)
        {
            return await _userService.GetUserByIdAsync(userId);
        }

        public async Task SetUserOnlineStatus(string userId, bool isOnline)
        {
            // Update in User service
            await _userService.UpdateUserOnlineStatusAsync(userId, isOnline);

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
            var userIds = participants.Select(p => p.UserId).ToArray();
            var users = await _userService.GetUsersByIdsAsync(userIds);
            return users.ToList();
        }

        public async Task<Message> CreateMessage(CreateMessageDto dto)
        {
            // Get sender info from User service
            var sender = await _userService.GetUserByIdAsync(dto.SenderId);
            if (sender == null)
            {
                throw new InvalidOperationException($"User {dto.SenderId} not found");
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

        // Additional chat-specific methods
        public async Task<Conversation> CreateOrGetConversation(string[] userIds, string? productId = null)
        {
            // Check if conversation already exists
            var existingConversation = await _conversationRepository.GetConversationByParticipantsAsync(userIds, productId);
            if (existingConversation != null)
            {
                return existingConversation;
            }

            // Get user and product info
            var users = await _userService.GetUsersByIdsAsync(userIds);
            ProductDto? product = null;
            if (!string.IsNullOrEmpty(productId))
            {
                product = await _productService.GetProductByIdAsync(productId);
            }

            // Create new conversation
            var conversation = new Conversation
            {
                ConversationType = !string.IsNullOrEmpty(productId) ? "buyer_seller" : "support",
                ProductId = productId,
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
                ProductContext = product != null ? new ProductContext
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductImage = product.ImageUrl,
                    ProductPrice = product.Price,
                    SellerId = product.SellerId,
                    SellerName = product.SellerName
                } : null
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
