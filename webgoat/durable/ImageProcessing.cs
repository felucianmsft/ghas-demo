using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace DurableDotnet6
{
   public class ImageProcessing
   {

       private static string _endpoint = "https://demofluciano.cognitiveservices.azure.com/";
       private static string _key = "";


       [FunctionName(nameof(ImageProcessingOrchestrator))]
       public static async Task ImageProcessingOrchestrator(
           [OrchestrationTrigger] IDurableOrchestrationContext context)
       {
           string inputFileName = context.GetInput<string>();

           var inconsistencyAnalysisResult = await context.CallActivityAsync<string>(nameof(InconsistencyAnalysis), inputFileName));
           if (!inconsistencyAnalysisResult.Passed)
           {
               context.SetCustomStatus(FailureReason.InconsistencyAnalysis);
               return;
           }
           1

           var contentExtractionResult = await context.CallActivityAsync<ContentExtractionResult>(nameof(ContentExtration), inputFileName));

           var transactionLookupResult = await context.CallActivityAsync<TransactionLookupResult>(nameof(TransactionLookup), contentExtractionResult.Transaction));
           if (!transactionLookupResult.IsFound)
           {
               context.SetCustomStatus(FailureReason.TransactionLookup);
               return;
           }

           var signatureAnalysisResult = await context.CallActivityAsync<SignatureAnalysisResult>(nameof(SignatureAnalysis), inputFileName, contentExtractionResult.Transaction));
           if (!transactionLookupResult.IsValid)
           {
               context.SetCustomStatus(FailureReason.SignatureAnalysis);
               return;
           }

           context.SetCustomStatus(SuccessReason.Success);
       }

       [FunctionName(nameof(InconsistencyAnalysis))]
       public static async Task<string> InconsistencyAnalysis(
           [ActivityTrigger] string fileInput,
           [Blob("transaction-images/{data.FileName}", FileAccess.Read)] string myBlob, ILogger log)
       {
           DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(_endpoint), new AzureKeyCredential(_key));

           Uri fileUri = new Uri("https://raw.githubusercontent.com/Azure-Samples/cognitive-services-REST-api-samples/master/curl/form-recognizer/sample-layout.pdf");
           AnalyzeDocumentOperation operation = await client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-document", fileUri);
           AnalyzeResult result = operation.Value;

           foreach (DocumentKeyValuePair kvp in result.KeyValuePairs)
           {
               Console.WriteLine($"  Found key-value pair: '{kvp.Key.Content}' and '{kvp.Value.Content}'");
           }

           return null;
       }

       [FunctionName(nameof(ContentExtration))]
       public static async Task<ContentExtractionResult> ContentExtration(
           [ActivityTrigger] OrchestratorInput orchestratorInput,
           [Blob("transaction-images/{data.FileName}", FileAccess.Read)] stream fileContent)
       {
           DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(_endpoint), new AzureKeyCredential(_key));
           ContentExtractionResult result = new ContentExtractionResult { FileName = orchestratorInput.FileName };

           AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", fileContent);
           AnalyzeResult operationResult = operation.Value;

           foreach (DocumentKeyValuePair kvp in operationResult.KeyValuePairs)
           {
               result.Content.Add(kvp.Key.Content, kvp.Value.Content);
           }

           return result;
       }

       [FunctionName(nameof(TransactionLookup))]
       public static async Task<TransactionLookupResult> TransactionLookup([ActivityTrigger] OrchestratorInput orchestratorInput)
       {
           return null;
       }

       [FunctionName(nameof(SignatureAnalysis))]
       public static async Task<TransactionLookupResult> SignatureAnalysis([ActivityTrigger] OrchestratorInput orchestratorInput)
       {
           return null;
       }

       [FunctionName("ImageProcessingTrigger")]
       public async Task ImageProcessingTrigger([BlobTrigger("transaction-images/{name}")] Stream fileContent,
           [DurableClient] IDurableOrchestrationClient starter, string fileName, ILogger log)
       {
           log.LogInformation($"A new transaction file is available. Name:{fileName}");
           string instanceId = await starter.StartNewAsync("OrderProcessing", fileName);
           log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);
       }

       [FunctionName(nameof(StartImageProcessing))]
       public static async Task<HttpResponseMessage> StartImageProcessing(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
          [DurableClient] IDurableOrchestrationClient starter,
          ILogger log)
       {
           // Function input comes from the request content.
           OrchestratorInput inputs = new OrchestratorInput { FileName = "DemofileLogicApps.txt" };
           string instanceId = await starter.StartNewAsync(nameof(ImageProcessingOrchestrator), inputs);
           log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);
           return starter.CreateCheckStatusResponse(req, instanceId);
       }

   }

   public class OrchestratorInput
   {
       public string FileName { get; set; }
   }

   public class ContentExtractionResult
   {
       public string FileName { get; set; }
       public Dictionary<string, string> Content { get; set; } = new Dictionary<string, string>();
   }

   public class TransactionLookupResult
   {
       public bool IsFound { get; set; }

       public string TransactionId { get; set; }

       public object Transaction { get; set; }
   }


   public class SignatureAnalysisResult
   {
       public bool IsValid { get; set; }

       public bool IsHandMade { get; set; }

   }

}