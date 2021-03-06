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
using Microsoft.Azure.WebJobs.ServiceBus;

namespace Azure.Serverless.FindPeopleDemo.Process
{
    public static class StartSearch
    {
        [FunctionName("StartSearch")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log, 
            [ServiceBus("images-to-classify", Connection = "ProcessQueue", EntityType = EntityType.Queue)]IAsyncCollector<string> outputMessages)
        {

            string emotion = req.Query["emotion"].FirstOrDefault() ?? "happy";
            string criteria = req.Query["search"].FirstOrDefault() ?? "random people with different emotions";

            log.LogInformation($"Finding people with emotion: {emotion}");
            
            // Get images from a Bing search
            var key = System.Environment.GetEnvironmentVariable("BingSearchKey");
            var search = new ImageSearchClient(new ApiKeyServiceClientCredentials(key));
            var results = await search.Images.SearchAsync(criteria, safeSearch: "Strict", count: 100);

            //var connection = Environment.GetEnvironmentVariable("ProcessQueue");
            //var q = new QueueClient(connection, "images-to-classify");
            foreach (var result in results.Value)
            {
                var data = $"{{ 'emotion': '{emotion}', 'imageUrl': '{result.ContentUrl}', 'fileName': '{result.ImageId + '.' + result.EncodingFormat}' }}";

                await outputMessages.AddAsync(data);

            }
            
            return new OkResult();
        }
    }
}
