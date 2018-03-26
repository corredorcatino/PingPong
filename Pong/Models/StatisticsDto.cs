using System.Collections.Generic;

namespace Pong.Models
{
    public class StatisticsDto
    {
        public int PingMessagesCount { get; set; }
        public int PongMessagesCount { get; set; }
        public List<PingMessage> PingMessages { get; set; }
        public List<PongMessage> PongMessages { get; set; }
    }
}