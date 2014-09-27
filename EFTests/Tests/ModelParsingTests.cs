using EfEnumToLookup.LookupGenerator;
using EFTests.Db;
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
            var refs = enumToLookup.FindReferences(contextType);
            Assert.AreEqual(0, refs.Count);
        }
    }
}
