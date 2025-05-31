using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aloha.ServiceDefaults.Converters
{
    public class StringEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Invalid data type for {typeof(T).Name}. Expected a string.");
            }

            string? value = reader.GetString();

            if (Enum.TryParse(value, true, out T result)) // Case-insensitive parsing
            {
                return result;
            }

            throw new JsonException($"Invalid value '{value}' for {typeof(T).Name}. Allowed values: {string.Join(", ", Enum.GetNames(typeof(T)))}.");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
