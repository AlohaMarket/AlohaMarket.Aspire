using System.Text.Json.Serialization;

namespace Aloha.Shared.Meta
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")] public T? Data { get; set; }
    }
}
