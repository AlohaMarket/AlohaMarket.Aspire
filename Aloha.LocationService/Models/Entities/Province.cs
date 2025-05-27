using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Aloha.LocationService.Models.Entities
{
    public class Province
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }            // "Thành phố Hà Nội"

        [BsonElement("code")]
        [JsonPropertyName("code")]
        public int Code { get; set; }               // 1, 2, 4, 6, …

        [BsonElement("division_type")]
        [JsonPropertyName("division_type")]
        public string DivisionType { get; set; }    // "tỉnh", "thành phố trung ương"

        [BsonElement("codename")]
        [JsonPropertyName("codename")]
        public string Codename { get; set; }        // "thanh_pho_ha_noi"

        [BsonElement("districts")]
        [JsonPropertyName("districts")]
        public List<District> Districts { get; set; } = new();
    }
}
