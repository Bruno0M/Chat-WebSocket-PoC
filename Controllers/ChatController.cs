using ChatWebSocketPoC.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatWebSocketPoC.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IWebSocketHandler _socketHandler;

        public ChatController(IWebSocketHandler socketHandler)
        {
            _socketHandler = socketHandler;
        }

        [HttpGet("ws")]
        public async Task<IActionResult> ConnectChannel(string channel, string username)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                if (string.IsNullOrEmpty(channel))
                {
                    HttpContext.Response.StatusCode = 400;
                    await HttpContext.Response.WriteAsync("Channel name is required");
                    return BadRequest();
                }

                var webSokcet = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _socketHandler.HandleWebSocketConnection(HttpContext, webSokcet, channel, username);
            }

            return Ok();
        }
    }
}
