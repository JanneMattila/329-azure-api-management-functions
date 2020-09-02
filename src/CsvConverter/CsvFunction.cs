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
                string.IsNullOrWhiteSpace(request.Uri))
            {
                return new BadRequestObjectResult(
                    new
                    {
                        error = "Missing required query parameter 'uri'."
                    });
            }

            var csv = await _client.GetStringAsync(request.Uri);

            using var reader = new StringReader(csv);
            using var parser = new TextFieldParser(reader);

            parser.SetDelimiters(new string[] { ",", "\t" });
            parser.HasFieldsEnclosedInQuotes = true;

            var header = parser.ReadFields();
            while (!parser.EndOfData)
            {
                var data = parser.ReadFields();
            }

            return new OkObjectResult(
                new
                {
                    length = csv.Length
                });
        }
    }
}
