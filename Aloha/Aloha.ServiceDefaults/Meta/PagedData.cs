using System.Text.Json.Serialization;

namespace Aloha.ServiceDefaults.Meta
{
    public class PagedData<T>
    {
        [JsonPropertyName("items")]
        public required IEnumerable<T> Items { get; set; }

        [JsonPropertyName("meta")]
        public required PaginationMeta Meta { get; set; }
    }
}
