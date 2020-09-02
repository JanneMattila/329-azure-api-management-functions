using System.Text.Json.Serialization;

namespace CsvConverter.Interfaces
{
    public class FileRequest
    {
        [JsonPropertyName("csv")]
        public string CsvUri { get; set; } = string.Empty;

        [JsonPropertyName("map")]
        public string MapUri { get; set; } = string.Empty;
    }
}
