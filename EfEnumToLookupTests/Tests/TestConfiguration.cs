using System.Data.Entity;
using EfEnumToLookup.LookupGenerator;
using EfEnumToLookupTests.Db;
using EfEnumToLookupTests.Model;
using NUnit.Framework;

namespace EfEnumToLookupTests.Tests
{
	[TestFixture]
	public class TestConfiguration
	{
		[SetUp]
		public void SetUp()
		{
			// Cleanup after other test runs
			// Using setup rather than teardown to make it easier to inspect the database after running a test.
			using (var context = new MagicContext())
			{
				if (context.Database.Exists())
				{
					context.Database.Delete();
				}
			}
		}

		[Test]
		public void SetsTablePrefix()
		{
			TestPrefixConfig("Teh", "select 1 from TehEars");
		}

		[Test]
		public void SetsTableSuffix()
		{
			TestSuffixConfig("_Lookup", "select 1 from Ears_Lookup");
		}

		[Test]
		public void NullTablePrefix()
		{
			TestPrefixConfig(null, "select 1 from Ears");
		}

		[Test]
		public void SetsVarcharTypeForNameField()
		{
			string colType = TestNameFieldType(NameFieldType.Varchar);
			Assert.AreEqual(colType.ToLower(), "varchar");
		}


		private string TestNameFieldType(NameFieldType columnType)
		{
			// arrange
			var enumToLookup = new EnumToLookup
			{
				TableNamePrefix = null,
				TableNameSuffix = null,
				NameFieldType = NameFieldType.Varchar,
			};
			TestConfig("select 1 from Ears", enumToLookup);
			string dbColumnType = GetNameColumnType("Ears");
			return dbColumnType;
		}

		private string GetNameColumnType(string tableName)
		{
			string sql = $@"
				SELECT DATA_TYPE 
				FROM INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = '{tableName}' AND 
					COLUMN_NAME = 'Name'";

			using (var context = new MagicContext())
			{
				string columnType = context.Database.SqlQuery<string>(sql)
					.FirstOrDefaultAsync().GetAwaiter().GetResult();
				return columnType;
			}
		}

		private static void TestSuffixConfig(string tableNameSuffix, string testSql)
		{
			// arrange
			var enumToLookup = new EnumToLookup
			{
				TableNamePrefix = null,
				TableNameSuffix = tableNameSuffix
			};
			TestConfig(testSql, enumToLookup);
		}
		private static void TestPrefixConfig(string tableNamePrefix, string testSql)
		{
			// arrange
			var enumToLookup = new EnumToLookup
			{
				TableNamePrefix = tableNamePrefix
			};
			TestConfig(testSql, enumToLookup);
		}

		private static void TestConfig(string testSql, EnumToLookup enumToLookup)
		{
			Database.SetInitializer(new TestInitializer(enumToLookup));
			using (var context = new MagicContext())
			{
				var roger = new Rabbit { Name = "Roger", TehEars = Ears.Pointy };
				context.PeskyWabbits.Add(roger);
				context.SaveChanges();

				// assert
				context.Database.ExecuteSqlCommand(testSql); // should explode if anything is wrong
			}
		}
	}
}
