using Aloha.LocationService.Models.Entities;
using MongoDB.Driver;
using System.Text.Json;

namespace Aloha.LocationService.Data
{
    public class DataSeeder
    {
        private readonly MongoContext _ctx;
        public DataSeeder(MongoContext ctx) => _ctx = ctx;

        public void SeedAsync()
        {
            var count = _ctx.Provinces.CountDocuments(Builders<Province>.Filter.Empty);
            if (count > 0) return;  // Only seed first time

            // Get path to provinces.json in the LocationService project
            var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "provinces.json");

            if (!File.Exists(jsonPath))
            {
                // Try to find file in source code location if not found in output directory
                jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "provinces.json");
            }

            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException($"provinces.json not found at {jsonPath}");
            }

            var json = File.ReadAllText(jsonPath);
            var list = JsonSerializer.Deserialize<List<Province>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            _ctx.Provinces.InsertMany(list);
        }
    }
}
