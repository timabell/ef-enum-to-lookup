using System.Data.Entity;

namespace EFTests
{
    public class MagicContext : DbContext
    {
        public DbSet<Rabbit> Rabbits { get; set; }
    }
}