using System.Text;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

namespace ObjectDetection.src.EventHub;

public class EventHubClient
{
    private readonly static string connectionString = Environment.GetEnvironmentVariable("EVENTHUB_CONNECTION_STRING") ?? string.Empty;
    public async static void SendMessage(string message)
    {
        await using (var producerClient = new EventHubProducerClient(connectionString, "depression-event"))
        {
            using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();

            eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(message)));

            await producerClient.SendAsync(eventBatch);
            Console.WriteLine("Message sended");
        }
    }
}
