using System.Data.Entity;
using EFTests.Model;

namespace EFTests.Db
{
	public class MagicContext : DbContext
	{
		public DbSet<Rabbit> PeskyWabbits { get; set; }

		public DbSet<Chicken> LittleChickens { get; set; }

		public DbSet<Fox> CunningFoxes { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Fox>().Map(f => f.ToTable("Foxies"));
			base.OnModelCreating(modelBuilder);
		}
	}
}
