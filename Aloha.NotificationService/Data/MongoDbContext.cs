using MongoDB.Driver;
using Microsoft.Extensions.Options;
using Aloha.NotificationService.Models.Entities;

namespace Aloha.NotificationService.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<Message> Messages => _database.GetCollection<Message>("messages");
        public IMongoCollection<Conversation> Conversations => _database.GetCollection<Conversation>("conversations");
    }
} 