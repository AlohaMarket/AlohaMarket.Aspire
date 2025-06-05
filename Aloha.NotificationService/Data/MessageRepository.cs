using MongoDB.Driver;
using MongoDB.Bson;
using Aloha.NotificationService.Models.Entities;

namespace Aloha.NotificationService.Data
{
    public class MessageRepository : MongoRepository<Message>, IMessageRepository
    {
        private readonly IMongoCollection<Message> _messages;

        public MessageRepository(MongoDbContext context) : base(context.Messages)
        {
            _messages = context.Messages;
        }

        public async Task<IEnumerable<Message>> GetMessagesByConversationIdAsync(string conversationId, int page = 1, int pageSize = 50)
        {
            var skip = (page - 1) * pageSize;
            return await _messages
                .Find(m => m.ConversationId == conversationId)
                .SortByDescending(m => m.Timestamp)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<Message?> GetLastMessageByConversationIdAsync(string conversationId)
        {
            return await _messages
                .Find(m => m.ConversationId == conversationId)
                .SortByDescending(m => m.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> MarkMessagesAsReadAsync(string userId, string[] messageIds)
        {
            var filter = Builders<Message>.Filter.In(m => m.Id, messageIds);
            var update = Builders<Message>.Update.AddToSet(m => m.ReadBy, new MessageReadStatus
            {
                UserId = userId,
                ReadAt = DateTime.UtcNow
            });

            var result = await _messages.UpdateManyAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<long> GetUnreadMessageCountAsync(string userId, string conversationId)
        {
            var filter = Builders<Message>.Filter.And(
                Builders<Message>.Filter.Eq(m => m.ConversationId, conversationId),
                Builders<Message>.Filter.Ne(m => m.SenderId, userId), // Not sent by the user
                Builders<Message>.Filter.Not(
                    Builders<Message>.Filter.ElemMatch(m => m.ReadBy, r => r.UserId == userId)
                )
            );

            return await _messages.CountDocumentsAsync(filter);
        }

        public async Task<bool> DeleteMessageAsync(string messageId, string userId)
        {
            var filter = Builders<Message>.Filter.And(
                Builders<Message>.Filter.Eq("_id", ObjectId.Parse(messageId)),
                Builders<Message>.Filter.Eq(m => m.SenderId, userId)
            );

            var result = await _messages.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<Message?> EditMessageAsync(string messageId, string newContent, string userId)
        {
            var filter = Builders<Message>.Filter.And(
                Builders<Message>.Filter.Eq("_id", ObjectId.Parse(messageId)),
                Builders<Message>.Filter.Eq(m => m.SenderId, userId)
            );

            var update = Builders<Message>.Update
                .Set(m => m.Content, newContent)
                .Set(m => m.IsEdited, true)
                .Set(m => m.EditedAt, DateTime.UtcNow);

            var options = new FindOneAndUpdateOptions<Message>
            {
                ReturnDocument = ReturnDocument.After
            };

            return await _messages.FindOneAndUpdateAsync(filter, update, options);
        }

        public async Task<IEnumerable<Message>> SearchMessagesAsync(string conversationId, string searchTerm)
        {
            var filter = Builders<Message>.Filter.And(
                Builders<Message>.Filter.Eq(m => m.ConversationId, conversationId),
                Builders<Message>.Filter.Regex(m => m.Content, new BsonRegularExpression(searchTerm, "i"))
            );

            return await _messages
                .Find(filter)
                .SortByDescending(m => m.Timestamp)
                .ToListAsync();
        }
    }
} 