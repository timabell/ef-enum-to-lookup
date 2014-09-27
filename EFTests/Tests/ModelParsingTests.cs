using System;
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
        readonly EnumToLookup _enumToLookup = new EnumToLookup();
        readonly Type _contextType = typeof(MagicContext);

        [Test]
        public void FindsDbSet()
        {
            var dbSets = _enumToLookup.FindDbSets(_contextType);
            Assert.AreEqual(1, dbSets.Count);
            var rabbits = dbSets.First();
            Assert.AreEqual(typeof(DbSet<Rabbit>), rabbits.PropertyType);
            Assert.AreEqual("PeskyWabbits", rabbits.Name);
        }


        [Test]
        public void FindsEnumOnDbSet()
        {
            var enums = _enumToLookup.FindEnums(typeof(Rabbit));
            Assert.AreEqual(1, enums.Count);
            var prop = enums.First();
            Assert.AreEqual(typeof(Ears), prop.PropertyType);
            Assert.AreEqual("TehEars", prop.Name);
        }
    }
}
