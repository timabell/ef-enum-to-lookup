using System.Linq;
using EfEnumToLookup.LookupGenerator;
using NUnit.Framework;

namespace EfEnumToLookupTests.Tests
{
	[TestFixture]
	public class EnumParserTests
	{
		[Test]
		public void ReadsName()
		{
			// arrange
			var parser = new EnumParser { SplitWords = false };

			// act
			var result = parser.GetLookupValues(typeof(BareEnum));

			// assert
			Assert.AreEqual("FooBar", result.Single().Name);
		}

		[Test]
		public void ReadsSplitName()
		{
			// arrange
			var parser = new EnumParser { SplitWords = true };

			// act
			var result = parser.GetLookupValues(typeof(BareEnum));

			// assert
			Assert.AreEqual("Foo Bar", result.Single().Name);
		}

		[Test]
		public void ReadsByteEnum()
		{
			// arrange
			var parser = new EnumParser { SplitWords = false };

			// act
			var result = parser.GetLookupValues(typeof(ByteEnum));

			// assert
			Assert.AreEqual("FooBar", result.Single().Name);
		}


		[Test]
		public void ReadsDecoratedName()
		{
			// arrange
			var parser = new EnumParser { SplitWords = true };

			// act
			var result = parser.GetLookupValues(typeof(DecoratedEnum));

			// assert
			Assert.AreEqual("Wide boy", result.Single().Name);
		}

		private enum BareEnum
		{
			// ReSharper disable once UnusedMember.Local
			// used by test suite
			FooBar
		}

		private enum ByteEnum : byte
		{
			// ReSharper disable once UnusedMember.Local
			// used by test suite
			FooBar
		}

		private enum DecoratedEnum
		{
			// ReSharper disable once UnusedMember.Local
			// used by test suite
			[System.ComponentModel.Description("Wide boy")]
			FooBar
		}
	}
}
