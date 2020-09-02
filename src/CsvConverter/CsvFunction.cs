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
using System.Collections.Generic;
using System;
using System.Net.Mime;

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

            var csvContent = await _client.GetStringAsync(request.CsvUri);
            var mapContent = await _client.GetFromJsonAsync<FieldMapping[]>(request.MapUri);

            using var reader = new StringReader(csvContent);
            using var parser = new TextFieldParser(reader);

            parser.SetDelimiters(new string[] { ",", "\t" });
            parser.HasFieldsEnclosedInQuotes = true;

            var headers = parser.ReadFields();

            var sharedFields = mapContent.Select(m => m.Name).Union(headers).Count();
            if (sharedFields != headers.Length)
            {
                return new BadRequestObjectResult(
                    new
                    {
                        error = "Invalid map file for given csv file."
                    });
            }

            var map = new Dictionary<string, MapType>();
            foreach (var mapItem in mapContent)
            {
                var type = mapItem.Type switch
                {
                    "string" => MapType.String,
                    "integer" => MapType.Integer,
                    "number" => MapType.Number,
                    "datetime" => MapType.DateTime,
                    _ => MapType.String
                };

                map.Add(mapItem.Name, type);
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
                    var fieldMap = map[header];

                    if (string.IsNullOrEmpty(field))
                    {
                        writer.WriteNull(header);
                    }
                    else
                    {
                        switch (fieldMap)
                        {
                            case MapType.Integer:
                                if (int.TryParse(field, out var intResult))
                                {
                                    writer.WriteNumber(header, intResult);
                                }
                                else
                                {
                                    writer.WriteNull(header);
                                }
                                break;
                            case MapType.Number:
                                if (double.TryParse(field, out var doubleResult))
                                {
                                    writer.WriteNumber(header, doubleResult);
                                }
                                else
                                {
                                    writer.WriteNull(header);
                                }
                                break;
                            case MapType.DateTime:
                                if (DateTime.TryParse(field, out var dateResult))
                                {
                                    writer.WriteString(header, dateResult);
                                }
                                else
                                {
                                    writer.WriteNull(header);
                                }
                                break;
                            default:
                                writer.WriteString(header, field);
                                break;
                        }
                    }
                }

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            await writer.FlushAsync();

            output.Seek(0, SeekOrigin.Begin);
            var json = Encoding.UTF8.GetString(output.ToArray());
            return new ContentResult()
            {
                Content = json,
                ContentType = MediaTypeNames.Application.Json,
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
