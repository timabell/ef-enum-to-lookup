using System.Collections.Generic;

namespace EfEnumToLookup.LookupGenerator
{
	/// <summary>
	/// Not the best name ever. Everything you need to know
	/// about a database to be able to generate lookup tables
	/// and add foreign keys pointing to them.
	/// </summary>
	internal class LookupDbModel
	{
		public IList<LookupData> Lookups { get; set; }
		public IList<EnumReference> References { get; set; }
	}
}
