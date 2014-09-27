using System.Data.Entity;
using System.Linq;
using EFTests.Db;
using EFTests.Model;
using NUnit.Framework;

namespace EFTests.Tests
{
    [TestFixture]
    public class TestStuff
    {
        [SetUp]
        public void SetUp()
        {
            Database.SetInitializer(new TestInitializer());
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
            }
        }
    }
}
