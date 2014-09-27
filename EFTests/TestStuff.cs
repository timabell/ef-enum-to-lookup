using System.Linq;
using NUnit.Framework;

namespace EFTests
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
                var actual = context.Rabbits.First();
                Assert.AreEqual("Roger", actual.Name);
            }
        }
    }
}
