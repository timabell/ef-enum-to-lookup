﻿namespace EfEnumToLookup.LookupGenerator
{
	using System;
	using System.Collections.Generic;
	using System.Data.SqlClient;
	using System.Text;

	class SqlServerHandler : IDbHandler
	{
		/// <summary>
		/// The size of the Name field that will be added to the generated lookup tables.
		/// Adjust to suit your data if required.
		/// </summary>
		public int NameFieldLength { get; set; }

		/// <summary>
		/// Prefix to add to all the generated tables to separate help group them together
		/// and make them stand out as different from other tables.
		/// </summary>
		public string TableNamePrefix { get; set; }

		/// <summary>
		/// Suffix to add to all the generated tables to separate help group them together
		/// and make them stand out as different from other tables.
		/// </summary>
		public string TableNameSuffix { get; set; }


		/// <summary>
		/// Whether to run the changes inside a database transaction.
		/// </summary>
		public bool UseTransaction { get; set; }


		/// <summary>
		/// Make the required changes to the database.
		/// </summary>
		/// <param name="model">Details of lookups and foreign keys to add/update</param>
		/// <param name="runSql">A callback providing a means to execute sql against the
		/// server. (Or possibly write it to a file for later use.</param>
		public void Apply(LookupDbModel model, Action<string, IEnumerable<SqlParameter>> runSql)
		{
			var sql = BuildSql(model);
			runSql(sql, null);
		}

		/// <summary>
		/// Generates the migration SQL needed to update the database to match
		/// the enums in the current model.
		/// </summary>
		/// <param name="model">Details of lookups and foreign keys to add/update</param>
		/// <returns>The generated SQL script</returns>
		public string GenerateMigrationSql(LookupDbModel model)
		{
			return BuildSql(model);
		}

		private string BuildSql(LookupDbModel model)
		{
			var sql = new StringBuilder();
			sql.AppendLine("set nocount on;");
			if (UseTransaction)
			{
				sql.AppendLine("set xact_abort on; -- rollback on error");
				sql.AppendLine("begin tran;");
			}
			sql.AppendLine(CreateTables(model.Lookups));
			sql.AppendLine(PopulateLookups(model.Lookups));
			sql.AppendLine(AddForeignKeys(model.References));
			if (UseTransaction)
			{
				sql.AppendLine("commit;");
			}
			return sql.ToString();
		}

		private string CreateTables(IEnumerable<LookupData> enums)
		{
			var sql = new StringBuilder();
			foreach (var lookup in enums)
			{
				sql.AppendFormat(
					@"IF OBJECT_ID('{0}', 'U') IS NULL
begin
	CREATE TABLE [{0}] (Id {2} CONSTRAINT PK_{0} PRIMARY KEY, Name nvarchar({1}));
	exec sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE',
		@level1name=N'{0}', @value=N'Automatically generated. Contents will be overwritten on app startup. Table & contents generated by https://github.com/timabell/ef-enum-to-lookup';
end
",
					TableName(lookup.Name), NameFieldLength, NumericSqlType(lookup.NumericType));
			}
			return sql.ToString();
		}

		private string AddForeignKeys(IEnumerable<EnumReference> refs)
		{
			var sql = new StringBuilder();
			foreach (var enumReference in refs)
			{
				var fkName = string.Format("FK_{0}_{1}", enumReference.ReferencingTable, enumReference.ReferencingField);

				sql.AppendFormat(
					" IF OBJECT_ID('{0}', 'F') IS NULL AND OBJECT_ID('{1}', 'U') IS NOT NULL ALTER TABLE [{1}] ADD CONSTRAINT {0} FOREIGN KEY ([{2}]) REFERENCES [{3}] (Id);\r\n",
					fkName, enumReference.ReferencingTable, enumReference.ReferencingField, TableName(enumReference.EnumType.Name)
				);
			}
			return sql.ToString();
		}

		private string PopulateLookups(IEnumerable<LookupData> lookupData)
		{
			var sql = new StringBuilder();
			sql.AppendLine(string.Format("CREATE TABLE #lookups (Id int, Name nvarchar({0}) COLLATE database_default);", NameFieldLength));
			foreach (var lookup in lookupData)
			{
				sql.AppendLine(PopulateLookup(lookup));
			}
			sql.AppendLine("DROP TABLE #lookups;");
			return sql.ToString();
		}

		private string PopulateLookup(LookupData lookup)
		{
			var sql = new StringBuilder();
			foreach (var value in lookup.Values)
			{
				sql.AppendFormat("INSERT INTO #lookups (Id, Name) VALUES ({0}, N'{1}');\r\n", value.Id, SanitizeSqlString(value.Name));
			}

			sql.AppendLine(string.Format(@"
MERGE INTO [{0}] dst
	USING #lookups src ON src.Id = dst.Id
	WHEN MATCHED AND src.Name <> dst.Name THEN
		UPDATE SET Name = src.Name
	WHEN NOT MATCHED THEN
		INSERT (Id, Name)
		VALUES (src.Id, src.Name)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE
;"
				, TableName(lookup.Name)));

			sql.AppendLine("TRUNCATE TABLE #lookups;");
			return sql.ToString();
		}

		private static string SanitizeSqlString(string value)
		{
			return value.Replace("'", "''");
		}

		private string TableName(string enumName)
		{
			return string.Format("{0}{1}{2}", TableNamePrefix, enumName, TableNameSuffix);
		}

		private static string NumericSqlType(Type numericType)
		{
			if (numericType == typeof(byte))
			{
				return "tinyint";
			}
			return "int";
		}
	}
}
