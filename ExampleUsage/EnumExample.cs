using System.Data.Entity;
using EfEnumToLookup.LookupGenerator;
using NUnit.Framework;

namespace ExampleUsage
{
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
		[Test]
		public void DoStuff()
		{
			using (var context = new MyDbContext())
			{
				// This would normally be run inside either a db initializer Seed()
				// or the migration Seed() method which both provide access to a context.
				var enumToLookup = new EnumToLookup();
				enumToLookup.NameFieldLength = 42; // optional, example of how to override default values
				enumToLookup.Apply(context);
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

	/// <summary>
	/// Example entity that references an enum
	/// </summary>
	public class Foo
	{
		public int Id { get; set; }
		public Size Size { get; set; }
	}

	/// <summary>
	/// Example enum that will be converted into a lookup table.
	/// </summary>
	public enum Size
	{
		Small = 1, //db friendly id
		Medium,
		ReallyVeryBig
	}
}
