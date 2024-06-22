using System.Net.WebSockets;

namespace ChatWebSocketPoC.Services
{
    public interface IWebSocketHandler
    {
        Task HandleWebSocketConnection(HttpContext context, WebSocket webSocket, string channelName);
        Task ReceiveMessagesAsync(HttpContext context, WebSocket webSocket, string channelName);
        Task BroadcastMessageToChannel(string channelName, string message);



    }
}
