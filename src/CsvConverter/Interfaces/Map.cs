using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CsvConverter.Interfaces
{
    public class Map
    {
        [JsonPropertyName("delimiter")]
        public string Delimiter { get; set; } = string.Empty;

        [JsonPropertyName("useQuotes")]
        public bool UseQuotes { get; set; } = false;

        [JsonPropertyName("fieldMappings")]
        public List<FieldMapping> FieldMappings { get; set; } = new List<FieldMapping>();
    }
}
