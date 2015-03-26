using EfEnumToLookup.LookupGenerator;

namespace EfEnumToLookupTests.Model
{
	public enum Ears
	{
		// Normal database convention for lookups is to start at 1, not zero (the enum default).
		// This causes fail-early behaviour on un-initialized data (default of int is 0) which is a "good thing" (TM).
		Floppy = 1,

		Pointy,

		[RuntimeOnly]
		Prototype
	}
}
