using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using EfEnumToLookup.LookupGenerator;
using EfEnumToLookupTests.Db;
using EfEnumToLookupTests.Model;
using NUnit.Framework;

namespace EfEnumToLookupTests.Tests
{
	[TestFixture]
	public class TestStuff
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

			Database.SetInitializer(new TestInitializer(new EnumToLookup()));
			using (var context = new MagicContext())
			{
				var roger = new Rabbit { Name = "Roger", TehEars = Ears.Pointy };
				context.PeskyWabbits.Add(roger);
				context.SaveChanges();
			}
		}

		[Test]
		public void DoesStuff()
		{
			using (var context = new MagicContext())
			{
				var actual = context.PeskyWabbits.First();
				Assert.AreEqual("Roger", actual.Name);
				Assert.AreEqual(Ears.Pointy, actual.TehEars);
				Assert.AreEqual(1, context.PeskyWabbits.Count()); // spot unwanted re-use of db
			}
		}

		[Test]
		public void IgnoresRuntimeValues()
		{
			using (var context = new MagicContext())
			{
				const int prototypeId = (int)Ears.Prototype;
				const string sql = "select @count = count(*) from Enum_Ears where id = @id";
				var idParam = new SqlParameter("id", prototypeId);
				var outParam = new SqlParameter("count", SqlDbType.Int) { Direction = ParameterDirection.Output };
				context.Database.ExecuteSqlCommand(sql, idParam, outParam);
				var matches = outParam.Value;
				Assert.AreEqual(0, matches, string.Format("Runtime only value '{1}' shouldn't be in db. Enum_Ears id {0}", prototypeId, Ears.Prototype));
			}
		}

		[Test]
		public void UsesDescriptionAttribute()
		{
			using (var context = new MagicContext())
			{
				const string sql = "select @description = name from Enum_Importance where id = @id";
				var idParam = new SqlParameter("id", (int)Importance.NotBovverd);
				var outParam = new SqlParameter("description", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
				context.Database.ExecuteSqlCommand(sql, idParam, outParam);
				var actualName = outParam.Value;
				Assert.AreEqual(Constants.BovveredDisplay, actualName);
			}
		}
	}
}
