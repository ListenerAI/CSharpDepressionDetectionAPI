using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
public class WebSocketHandler
{
    protected readonly ConcurrentBag<WebSocket> Connections = new();

    public virtual async Task HandleAsync(WebSocket webSocket, byte[] buffer)
    {
        Connections.Add(webSocket);
        var messageBuilder = new StringBuilder();

        try
        {
            WebSocketReceiveResult result;
            do
            {
                try
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    var messagePart = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    messageBuilder.Append(messagePart);

                    if (result.EndOfMessage)
                    {
                        var message = messageBuilder.ToString();
                        await BroadcastMessageAsync(message, webSocket);
                        messageBuilder.Clear();
                    }
                }
                catch (WebSocketException wsex)
                {
                    Console.WriteLine($"WebSocketException: {wsex.Message}");
                    break; // Exit loop on WebSocket error instead of crashing
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("WebSocket operation was canceled.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    break;
                }

            } while (!result.CloseStatus.HasValue);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical WebSocket Error: {ex.Message}");
        }
        finally
        {
            if (webSocket.State != WebSocketState.Closed && webSocket.State != WebSocketState.Aborted)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }

            Connections.TryTake(out var _);
            Console.WriteLine("WebSocket disconnected.");
        }

    }

    protected virtual async Task BroadcastMessageAsync(string message, WebSocket sender)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        foreach (var socket in Connections)
        {
            if (socket.State == WebSocketState.Open && socket != sender)
            {
                await socket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}