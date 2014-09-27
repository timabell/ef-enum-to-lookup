using System.Data.Entity;
using EFTests.Model;

namespace EFTests.Db
{
    public class MagicContext : DbContext
    {
        public DbSet<Rabbit> Rabbits { get; set; }
    }
}