using Aloha.LocationService.Models.Entities;
using Aloha.LocationService.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Aloha.LocationService.Data
{
    public class MongoContext
    {
        public IMongoCollection<Province> Provinces { get; }

        public MongoContext(IMongoClient mongoClient, IOptions<MongoSettings> settings)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            Provinces = database.GetCollection<Province>("provinces");

            // Tạo index trên Code và Codename để lookup nhanh
            var idxBuilder = Builders<Province>.IndexKeys;
            var idxModel = new CreateIndexModel<Province>(
                idxBuilder.Ascending(p => p.Code), new CreateIndexOptions { Unique = true });
            Provinces.Indexes.CreateOne(idxModel);

            Provinces.Indexes.CreateOne(new CreateIndexModel<Province>(
                idxBuilder.Ascending(p => p.Codename), new CreateIndexOptions { Unique = true }));
        }
    }
}
