using System.Data.Entity;
using EfEnumToLookup.LookupGenerator;
using NUnit.Framework;

namespace EfLookupTesting
{
	/// <summary>
	/// Example usage of the ef lookup generator.
	/// To run this create a project, drop this file in,
	/// add EF 6.1 and NUnit through nuget and run the test.
	/// Open up (localdb)\v11.0 in Sql Server Object Explorer
	/// and look inside the new it has generated to see an
	/// example of the generated Enum table and its content.
	/// </summary>
	[TestFixture]
	public class EnumExample
	{
		[Test]
		public void DoStuff()
		{
			// This would normally be run inside either a db initializer seed
			// or the migration seed method.
			var enumToLookup = new EnumToLookup();
			using (var context = new MyDbContext())
			{
				enumToLookup.Apply(context);
			}
		}
	}

	public class MyDbContext : DbContext
	{
		public DbSet<Foo> Foos { get; set; }
	}

	public class Foo
	{
		public int Id { get; set; }
		public Size Size { get; set; }
	}

	public enum Size
	{
		Small = 1, //db friendly id
		Medium,
		ReallyVeryBig
	}
}
