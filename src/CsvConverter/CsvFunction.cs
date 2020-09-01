using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using CsvConverter.Interfaces;
using System.Text.Json;
using System.Net.Http;

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
            log.LogInformation("C# HTTP trigger function processed a request.");

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

            var data = await _client.GetStringAsync(request.Uri);

            return new OkObjectResult(
                new
                {
                    length = data.Length
                });
        }
    }
}
