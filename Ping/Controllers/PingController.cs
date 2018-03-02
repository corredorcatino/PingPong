using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Ping.Controllers
{
    [Route("api/EmitPingMessage")]
    public class PingMessageController : Controller
    {
        [HttpPost]
        public IActionResult Post()
        {
            var pingClient = new PingClient();

            var pongMessage = pingClient.Call();
            Console.WriteLine(pongMessage);
            pingClient.Close();

            return new ObjectResult(pongMessage);
        }
    }
}