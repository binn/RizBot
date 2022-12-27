using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace RizBot.Data
{
    public class RizContext : DbContext
    {
        public RizContext(DbContextOptions<RizContext> options) : base(options) { }
        
        public DbSet<Channel> Servers { get; set; }
    }

    public class Channel
    {
        [Key]
        public string Id { get; set; } = default!;
    }
}
