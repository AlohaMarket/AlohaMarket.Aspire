using Aloha.LocationService.Data;
using Aloha.LocationService.Models.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aloha.LocationService.Repositories
{
    public class LocationRepository(MongoContext ctx) : ILocationRepository
    {
        public async Task<IEnumerable<Province>> GetAllAsync()
        {
            var projection = Builders<Province>.Projection
                //.Exclude(p => p.Districts)
                ;

            return await ctx.Provinces
                .Find(_ => true)
                //.Project<Province>(projection)
                .ToListAsync();
        }

        public async Task<Province> GetByCodeAsync(int code)
            => await ctx.Provinces.Find(p => p.Code == code).FirstOrDefaultAsync();

        public async Task<Province> GetByCodenameAsync(string codename)
            => await ctx.Provinces.Find(p => p.Codename == codename).FirstOrDefaultAsync();

        public async Task<IEnumerable<Province>> SearchAsync(string keyword)
        {
            var filter = Builders<Province>.Filter.Or(
                Builders<Province>.Filter.Regex(p => p.Name, new BsonRegularExpression(keyword, "i")),
                Builders<Province>.Filter.Eq(p => p.Code, int.TryParse(keyword, out var c) ? c : -1)
            );
            return await ctx.Provinces.Find(filter).ToListAsync();
        }
    }
}
