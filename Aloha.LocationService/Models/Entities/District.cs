using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Aloha.LocationService.Models.Entities
{
    public class District
    {
        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [BsonElement("code")]
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [BsonElement("division_type")]
        [JsonPropertyName("division_type")]
        public string DivisionType { get; set; }   // ví dụ: "huyện", "quận"

        [BsonElement("codename")]
        [JsonPropertyName("codename")]
        public string Codename { get; set; }        // ví dụ: "huyen_ba_dinh"

        [BsonElement("short_codename")]
        [JsonPropertyName("short_codename")]
        public string ShortCodename { get; set; }   // "ba_dinh"

        [BsonElement("wards")]
        [JsonPropertyName("wards")]
        public List<Ward> Wards { get; set; } = new();
    }
}
