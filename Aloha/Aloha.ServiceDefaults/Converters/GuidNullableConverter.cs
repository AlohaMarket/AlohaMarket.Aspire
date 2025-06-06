using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aloha.ServiceDefaults.Converters
{
    public class GuidNullableConverter : JsonConverter<Guid?>
    {
        public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            if (Guid.TryParse(value, out var guid))
            {
                return guid;
            }
            throw new ArgumentException($"Invalid GUID format: {value}");
        }

        public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value.ToString());
        }
    }
}
