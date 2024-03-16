using System.Text;
using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function
{
    public class SummarizeText
    {
        private readonly ILogger<SummarizeText> _logger;
        private string _url;
        private string _subscriptionKey;

        public SummarizeText(ILogger<SummarizeText> logger)
        {
            _logger = logger;
            _url = Environment.GetEnvironmentVariable("TextAnalyticsClientUrl");
            _subscriptionKey = Environment.GetEnvironmentVariable("TextAnalyticsAPIKey");
        }

        [Function("SummarizeText")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "text/summarize")] HttpRequest req)
        {
            _logger.LogInformation("Summarize Text processing a request");
            IFormFile file = req.Form.Files["file"]; // Assuming file input name is "file"
            if (file != null && file.Length > 0)
            {

                if (!IsTextContentType(file.ContentType))
                {
                    return new BadRequestObjectResult("Unsupported content type. Only text-based files are supported.");
                }

                var summary = await Summarize(file.OpenReadStream());

                // Process the file content as needed
                _logger.LogInformation($"Processed file: {file.FileName}, Size: {file.Length} bytes");
                return new OkObjectResult(new { summary = summary});
            }
            else
            {
                return new BadRequestObjectResult("No file processed");
            }
        }
        private async Task<string> Summarize(Stream stream)
        {
            string textToAnalyze = "";
            textToAnalyze = ReadTextFromStream(stream);

            Uri endpoint = new(_url);
            AzureKeyCredential credential = new(_subscriptionKey);
            TextAnalyticsClient client = new(endpoint, credential);

            List<string> batchedDocuments = new()
            {
                textToAnalyze
            };

            try
            {
                StringBuilder builder = new StringBuilder();
                AbstractiveSummarizeOperation operation = await client.AbstractiveSummarizeAsync(WaitUntil.Completed, batchedDocuments);

                await foreach (AbstractiveSummarizeResultCollection documentsInPage in operation.Value)
                {
                    Console.WriteLine($"Extractive Summarize, version: \"{documentsInPage.ModelVersion}\"");
                    Console.WriteLine();

                    foreach (AbstractiveSummarizeResult documentResult in documentsInPage)
                    {
                        if (documentResult.HasError)
                        {
                            Console.WriteLine($"  Error!");
                            Console.WriteLine($"  Document error code: {documentResult.Error.ErrorCode}");
                            Console.WriteLine($"  Message: {documentResult.Error.Message}");
                            continue;
                        }

                        Console.WriteLine($"  Produced the following abstractive summaries:");
                        Console.WriteLine();

                        foreach (AbstractiveSummary summary in documentResult.Summaries)
                        {
                            builder.AppendLine(summary.Text.Replace("\n", " "));
                        }
                    }
                }

                return builder.ToString();

            }
            catch (RequestFailedException exception)
            {
                Console.WriteLine($"Error Code: {exception.ErrorCode}");
                Console.WriteLine($"Message: {exception.Message}");
                return exception.Message;
            }

        }
        static string ReadTextFromStream(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
        private bool IsTextContentType(string contentType)
        {
            // Add more supported content types as needed
            return contentType.StartsWith("text/") || contentType == "application/json";
        }
    }

}
