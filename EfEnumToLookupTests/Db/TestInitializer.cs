using System.Data.Entity;
using EfEnumToLookup.LookupGenerator;

namespace EfEnumToLookupTests.Db
{
	public class TestInitializer : DropCreateDatabaseAlways<MagicContext>
	{
		private readonly IEnumToLookup _enumToLookup;

		public TestInitializer(IEnumToLookup enumToLookup)
		{
			_enumToLookup = enumToLookup;
		}

		protected override void Seed(MagicContext context)
		{
			_enumToLookup.Apply(context);
			base.Seed(context);
		}
	}
}
