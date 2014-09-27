using System.Data.Entity;
using System.Linq;
using EfEnumToLookup.LookupGenerator;
using EFTests.Db;
using EFTests.Model;
using NUnit.Framework;

namespace EFTests.Tests
{
    [TestFixture]
    public class ModelParsingTests
    {
        [Test]
        public void FindsDbSet()
        {
            var enumToLookup = new EnumToLookup();
            var contextType = typeof(MagicContext);
            var dbSets = enumToLookup.FindDbSets(contextType);
            Assert.AreEqual(1, dbSets.Count);
            var rabbits = dbSets.First();
            Assert.AreEqual(typeof(DbSet<Rabbit>), rabbits.PropertyType);
            Assert.AreEqual("Rabbits", rabbits.Name);
        }
    }
}
