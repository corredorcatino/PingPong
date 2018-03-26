using Microsoft.EntityFrameworkCore;

namespace Pong.Models
{
    public class PingPongContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("PingPongMessages");
        }

        public DbSet<PingMessage> PingMessages { get; set; }
        public DbSet<PongMessage> PongMessages { get; set; }
    }
}