using System.Text.Json.Serialization;

namespace CsvConverter.Interfaces
{
    public class FileRequest
    {
        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;
    }
}
