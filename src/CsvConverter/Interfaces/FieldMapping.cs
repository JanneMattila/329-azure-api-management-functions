using System.Text.Json.Serialization;

namespace CsvConverter.Interfaces
{
    public class FieldMapping
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }
}
