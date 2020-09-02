using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using CsvConverter.Interfaces;
using System.Text.Json;
using System.Net.Http;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Text;
using System.Net.Http.Json;
using System.Linq;

namespace CsvConverter
{
    public static class CsvFunction
    {
        private static readonly HttpClient _client = new HttpClient();

        [FunctionName("csv")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Csv converter function processed a request.");

            var request = await JsonSerializer.DeserializeAsync<FileRequest>(req.Body);
            if (request is null || 
                string.IsNullOrWhiteSpace(request.CsvUri) ||
                string.IsNullOrWhiteSpace(request.MapUri))
            {
                return new BadRequestObjectResult(
                    new
                    {
                        error = "Missing mandatory parameters."
                    });
            }

            var csv = await _client.GetStringAsync(request.CsvUri);
            var map = await _client.GetFromJsonAsync<FieldMapping[]>(request.MapUri);

            using var reader = new StringReader(csv);
            using var parser = new TextFieldParser(reader);

            parser.SetDelimiters(new string[] { ",", "\t" });
            parser.HasFieldsEnclosedInQuotes = true;

            var headers = parser.ReadFields();

            var sharedFields = map.Select(m => m.Name).Union(headers).Count();
            if (sharedFields != headers.Length)
            {
                return new BadRequestObjectResult(
                    new
                    {
                        error = "Invalid map file for given csv file."
                    });
            }

            using var output = new MemoryStream();
            using var writer = new Utf8JsonWriter(output);

            writer.WriteStartArray();
            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();

                writer.WriteStartObject();
                for (int i = 0; i < headers.Length; i++)
                {
                    var header = headers[i];
                    var field = fields[i];

                    writer.WriteString(header, field);
                }

                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            var json = Encoding.UTF8.GetString(output.ToArray());
            return new OkObjectResult(json);
        }
    }
}
