using Azure.Messaging.ServiceBus;
using Bluecorp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Bluecorp.DispatchOrder
{
    public class DispatchOrder
    {
        private const string ContentType = "application/json";

        private readonly ILogger<DispatchOrder> _logger;

        public DispatchOrder(ILogger<DispatchOrder> log)
        {
            _logger = log;
        }

        [FunctionName("DispatchOrder")]
        [OpenApiOperation(operationId: "DispatchOrder")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType, bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [ServiceBus("readytodispatch", EntityType = Microsoft.Azure.WebJobs.ServiceBus.ServiceBusEntityType.Queue)] ServiceBusSender sender)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("No request body found");
            }

            try
            {
                //TODO: Validate JSON payload against the schema to check for required fields and allowed enum values.

                DispatchEvent data = JsonConvert.DeserializeObject<DispatchEvent>(requestBody);

                var message = new ServiceBusMessage(requestBody)
                {
                    ContentType = ContentType,
                    //De-duplicate based on MessageId
                    MessageId = data.controlNumber.ToString()
                };

                await sender.SendMessageAsync(message);

                var responseMessage = $"Received dispatch event with control number {message.MessageId}";

                _logger.LogInformation($"{responseMessage}");

                return new OkObjectResult(responseMessage);
            }
            catch (JsonSerializationException ex)
            {
                var errorMessage = "Invalid payload";

                _logger.LogError(ex, errorMessage);

                return new BadRequestObjectResult(errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch event");

                return new InternalServerErrorResult();
            }
        }

        /* Sample code to upload JSON file to blob storage with controlNumber as filename
        [FunctionName("DispatchOrderBlob")]
        [OpenApiOperation(operationId: "DispatchOrderBlob")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [Blob("dispatchedorders", Connection = "AzureDispatchOrderStorage")] BlobContainerClient outputContainer)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("No request body found");
            }

            dynamic data = JObject.Parse(requestBody);

            string blobName = (string)data.controlNumber;

            var cloudBlockBlob = outputContainer.GetBlobClient(blobName);

            // upload will fail if file with same name already exists in blob storage
            await cloudBlockBlob.UploadAsync(new BinaryData(requestBody));

            return new OkObjectResult(blobName);
        }
        */
    }
}

