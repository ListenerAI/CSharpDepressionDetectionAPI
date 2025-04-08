using System.Text;
using System.Net.WebSockets;

namespace ObjectDetection.src.Websocket;

public class WebsocketSender : WebSocketHandler
{
    protected override async Task BroadcastMessageAsync(string message, WebSocket sender)
    {
        Console.WriteLine($"Endpoint /ws received: {message}");
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            await sender.SendAsync(new ArraySegment<byte>(messageBytes, 0, messageBytes.Length),
                                           WebSocketMessageType.Text,
                                           true,
                                           CancellationToken.None);
            await base.BroadcastMessageAsync(message, sender);
    }
}