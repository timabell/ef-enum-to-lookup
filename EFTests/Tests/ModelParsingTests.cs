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
        public void FindsReferences()
        {
            var refs = _enumToLookup.FindReferences(_contextType);
            Assert.AreEqual(2, refs.Count);
            var legs = refs.SingleOrDefault(r => r.ReferencingField == "SpeedyLegs");
            Assert.IsNotNull(legs, "SpeedyLegs ref not found");
            var ears = refs.SingleOrDefault(r => r.ReferencingField == "TehEars");
            Assert.IsNotNull(ears, "TehEars ref not found");
            Assert.IsTrue(refs.All(r => r.EnumType.IsEnum), "Non-enum type found");
        }

        [Test]
        public void FindsEnumOnDbSet()
        {
            var enums = _enumToLookup.FindEnums(typeof(Rabbit));
            var prop = enums.SingleOrDefault(p => p.Name == "TehEars");
            Assert.IsNotNull(prop, "Enum property not found");
            Assert.AreEqual(typeof(Ears), prop.PropertyType);
        }

        [Test]
        public void FindsNullableEnumOnDbSet()
        {
            var enums = _enumToLookup.FindEnums(typeof(Rabbit));
            var prop = enums.SingleOrDefault(p => p.Name == "SpeedyLegs");
            Assert.IsNotNull(prop, "Enum property not found");
            Assert.AreEqual(typeof(Legs?), prop.PropertyType);
        }
    }
}
