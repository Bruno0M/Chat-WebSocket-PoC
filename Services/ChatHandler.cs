using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace ChatWebSocketPoC.Services
{
    public class ChatHandler : IWebSocketHandler
    {
        private static readonly ConcurrentDictionary<string, ConcurrentBag<WebSocket>> Channels = new();
        public async Task HandleWebSocketConnection(HttpContext context, WebSocket webSocket, string channelName)
        {
            var channel = Channels.GetOrAdd(channelName, _ => new ConcurrentBag<WebSocket>());
            channel.Add(webSocket);

            await ReceiveMessagesAsync(context, webSocket, channelName);
        }

        public async Task ReceiveMessagesAsync(HttpContext context, WebSocket webSocket, string channelName)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                await BroadcastMessageToChannel(channelName, message);

                result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
            }
            Channels[channelName].TryTake(out _);
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        public async Task BroadcastMessageToChannel(string channelName, string message)
        {
            if (Channels.TryGetValue(channelName, out var channel))
            {
                var messageBuffer = Encoding.UTF8.GetBytes(message);
                var messageSegment = new ArraySegment<byte>(messageBuffer);

                foreach (var socket in channel)
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        await socket.SendAsync(messageSegment, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
        }
    }
}
