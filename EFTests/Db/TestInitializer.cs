using System.Data.Entity;

namespace EFTests.Db
{
    public class TestInitializer : DropCreateDatabaseAlways<MagicContext>
    {
    }
}
