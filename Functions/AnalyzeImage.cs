using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function
{
    public class AnalyzeImage
    {
        private readonly ILogger<AnalyzeImage> _logger;
        private string _url;
        private string _subscriptionKey;

        public AnalyzeImage(ILogger<AnalyzeImage> logger)
        {
            _logger = logger;
            _url = Environment.GetEnvironmentVariable("CognitiveServicesUrl");
            _subscriptionKey = Environment.GetEnvironmentVariable("CognitiveServicesSubscriptionKey");
        }

        [Function("AnalyzeImage")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route ="image/keywords")] HttpRequest req)
        {
            _logger.LogInformation("Keywords function processing request...");

            IFormFile file = req.Form.Files["file"]; // Assuming file input name is "file"


            if (file != null && file.Length > 0)
            {

                var keywords = await MakeCognitiveServicesRequest(file.OpenReadStream());
                // Process the file content as needed
                _logger.LogInformation($"Processed file: {file.FileName}, Size: {file.Length} bytes");
                return new OkObjectResult(keywords);
            }
            else
            {
                return new BadRequestObjectResult("No file processed");
            }
        }
        private async Task<string> MakeCognitiveServicesRequest(Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                byte[] byteArray = memoryStream.ToArray();

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);

                    var content = new ByteArrayContent(byteArray);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    content.Headers.ContentLength = byteArray.Length;



                    var response = await client.PostAsync(_url, content);
                    string result = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Response: {response.StatusCode}");
                    return result;
                }
            }
        }
        
    }
}
