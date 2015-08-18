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

		/// <summary>
		/// Whether to run the changes inside a database transaction.
		/// </summary>
		bool UseTransaction { get; set; }

		/// <summary>
		/// Make the required changes to the database.
		/// </summary>
		/// <param name="model">Details of lookups and foreign keys to add/update</param>
		/// <param name="runSql">A callback providing a means to execute sql against the
		/// server. (Or possibly write it to a file for later use.</param>
		void Apply(LookupDbModel model, Action<string, IEnumerable<SqlParameter>> runSql);

		/// <summary>
		/// Generates the migration SQL needed to update the database to match
		/// the enums in the current model.
		/// </summary>
		/// <param name="model">Details of lookups and foreign keys to add/update</param>
		/// <returns>The generated SQL script</returns>
		string GenerateMigrationSql(LookupDbModel model);
	}
}
