using System.Data.Entity;
using EfEnumToLookup.LookupGenerator;

namespace EFTests.Db
{
    public class TestInitializer : DropCreateDatabaseAlways<MagicContext>
    {
        protected override void Seed(MagicContext context)
        {
            IEnumToLookup enumToLookup = new EnumToLookup();
            enumToLookup.Apply(context);
            base.Seed(context);
        }
    }
}
