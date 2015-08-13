namespace ExampleUsage
{
	using System;
	using System.Data.Entity;
	using EfEnumToLookup.LookupGenerator;
	using NUnit.Framework;

	/// <summary>
	/// Example usage of the ef lookup generator.
	/// To see the library in action create a project for this example cs
	/// add this file to it, add EF 6.1 and NUnit through nuget and run the 
	/// below test method with NUnit.
	/// 
	/// To see the generated schema:
	/// Open up (localdb)\v11.0 in Sql Server Object Explorer
	/// and look inside the new database it has generated.
	/// Take a look at the Enum_Size table and its contents.
	/// </summary>
	[TestFixture]
	public class EnumExample
	{
		[SetUp]
		public void Setup()
		{
			Database.SetInitializer(new DropCreateDatabaseAlways<MyDbContext>());
			Database.SetInitializer(new DropCreateDatabaseAlways<MyCustomSchemaDbContext>());
		}

		[Test]
		public void ExampleOfUsingApply()
		{
			using (var context = new MyDbContext())
			{
				var enumToLookup = new EnumToLookup();
				enumToLookup.NameFieldLength = 42; // optional, example of how to override default values

				// This would normally be run inside either a db initializer Seed()
				// or the migration Seed() method which both provide access to a context.
				enumToLookup.Apply(context);
			}
		}

		[Test]
		public void ExampleOfGeneratingSql()
		{
			using (var context = new MyDbContext())
			{
				var enumToLookup = new EnumToLookup();

				// if you need to get at the raw sql to run a migration separately then use:
				var migrationSql = enumToLookup.GenerateMigrationSql(context);
				// you'd probably want to write this to a file and then add it to source control, but for
				// the purpose of demonstration we'll write it to the console instead:
				Console.Out.WriteLine(migrationSql);

				// at some point you'd then run the sql (probably not like this, but this serves as a test that it's working)
				context.Database.ExecuteSqlCommand(migrationSql);
			}
		}

		[Test]
		public void CheckThatDefaultSchemaExists()
		{
			using (var context = new MyDbContext())
			{
				var enumToLookup = new EnumToLookup();
				enumToLookup.Apply(context);

				context.Foos.Add(new Foo { Size = Size.Small, Shape = Shape.Round });

				context.Database.SqlQuery<Foo>("Select Id, Size, Shape From dbo.Enum_Size");
			}
		}

		[Test]
		public void CheckIfCustomSchemaExists()
		{
			using (var context = new MyCustomSchemaDbContext())
			{
				var enumToLookup = new EnumToLookup();
				enumToLookup.Apply(context);

				context.Foos.Add(new Foo { Size = Size.Small, Shape = Shape.Round });

				context.Database.SqlQuery<Foo>("Select Id, Size, Shape From myschema.Enum_Size");
			}
		}
	}

	/// <summary>
	/// Example context
	/// </summary>
	public class MyDbContext : DbContext
	{
		public DbSet<Foo> Foos { get; set; }
	}

	public class MyCustomSchemaDbContext : DbContext
	{
		public DbSet<Foo> Foos { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.HasDefaultSchema("myschema");
			base.OnModelCreating(modelBuilder);
		}
	}

	/// <summary>
	/// Example entity that references an enum
	/// </summary>
	public class Foo
	{
		public int Id { get; set; }
		public Size Size { get; set; }
		public Shape Shape { get; set; }
	}

	/// <summary>
	/// Example enum that will be converted into a lookup table.
	/// </summary>
	public enum Size
	{
		Small = 1, //db friendly id

		// this is only fully qualified because of a name clash with NUnit, you wouldn't normally need to.
		[System.ComponentModel.Description("It's average")] // example of apostrophe that would need escaping in sql
		Medium,

		ReallyVeryBig,

		// this is only fully qualified because of a name clash with NUnit, you wouldn't normally need to.
		[System.ComponentModel.Description("Huge you know?")] // give it a different name in the lookup table
		Huge,

		[RuntimeOnly] // this won't exist in the database, handy to prevent unwanted data creeping in (enforced by foreign key constraint).
		Undecided
	}

	public enum Shape
	{
		Square,
		Round
	}
}
