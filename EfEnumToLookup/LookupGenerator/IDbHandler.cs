namespace EfEnumToLookup.LookupGenerator
{
	using System;
	using System.Collections.Generic;
	using System.Data.SqlClient;

	internal interface IDbHandler
	{
		/// <summary>
		/// The size of the Name field that will be added to the generated lookup tables.
		/// Adjust to suit your data if required.
		/// </summary>
		int NameFieldLength { get; set; }

		/// <summary>
		/// Prefix to add to all the generated tables to separate help group them together
		/// and make them stand out as different from other tables.
		/// </summary>
		string TableNamePrefix { get; set; }

		/// <summary>
		/// Suffix to add to all the generated tables to separate help group them together
		/// and make them stand out as different from other tables.
		/// </summary>
		string TableNameSuffix { get; set; }

		void Apply(List<LookupData> lookups, IList<EnumReference> enumReferences, Action<string, IEnumerable<SqlParameter>> runSql);
	}
}
