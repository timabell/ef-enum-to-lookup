namespace EfEnumToLookupTests.Tests
{
	using System.Collections.Generic;
	using System.Data.Entity;
	using System.Linq;
	using Db;
	using EfEnumToLookup.LookupGenerator;
	using Model;
	using NUnit.Framework;

	[TestFixture]
	public class ModelCustomSchemaParsingTests
	{
		private readonly EnumToLookup _enumToLookup = new EnumToLookup();

		[SetUp]
		public void SetUp()
		{
			Database.SetInitializer(new TestInitializer(new EnumToLookup()));
		}
		
		[Test]
		public void FindsReferencesCustomSchema()
		{
			IList<EnumReference> references;
			using (var context = new MagicCustomSchemaContext())
			{
				references = _enumToLookup.FindEnumReferences(context);
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
		public void FindsEnumOnTypeCustomSchema()
		{
			var enums = _enumToLookup.FindEnums(typeof (Rabbit));
			var prop = enums.SingleOrDefault(p => p.Name == "TehEars");
			Assert.IsNotNull(prop, "Enum property not found");
			Assert.AreEqual(typeof (Ears), prop.PropertyType);
		}

		[Test]
		public void FindsNullableEnumOnTypeCustomSchema()
		{
			var enums = _enumToLookup.FindEnums(typeof (Rabbit));
			var prop = enums.SingleOrDefault(p => p.Name == "SpeedyLegs");
			Assert.IsNotNull(prop, "Enum property not found");
			Assert.AreEqual(typeof (Legs?), prop.PropertyType);
		}
	}
}
