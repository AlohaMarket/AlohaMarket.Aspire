using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Aloha.LocationService.Models.Entities
{
    public class Ward
    {
        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [BsonElement("code")]
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [BsonElement("division_type")]
        [JsonPropertyName("division_type")]
        public string DivisionType { get; set; }   // "xã", "phường"

        [BsonElement("codename")]
        [JsonPropertyName("codename")]
        public string Codename { get; set; }        // "xa_ba_dinh"

        [BsonElement("short_codename")]
        [JsonPropertyName("short_codename")]
        public string ShortCodename { get; set; }   // "ba_dinh"
    }
}
