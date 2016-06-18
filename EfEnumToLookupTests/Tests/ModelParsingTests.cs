using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EfEnumToLookup.LookupGenerator;
using EfEnumToLookupTests.Db;
using EfEnumToLookupTests.Model;
using NUnit.Framework;

namespace EfEnumToLookupTests.Tests
{
	[TestFixture]
	public class ModelParsingTests
	{
		private readonly EnumToLookup _enumToLookup = new EnumToLookup();

		[SetUp]
		public void SetUp()
		{
			Database.SetInitializer(new TestInitializer(new EnumToLookup()));
		}


		[Test]
		public void FindsReferences()
		{
			IList<EnumReference> references;
			using (var context = new MagicContext())
			{
				references = EnumToLookup.FindEnumReferences(context);
			}
			var legs = references.SingleOrDefault(r => r.ReferencingField == "SpeedyLegs");
			Assert.IsNotNull(legs, "SpeedyLegs ref not found");
			var ears = references.SingleOrDefault(r => r.ReferencingField == "TehEars");
			Assert.IsNotNull(ears, "TehEars ref not found");
			var echos = references.SingleOrDefault(r => r.ReferencingField == "EchoType");
			Assert.IsNotNull(echos, "EchoType ref not found");
			var eons = references.Count(r => r.EnumType == typeof(Eon));
			Assert.AreEqual(2, eons, "Wrong number of Eon refs found");
			Assert.IsTrue(references.All(r => r.EnumType.IsEnum), "Non-enum type found");
			Assert.AreEqual(13, references.Count);
		}

		[Test]
		public void FindsEnumOnType()
		{
			var enums = EnumToLookup.FindEnums(typeof (Rabbit));
			var prop = enums.SingleOrDefault(p => p.Name == "TehEars");
			Assert.IsNotNull(prop, "Enum property not found");
			Assert.AreEqual(typeof (Ears), prop.PropertyType);
		}

		[Test]
		public void FindsNullableEnumOnType()
		{
			var enums = EnumToLookup.FindEnums(typeof (Rabbit));
			var prop = enums.SingleOrDefault(p => p.Name == "SpeedyLegs");
			Assert.IsNotNull(prop, "Enum property not found");
			Assert.AreEqual(typeof (Legs?), prop.PropertyType);
		}
	}
}
