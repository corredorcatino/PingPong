using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Ping.Controllers
{
    [Route("api/EmitPingMessage")]
    public class PingMessageController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            Console.WriteLine("PingMessageController");
            var pingMessageClient = new PingMessageClient();

            var response = await pingMessageClient.SendPingMessage();
            pingMessageClient.Close();

            return new ObjectResult(response);
        }
    }
}