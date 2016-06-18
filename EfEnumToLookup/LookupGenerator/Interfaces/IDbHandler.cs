using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using EfEnumToLookup.LookupGenerator.Models;

namespace EfEnumToLookup.LookupGenerator.Interfaces
{
	internal interface IDbHandler
	{
		/// <summary>
		///     Holds the configuration data
		/// </summary>
		EnumToLookupConfiguration Configuration { get; set; }

		/// <summary>
		///     Make the required changes to the database.
		/// </summary>
		/// <param name="model">Details of lookups and foreign keys to add/update</param>
		/// <param name="runSql">
		///     A callback providing a means to execute sql against the
		///     server. (Or possibly write it to a file for later use.
		/// </param>
		void Apply(LookupDbModel model, Action<string, IEnumerable<SqlParameter>> runSql);

		/// <summary>
		///     Generates the migration SQL needed to update the database to match
		///     the enums in the current model.
		/// </summary>
		/// <param name="model">Details of lookups and foreign keys to add/update</param>
		/// <returns>The generated SQL script</returns>
		string GenerateMigrationSql(LookupDbModel model);
	}
}