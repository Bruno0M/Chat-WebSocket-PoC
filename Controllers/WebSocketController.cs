using ChatWebSocketPoC.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatWebSocketPoC.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        private readonly IWebSocketHandler _socketHandler;

        public WebSocketController(IWebSocketHandler socketHandler)
        {
            _socketHandler = socketHandler;
        }

        [HttpGet("ws")]
        public async Task<IActionResult> ConnectChannel(string channel)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var channelName = HttpContext.Request.Query["channel"];
                if (string.IsNullOrEmpty(channelName))
                {
                    HttpContext.Response.StatusCode = 400;
                    await HttpContext.Response.WriteAsync("Channel name is required");
                    return BadRequest();
                }

                var webSokcet = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _socketHandler.HandleWebSocketConnection(HttpContext, webSokcet, channelName);
            }

            return Ok();
        }
    }
}
