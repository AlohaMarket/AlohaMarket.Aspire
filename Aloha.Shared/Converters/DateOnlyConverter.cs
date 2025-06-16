using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aloha.Shared.Converters
{
    public class DateOnlyConverter : JsonConverter<DateOnly>
    {
        private readonly string[] _supportedFormats =
        [
            "yyyy/MM/dd",
            "dd/MM/yyyy",
            "yyyy-MM-dd",
            "dd-MM-yyyy"
        ];

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();

            if (DateOnly.TryParseExact(dateString, _supportedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }

            throw new JsonException($"Invalid date format. Use [{string.Join(", ", _supportedFormats)}].");
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd")); // Standardize response format
        }
    }
}
