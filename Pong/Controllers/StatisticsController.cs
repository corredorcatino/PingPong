using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pong.Models;
using System.Collections.Generic;
using System.Linq;

namespace Pong.Controllers
{
    [Route("api/Statistics")]
    public class StatisticsController : Controller
    {
        private readonly PingPongContext _context;
        public StatisticsController(PingPongContext context)
        {
            _context = context;
        }

        [HttpGet]
        public StatisticsDto Get()
        {
            var pingMessages = _context.PingMessages.ToList();
            var pongMessages = _context.PongMessages.ToList();

            var statistics = new StatisticsDto
            {
                PingMessagesCount = pingMessages.Count,
                PongMessagesCount = pongMessages.Count,
                PingMessages = pingMessages,
                PongMessages = pongMessages
            };

            return statistics;
        }
    }
}