namespace EfEnumToLookupTests.Db
{
	using System.Data.Entity;
	using EfEnumToLookupTests.Model;

	public class MagicCustomSchemaContext : DbContext
	{
		public DbSet<Rabbit> PeskyWabbits { get; set; }

		// this one goes deeper
		public DbSet<Warren> Warrens { get; set; }

		public DbSet<Chicken> LittleChickens { get; set; }

		public DbSet<Fox> CunningFoxes { get; set; }

		// Table-per-Hierarchy (TPH)
		public DbSet<Furniture> Furniture { get; set; }

		// Table-per-Type (TPT)
		public DbSet<Vehicle> Vehicles { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.HasDefaultSchema("myschema");
			modelBuilder.Entity<Fox>().Map(f => f.ToTable("Foxies"));
			base.OnModelCreating(modelBuilder);
		}
	}
}
