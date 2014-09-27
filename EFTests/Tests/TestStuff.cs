using System.Linq;
using EFTests.Database;
using EFTests.Model;
using NUnit.Framework;

namespace EFTests.Tests
{
    [TestFixture]
    public class TestStuff
    {
        [Test]
        public void DoesStuff()
        {
            using (var context = new MagicContext())
            {
                var roger = new Rabbit { Name = "Roger" };
                context.Rabbits.Add(roger);
                context.SaveChanges();
                var actual = context.Rabbits.First();
                Assert.AreEqual("Roger", actual.Name);
            }
        }
    }
}
