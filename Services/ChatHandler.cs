using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace ChatWebSocketPoC.Services
{
    public class ChatHandler : IWebSocketHandler
    {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocket>> Channels = new();
        public async Task HandleWebSocketConnection(HttpContext context, WebSocket webSocket, string channelName, string username)
        {
            var channel = Channels.GetOrAdd(channelName, _ => new ConcurrentDictionary<string, WebSocket>());
            if (!channel.TryAdd(username, webSocket))
            {
                await context.Response.WriteAsync("Username already taken in this channel.");
                return;
            }

            var userConnected = $"{username} connected";

            await BroadcastMessageToChannel(channelName, userConnected);
            await ReceiveMessagesAsync(context, webSocket, channelName, username);
        }

        public async Task ReceiveMessagesAsync(HttpContext context, WebSocket webSocket, string channelName, string username)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var fullMessage = $"{username}: {message}";
                await BroadcastMessageToChannel(channelName, fullMessage);

                result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
            }

            var userDisconnected = $"{username} disconnected";
            await BroadcastMessageToChannel(channelName, userDisconnected);

            Channels[channelName].TryRemove(username, out _);
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        public async Task BroadcastMessageToChannel(string channelName, string message)
        {
            if (Channels.TryGetValue(channelName, out var channel))
            {
                var messageBuffer = Encoding.UTF8.GetBytes(message);
                var messageSegment = new ArraySegment<byte>(messageBuffer);

                foreach (var socket in channel.Values)
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
