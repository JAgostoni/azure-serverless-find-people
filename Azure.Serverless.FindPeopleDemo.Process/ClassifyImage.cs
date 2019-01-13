using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Azure.Serverless.FindPeopleDemo.Process
{
    public static class ClassifyImage
    {
        [FunctionName("ClassifyImage")]
        public async static void Run([ServiceBusTrigger("images-to-classify", Connection = "ProcessQueue")]string myQueueItem, ILogger log)
        {
            log.LogInformation("Triggered image classifier");

            var image = JsonConvert.DeserializeObject<ImageToClassify>(myQueueItem);

            var faceClient = new FaceClient(new ApiKeyServiceClientCredentials(Environment.GetEnvironmentVariable("CVKey")));
            faceClient.Endpoint = Environment.GetEnvironmentVariable("CVEndpoint");

            try
            {
                var faces = await faceClient.Face.DetectWithUrlAsync(
                    image.imageUrl, 
                    returnFaceAttributes: new List<FaceAttributeType>(1){ FaceAttributeType.Emotion }
                );

                foreach (var face in faces)
                {
                    var emotion = face.FaceAttributes.Emotion;
                    if (emotion.Happiness > 0.5f)
                    {
                        log.LogInformation($"Emotion matched for {image.imageUrl}");
                        break;
                    }
                }
            } catch(Exception ex)
            {
                log.LogError(ex, "Face detection failed");
            }
            
        }
    }


    class ImageToClassify
    {
        public string emotion { get; set; }
        public string imageUrl { get; set; }
    }
}
