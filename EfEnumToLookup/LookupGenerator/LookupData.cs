namespace EfEnumToLookup.LookupGenerator
{
	using System.Collections.Generic;

	internal class LookupData
	{
		public string Name { get; set; }
		public IEnumerable<LookupValue> Values { get; set; }
	}
}
