using System.Net.WebSockets;

namespace ChatWebSocketPoC.Services
{
    public interface IWebSocketHandler
    {
        Task HandleWebSocketConnection(HttpContext context, WebSocket webSocket, string channelName, string username);
        Task ReceiveMessagesAsync(HttpContext context, WebSocket webSocket, string channelName, string username);
        Task BroadcastMessageToChannel(string channelName, string message);



    }
}
