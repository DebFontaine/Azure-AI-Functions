using System.Text;
using System.Text.Json;
using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function
{
    public class AnalyzeText
    {
        private readonly ILogger<AnalyzeText> _logger;
        private string _url;
        private string _subscriptionKey;

        public AnalyzeText(ILogger<AnalyzeText> logger)
        {
            _logger = logger;
            _url = Environment.GetEnvironmentVariable("TextAnalyticsClientUrl");
            _subscriptionKey = Environment.GetEnvironmentVariable("TextAnalyticsAPIKey");
        }

        [Function("AnalyzeText")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "text/keywords")] HttpRequest req)
        {
            _logger.LogInformation("Analyze Text processing a request");
            IFormFile file = req.Form.Files["file"]; // Assuming file input name is "file"
            if (file != null && file.Length > 0)
            {

                if (!IsTextContentType(file.ContentType))
                {
                    return new BadRequestObjectResult("Unsupported content type. Only text-based files are supported.");
                }

                var keywords = await AnalyzeTextForKeywords(file.OpenReadStream());

                // Process the file content as needed
                _logger.LogInformation($"Processed file: {file.FileName}, Size: {file.Length} bytes");
                return new OkObjectResult(new { keywords = keywords});
            }
            else
            {
                return new BadRequestObjectResult("No file processed");
            }
        }

        private async Task<string> AnalyzeTextForKeywords(Stream stream)
        {
            string textToAnalyze = "";
            textToAnalyze = ReadTextFromStream(stream);

            Uri endpoint = new(_url);
            AzureKeyCredential credential = new(_subscriptionKey);
            TextAnalyticsClient client = new(endpoint, credential);

            try
            {
                Response<KeyPhraseCollection> response = await client.ExtractKeyPhrasesAsync(textToAnalyze);
                KeyPhraseCollection keyPhrases = response.Value;
                List<string> keyPhraseList = keyPhrases.Select(phrase => phrase).ToList();

                var phrases = string.Join(',', keyPhraseList.ToArray());

                _logger.LogInformation($"Extracted {keyPhrases.Count} key phrases:");

                _logger.LogInformation($"  {phrases}");

                return phrases;
                
            }
            catch (RequestFailedException exception)
            {
                Console.WriteLine($"Error Code: {exception.ErrorCode}");
                Console.WriteLine($"Message: {exception.Message}");
                return exception.Message;
            }

        }
        /* private async Task<string> AnalyzeTextSentiment(Stream stream)
        {
            string textToAnalyze = "";
            textToAnalyze = ReadTextFromStream(stream);

            Uri endpoint = new("https://dlf-ai-language.cognitiveservices.azure.com/");
            AzureKeyCredential credential = new("0550f25df31d46569a2a007de171d471");
            TextAnalyticsClient client = new(endpoint, credential);

            try
            {
                Response<DocumentSentiment> response = client.AnalyzeSentiment(textToAnalyze);
                DocumentSentiment docSentiment = response.Value;
                var sentences = docSentiment.Sentences.Select(d => d.Text).ToList();

                var sentiment = $"The document '{docSentiment.Sentiment.ToString()}' {string.Join(',', sentences)}";


                return sentiment;

            }
            catch (RequestFailedException exception)
            {
                Console.WriteLine($"Error Code: {exception.ErrorCode}");
                Console.WriteLine($"Message: {exception.Message}");
            }

            return "";

        } */
        private async Task<string> SummarizeText(Stream stream)
        {
            string textToAnalyze = "";
            textToAnalyze = ReadTextFromStream(stream);

            Uri endpoint = new("https://dlf-ai-language.cognitiveservices.azure.com/");
            AzureKeyCredential credential = new("0550f25df31d46569a2a007de171d471");
            TextAnalyticsClient client = new(endpoint, credential);

            List<string> batchedDocuments = new()
            {
                textToAnalyze
            };

            try
            {
                StringBuilder builder = new StringBuilder();
                AbstractiveSummarizeOperation operation = client.AbstractiveSummarize(WaitUntil.Completed, batchedDocuments);

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
            }

            return "";

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
