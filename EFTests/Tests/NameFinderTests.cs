using System.Data.Entity;
using EfEnumToLookup.LookupGenerator;
using EFTests.Db;
using EFTests.Model;
using NUnit.Framework;

namespace EFTests.Tests
{
	[TestFixture]
	public class NameFinderTests
	{
		[SetUp]
		public void SetUp()
		{
			Database.SetInitializer(new TestInitializer());
		}

		[Test]
		public void MapsPluralisedName()
		{
			string actual;
			using (var context = new MagicContext())
			{
				actual = TableNameFinder.GetTableName(typeof (Rabbit), context);
			}
			// default pluralized type name DbSet<Rabbit> -> Rabbits
			Assert.AreEqual("Rabbits", actual);
		}

		[Test]
		public void MapsAttributeName()
		{
			string actual;
			using (var context = new MagicContext())
			{
				actual = TableNameFinder.GetTableName(typeof (Chicken), context);
			}
			// name set in [Table] attribute
			Assert.AreEqual("FunkyChickens", actual);
		}

		[Test]
		public void MapsFluentConfigurationName()
		{
			string actual;
			using (var context = new MagicContext())
			{
				actual = TableNameFinder.GetTableName(typeof (Fox), context);
			}
			// name set in [Table] attribute
			Assert.AreEqual("Foxies", actual);
		}
	}
}
