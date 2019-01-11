using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.CognitiveServices.Search.ImageSearch;

namespace Azure.Serverless.FindPeopleDemo.Process
{
    public static class StartSearch
    {
        [FunctionName("StartSearch")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string emotion = req.Query["emotion"].FirstOrDefault() ?? "happy";

            // Get images from a Bing search
            var key = System.Environment.GetEnvironmentVariable("BingSearchKey");
            var search = new ImageSearchClient(new ApiKeyServiceClientCredentials(key));
            var results = await search.Images.SearchAsync("people", safeSearch: "Strict");

            var connection = Environment.GetEnvironmentVariable("ProcessQueue");
            var q = new QueueClient(connection, "images-to-classify");
            foreach (var result in results.Value.Take(10))
            {
                var data = System.Text.Encoding.UTF8.GetBytes($"{{ 'emotion': '{emotion}', 'imageUrl': '{result.ContentUrl}' }}");
                await q.SendAsync(new Message(data));
            }

            return new OkResult();
        }
    }
}
