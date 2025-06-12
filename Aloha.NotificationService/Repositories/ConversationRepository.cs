using MongoDB.Driver;
using MongoDB.Bson;
using Aloha.NotificationService.Models.Entities;
using Aloha.NotificationService.Data;

namespace Aloha.NotificationService.Repositories
{
    public class ConversationRepository : MongoRepository<Conversation>, IConversationRepository
    {
        private readonly IMongoCollection<Conversation> _conversations;

        public ConversationRepository(MongoDbContext context) : base(context.Conversations)
        {
            _conversations = context.Conversations;
        }

        public async Task<IEnumerable<Conversation>> GetConversationsByUserIdAsync(string userId)
        {
            var filter = Builders<Conversation>.Filter.ElemMatch(c => c.Participants, p => p.UserId == userId);
            return await _conversations
                .Find(filter)
                .SortByDescending(c => c.LastMessageAt)
                .ToListAsync();
        }

        public async Task<Conversation?> GetConversationByParticipantsAsync(string[] userIds, string? productId = null)
        {
            var participantFilters = userIds.Select(userId =>
                Builders<Conversation>.Filter.ElemMatch(c => c.Participants, p => p.UserId == userId)
            ).ToArray();

            var filter = Builders<Conversation>.Filter.And(participantFilters);

            if (!string.IsNullOrEmpty(productId))
            {
                filter = Builders<Conversation>.Filter.And(filter,
                    Builders<Conversation>.Filter.Eq(c => c.ProductId, productId));
            }

            return await _conversations.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> IsUserInConversationAsync(string userId, string conversationId)
        {
            var filter = Builders<Conversation>.Filter.And(
                Builders<Conversation>.Filter.Eq("_id", ObjectId.Parse(conversationId)),
                Builders<Conversation>.Filter.ElemMatch(c => c.Participants, p => p.UserId == userId)
            );

            var count = await _conversations.CountDocumentsAsync(filter);
            return count > 0;
        }

        public async Task<bool> UpdateLastMessageAsync(string conversationId, DateTime lastMessageAt)
        {
            var filter = Builders<Conversation>.Filter.Eq("_id", ObjectId.Parse(conversationId));
            var update = Builders<Conversation>.Update
                .Set(c => c.LastMessageAt, lastMessageAt)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);

            var result = await _conversations.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> AddParticipantAsync(string conversationId, ConversationParticipant participant)
        {
            var filter = Builders<Conversation>.Filter.Eq("_id", ObjectId.Parse(conversationId));
            var update = Builders<Conversation>.Update.AddToSet(c => c.Participants, participant);

            var result = await _conversations.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> RemoveParticipantAsync(string conversationId, string userId)
        {
            var filter = Builders<Conversation>.Filter.Eq("_id", ObjectId.Parse(conversationId));
            var update = Builders<Conversation>.Update.PullFilter(c => c.Participants, p => p.UserId == userId);

            var result = await _conversations.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateParticipantOnlineStatusAsync(string conversationId, string userId, bool isOnline)
        {
            var filter = Builders<Conversation>.Filter.And(
                Builders<Conversation>.Filter.Eq("_id", ObjectId.Parse(conversationId)),
                Builders<Conversation>.Filter.ElemMatch(c => c.Participants, p => p.UserId == userId)
            );

            var update = Builders<Conversation>.Update.Set("participants.$.isOnline", isOnline);

            var result = await _conversations.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateParticipantLastReadAsync(string conversationId, string userId, DateTime lastReadAt)
        {
            var filter = Builders<Conversation>.Filter.And(
                Builders<Conversation>.Filter.Eq("_id", ObjectId.Parse(conversationId)),
                Builders<Conversation>.Filter.ElemMatch(c => c.Participants, p => p.UserId == userId)
            );

            var update = Builders<Conversation>.Update.Set("participants.$.lastReadAt", lastReadAt);

            var result = await _conversations.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<IEnumerable<ConversationParticipant>> GetConversationParticipantsAsync(string conversationId)
        {
            var filter = Builders<Conversation>.Filter.Eq("_id", ObjectId.Parse(conversationId));
            var projection = Builders<Conversation>.Projection.Include(c => c.Participants);

            var conversation = await _conversations.Find(filter).Project(projection).FirstOrDefaultAsync();
            return conversation?["participants"]?.AsBsonArray
                .Select(p => MongoDB.Bson.Serialization.BsonSerializer.Deserialize<ConversationParticipant>(p.AsBsonDocument))
                ?? new List<ConversationParticipant>();
        }
    }
}