using Azure.Storage.Blobs;
using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Messaging.EventHubs.Consumer;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ObjectDetection.src.EventHub
{
    public class EventHubListener
    {
        private static EventHubListener instance = new EventHubListener();
        public static readonly string connectionStringEvenHub = Environment.GetEnvironmentVariable("EVENTHUB_TO_LISTEN") ?? string.Empty;

        public static async Task ReadFromEventHub(string eventHubName, string consumerGroup, string connectionString)
        {

            BlobContainerClient storageClient = new BlobContainerClient(
                "ChangeTrackingService.blobInformation.connectionString",
                "event-processor-client-blob"
            );

            var processor = new EventProcessorClient(
                storageClient,
                consumerGroup,
                connectionString,
                eventHubName
            );


            processor.PartitionInitializingAsync += async args =>
            {
                await Task.Run(() =>
                {
                    args.DefaultStartingPosition = EventPosition.Latest;
                });
            };

            processor.ProcessEventAsync += instance.ProcessEventHandler;
            processor.ProcessErrorAsync += instance.ProcessErrorHandler;

            try
            {
                await processor.StartProcessingAsync();

                await Task.Delay(Timeout.Infinite);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error on the event processor: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Stopping Event Processor...");
                await processor.StopProcessingAsync();
            }
        }

        private async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            try
            {
                string eventBody = Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray());

                //ENVIAR POR EL WSS
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing event: {ex.Message}");
            }
            finally
            {
                await eventArgs.UpdateCheckpointAsync();
            }
        }

        private Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            Console.WriteLine($"Error on partition'{eventArgs.PartitionId}': {eventArgs.Exception.Message}");
            return Task.CompletedTask;
        }
    }
}