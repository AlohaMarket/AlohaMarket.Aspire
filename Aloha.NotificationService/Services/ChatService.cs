using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Models;
using Aloha.NotificationService.Models.DTOs;
using Aloha.NotificationService.Models.Entities;
using Aloha.NotificationService.Repositories;

namespace Aloha.NotificationService.Services
{
    public class ChatService : IChatService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly ILogger<ChatService> _logger;
        private readonly IEventPublisher _eventPublisher;
        private readonly IUserProfileCache _userCache;
        private readonly IPostInfoCache _postCache;

        // Fallback mock users only for development/testing
        private static readonly Dictionary<string, UserDto> _fallbackUsers = new();

        public ChatService(
            IMessageRepository messageRepository,
            IConversationRepository conversationRepository,
            ILogger<ChatService> logger,
            IEventPublisher eventPublisher,
            IUserProfileCache userCache,
            IPostInfoCache postCache)
        {
            _messageRepository = messageRepository;
            _conversationRepository = conversationRepository;
            _logger = logger;
            _eventPublisher = eventPublisher;
            _userCache = userCache;
            _postCache = postCache;
        }

        public async Task<UserDto?> GetUser(string userId)
        {
            try
            {
                // First, check cache
                var cachedUser = await _userCache.GetUserAsync(userId);
                if (cachedUser != null)
                {
                    _logger.LogDebug("User {UserId} found in cache", userId);
                    return cachedUser;
                }

                // Request from UserService via Kafka
                _logger.LogInformation("Requesting user profile from UserService for userId: {UserId}", userId);

                await _eventPublisher.PublishAsync(new UserChatRequestEventModel
                {
                    UserId = userId,
                    RequestingService = "NotificationService"
                });

                // Wait for response with timeout (polling approach)
                var maxWaitTime = TimeSpan.FromSeconds(3);
                var pollingInterval = TimeSpan.FromMilliseconds(100);
                var startTime = DateTime.UtcNow;

                while (DateTime.UtcNow - startTime < maxWaitTime)
                {
                    await Task.Delay(pollingInterval);

                    cachedUser = await _userCache.GetUserAsync(userId);
                    if (cachedUser != null)
                    {
                        _logger.LogInformation("User profile received and cached for userId: {UserId}", userId);
                        return cachedUser;
                    }
                }

                _logger.LogWarning("Timeout waiting for user profile from UserService for userId: {UserId}", userId);

                // Fallback to mock user for development
                return await CreateFallbackUser(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile for userId: {UserId}", userId);
                return await CreateFallbackUser(userId);
            }
        }

        private async Task<UserDto> CreateFallbackUser(string userId)
        {
            if (_fallbackUsers.TryGetValue(userId, out var existingUser))
            {
                return existingUser;
            }

            var fallbackUser = new UserDto
            {
                Id = userId,
                Name = $"User {userId}",
                Email = $"user{userId}@fallback.com",
                Avatar = $"https://api.dicebear.com/7.x/avataaars/svg?seed={userId}",
                IsOnline = false
            };

            _fallbackUsers[userId] = fallbackUser;

            // Also cache it for consistency
            await _userCache.SetUserAsync(fallbackUser);

            _logger.LogWarning("Created fallback user for userId: {UserId}", userId);
            return fallbackUser;
        }

        public async Task SetUserOnlineStatus(string userId, bool isOnline)
        {
            // Update cached user online status
            var user = await _userCache.GetUserAsync(userId);
            if (user != null)
            {
                user.IsOnline = isOnline;
                await _userCache.SetUserAsync(user);
                _logger.LogDebug("Updated online status for user {UserId}: {IsOnline}", userId, isOnline);
            }

            // Update in fallback storage if exists
            if (_fallbackUsers.TryGetValue(userId, out var fallbackUser))
            {
                fallbackUser.IsOnline = isOnline;
                _fallbackUsers[userId] = fallbackUser;
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

        public async Task<Conversation> CreateOrGetConversation(string[] userIds, string? productId)
        {
            // Check if conversation already exists between these users (ignore productId)
            var existingConversation = await _conversationRepository.GetConversationByParticipantsAsync(userIds, null);
            if (existingConversation != null)
            {
                // If we have a different productId, update the existing conversation
                if (existingConversation.ProductId != productId)
                {
                    _logger.LogInformation("Updating existing conversation {ConversationId} with new product: {ProductId}", 
                        existingConversation.Id, productId);
                    
                    var updatedConversation = await UpdateConversationProduct(existingConversation.Id, productId);
                    return updatedConversation ?? existingConversation;
                }
                
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

            // Get post info if productId is provided
            PostDto? postInfo = null;
            ProductContext? productContext = null;
            string conversationType = "chat"; // Default to simple chat

            if (!string.IsNullOrEmpty(productId))
            {
                postInfo = await GetPostInfo(productId);
                if (postInfo != null)
                {
                    conversationType = "product"; // Set to product conversation
                    productContext = new ProductContext
                    {
                        ProductId = postInfo.Id,
                        ProductName = postInfo.Title,
                        ProductImage = postInfo.ThumbnailUrl,
                        ProductPrice = postInfo.Price,
                        // Note: You'll need to get seller info from post or users
                        SellerId = "", // You might need to add this to PostDto
                        SellerName = "" // You might need to add this to PostDto
                    };

                    _logger.LogInformation("Created product context for conversation with PostId: {PostId}, Title: {Title}",
                        postInfo.Id, postInfo.Title);
                }
                else
                {
                    _logger.LogWarning("Could not retrieve post info for ProductId: {ProductId}", productId);
                }
            }

            // Create new conversation
            var conversation = new Conversation
            {
                ConversationType = conversationType,
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
                ProductContext = productContext
            };

            return await _conversationRepository.CreateAsync(conversation);
        }

        public async Task<IEnumerable<Conversation>> GetUserConversations(string userId)
        {
            return await _conversationRepository.GetConversationsByUserIdAsync(userId);
        }

        public async Task<Conversation?> UpdateConversationProduct(string conversationId, string? productId)
        {
            // First get the existing conversation
            var conversation = await _conversationRepository.GetByIdAsync(conversationId);
            if (conversation == null)
            {
                _logger.LogWarning("Conversation not found for id: {ConversationId}", conversationId);
                return null;
            }

            // Get post info if productId is provided
            ProductContext? productContext = null;
            string conversationType = "chat"; // Default to simple chat

            if (!string.IsNullOrEmpty(productId))
            {
                var postInfo = await GetPostInfo(productId);
                if (postInfo != null)
                {
                    conversationType = "product"; // Set to product conversation
                    productContext = new ProductContext
                    {
                        ProductId = postInfo.Id,
                        ProductName = postInfo.Title,
                        ProductImage = postInfo.ThumbnailUrl,
                        ProductPrice = postInfo.Price,
                        SellerId = "", // You might need to add this to PostDto
                        SellerName = "" // You might need to add this to PostDto
                    };

                    _logger.LogInformation("Updated product context for conversation {ConversationId} with PostId: {PostId}", 
                        conversationId, postInfo.Id);
                }
                else
                {
                    _logger.LogWarning("Could not retrieve post info for ProductId: {ProductId}", productId);
                }
            }
            else
            {
                _logger.LogInformation("Removed product context from conversation {ConversationId}", conversationId);
            }

            // Update the conversation in the database with all the new context
            var updateSuccess = await _conversationRepository.UpdateConversationProductAsync(
                conversationId, 
                productId, 
                conversationType, 
                productContext);

            if (!updateSuccess)
            {
                _logger.LogError("Failed to update conversation {ConversationId} in database", conversationId);
                return null;
            }

            // Update the conversation object with new context for return
            conversation.ProductId = productId;
            conversation.ConversationType = conversationType;
            conversation.ProductContext = productContext;
            conversation.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Successfully updated conversation {ConversationId} with new product context", conversationId);
            return conversation;
        }

        public async Task<IEnumerable<Message>> GetConversationMessages(string conversationId, int page = 1, int pageSize = 50)
        {
            return await _messageRepository.GetMessagesByConversationIdAsync(conversationId, page, pageSize);
        }

        public async Task<long> GetUnreadMessageCount(string userId, string conversationId)
        {
            return await _messageRepository.GetUnreadMessageCountAsync(userId, conversationId);
        }

        public async Task<PostDto?> GetPostInfo(string postId)
        {
            try
            {
                // First, check cache
                var cachedPost = await _postCache.GetPostAsync(postId);
                if (cachedPost != null)
                {
                    _logger.LogDebug("Post {PostId} found in cache", postId);
                    return cachedPost;
                }

                // Request from PostService via Kafka
                _logger.LogInformation("Requesting post info from PostService for postId: {PostId}", postId);

                await _eventPublisher.PublishAsync(new PostChatRequestEventModel
                {
                    PostId = postId,
                    RequestingService = "NotificationService"
                });

                // Wait for response with timeout (polling approach)
                var maxWaitTime = TimeSpan.FromSeconds(5);
                var pollingInterval = TimeSpan.FromMilliseconds(100);
                var startTime = DateTime.UtcNow;

                while (DateTime.UtcNow - startTime < maxWaitTime)
                {
                    await Task.Delay(pollingInterval);

                    cachedPost = await _postCache.GetPostAsync(postId);
                    if (cachedPost != null)
                    {
                        _logger.LogInformation("Post info received and cached for postId: {PostId}", postId);
                        return cachedPost;
                    }
                }

                _logger.LogWarning("Timeout waiting for post info from PostService for postId: {PostId}", postId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post info for postId: {PostId}", postId);
                return null;
            }
        }
    }
}
