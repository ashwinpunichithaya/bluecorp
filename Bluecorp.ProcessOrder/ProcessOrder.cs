using Azure.Messaging.ServiceBus;
using Bluecorp.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Text;

namespace Bluecorp.ProcessOrder
{
    public static class ProcessOrder
    {
        private static readonly string[] headers =
        {
            "CustomerReference",
            "LoadId",
            "ContainerType",
            "ItemCode",
            "ItemQuantity",
            "ItemWeight",
            "Street",
            "City",
            "State",
            "PostalCode",
            "Country"
        };

        [FunctionName("ProcessOrder")]
        public static void Run([ServiceBusTrigger("readytodispatch", AutoCompleteMessages = false)] ServiceBusReceivedMessage message,
                               Microsoft.Azure.WebJobs.ServiceBus.ServiceBusMessageActions messageActions,
                               ILogger log)
        {
            StringBuilder builder = new();

            builder.AppendJoin(',', headers);

            DispatchEvent data = JsonConvert.DeserializeObject<DispatchEvent>(message.Body.ToString());

            foreach (var (container, item) in from Container container in data.containers
                                              from Item item in container.items
                                              select (container, item))
            {
                var containerType = container.containerType;

                // TODO: Use configurable container type maps
                switch (container.containerType)
                {
                    case "20RF":
                        containerType = "REF20";
                        break;
                    case "40RF":
                        containerType = "REF40";
                        break;
                    case "20HC":
                        containerType = "HC20";
                        break;
                    case "40HC":
                        containerType = "HC40";
                        break;
                }

                builder.AppendJoin(',', new string[]
                {
                    data.salesOrder,
                    container.loadId,
                    containerType,
                    item.itemCode,
                    item.quantity.ToString(),
                    item.cartonWeight.ToString(),
                    data.deliveryAddress.street,
                    data.deliveryAddress.state,
                    data.deliveryAddress.postalCode
                });
            }

            // TODO: Push to SFTP sink
            log.LogTrace(builder.ToString());

            var responseMessage = $"Processed dispatch event with control number {message.MessageId}";

            log.LogInformation(responseMessage);

            // TODO: Abandon message if unsuccessful in pushing to SFTP sink
            messageActions.CompleteMessageAsync(message);
        }
    }
}