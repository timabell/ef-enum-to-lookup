using System.Collections.Generic;
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
        [SetUp]
        public void SetUp()
        {
            Database.SetInitializer(new TestInitializer());
        }


        [Test]
        public void FindsReferences()
        {
            IList<EnumReference> references;
            using (var context = new MagicContext())
            {
                references = _enumToLookup.FindReferences(context);
            }
            Assert.AreEqual(2, references.Count);
            var legs = references.SingleOrDefault(r => r.ReferencingField == "SpeedyLegs");
            Assert.IsNotNull(legs, "SpeedyLegs ref not found");
            var ears = references.SingleOrDefault(r => r.ReferencingField == "TehEars");
            Assert.IsNotNull(ears, "TehEars ref not found");
            Assert.IsTrue(references.All(r => r.EnumType.IsEnum), "Non-enum type found");
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
