using System.Data.Entity;
using EfEnumToLookup.LookupGenerator;
using NUnit.Framework;

namespace EfLookupTesting
{
	[TestFixture]
	public class EnumExample
	{
		[Test]
		public void DoStuff()
		{
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
