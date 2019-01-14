using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace Azure.Serverless.FindPeopleDemo.Process
{
    public static class ClassifyImage
    {
        [FunctionName("ClassifyImage")]
        public async static void Run(
            [ServiceBusTrigger("images-to-classify", Connection = "ProcessQueue")]string myQueueItem,
            ILogger log,
            Binder binder
        )
        {
            log.LogInformation("Triggered image classifier");

            var image = JsonConvert.DeserializeObject<ImageToClassify>(myQueueItem);
            var matched = false;

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
                    if (MatchEmotion(image.emotion, face.FaceAttributes.Emotion))
                    {
                        log.LogInformation($"Emotion matched for {image.imageUrl}");
                        matched = true;
                        break;
                    }
                }
            } catch(Exception ex)
            {
                matched = false;
                log.LogError(ex, "Face detection failed");
            }


            if(matched)
            {
                // Save to output
                var download = new HttpClient();
                try
                {
                    using (var httpStream = await download.GetStreamAsync(image.imageUrl))
                    {
                        var path = $@"images/{image.emotion}/{image.fileName}";
                        using (Stream destination = await binder.BindAsync<CloudBlobStream>(new BlobAttribute(path, FileAccess.Write)))
                        {
                            await httpStream.CopyToAsync(destination);
                            await httpStream.FlushAsync();
                            await destination.FlushAsync();
                        }
                    }
                } catch(Exception ex)
                {
                    log.LogError(ex, "Image download failed");
                }
            }
            
        }

        private static bool MatchEmotion(string criteria, Emotion detectedEmotions, double threshold = 0.5d)
        {
            var emotion = 0.0d;
            switch (criteria)
            {
                case "anger":
                case "angry":
                case "angriness":
                    emotion = detectedEmotions.Anger;
                    break;
                case "contempt":
                    emotion = detectedEmotions.Contempt;
                    break;
                case "disgust":
                case "disgusted":
                    emotion = detectedEmotions.Disgust;
                    break;
                case "fear":
                    emotion = detectedEmotions.Fear;
                    break;
                case "sad":
                case "sadness":
                case "unhappy":
                    emotion = detectedEmotions.Sadness;
                    break;
                case "surprise":
                    emotion = detectedEmotions.Surprise;
                    break;
                default:
                    emotion = detectedEmotions.Happiness;
                    break;
            }

            return emotion >= threshold;
        }
    }


    class ImageToClassify
    {
        public string emotion { get; set; }
        public string imageUrl { get; set; }
        public string fileName { get; set; }
    }
}
