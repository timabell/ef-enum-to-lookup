using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace EfEnumToLookup.LookupGenerator
{
	/// <summary>
	/// Makes up for a missing feature in Entity Framework 6.1
	/// Creates lookup tables and foreign key constraints based on the enums
	/// used in your model.
	/// Use the properties exposed to control behaviour.
	/// Run <c>Apply</c> from your Seed method in either your database initializer
	/// or your EF Migrations.
	/// It is safe to run repeatedly, and will ensure enum values are kept in line
	/// with your current code.
	/// Source code: https://github.com/timabell/ef-enum-to-lookup
	/// License: MIT
	/// </summary>
	public class EnumToLookup : IEnumToLookup
	{
		public EnumToLookup()
		{
			NameFieldLength = 255; // default
			TableNamePrefix = "Enum_";
			SplitWords = true;
		}

		/// <summary>
		/// If set to true (default) enum names will have spaces inserted between
		/// PascalCase words, e.g. enum SomeValue is stored as "Some Value".
		/// </summary>
		public bool SplitWords { get; set; }

		/// <summary>
		/// The size of the Name field that will be added to the generated lookup tables.
		/// Adjust to suit your data if required, defaults to 255.
		/// </summary>
		public int NameFieldLength { get; set; }

		/// <summary>
		/// Prefix to add to all the generated tables to separate help group them together
		/// and make them stand out as different from other tables.
		/// Defaults to "Enum_" set to null or "" to not have any prefix.
		/// </summary>
		public string TableNamePrefix { get; set; }

		public void Apply(DbContext context)
		{
			// recurese through dbsets and references finding anything that uses an enum
			var enumReferences = FindReferences(context);
			// for the list of enums generate tables
			var enums = enumReferences.Select(r => r.EnumType).Distinct().ToList();
			CreateTables(enums, (sql) => context.Database.ExecuteSqlCommand(sql));
			// t-sql merge values into table
			PopulateLookups(enums, (sql) => context.Database.ExecuteSqlCommand(sql));
			// add fks from all referencing tables
			AddForeignKeys(enumReferences, (sql) => context.Database.ExecuteSqlCommand(sql));
		}

		private void AddForeignKeys(IEnumerable<EnumReference> refs, Action<string> runSql)
		{
			foreach (var enumReference in refs)
			{
				var fkName = string.Format("FK_{0}_{1}", enumReference.ReferencingTable, enumReference.ReferencingField);
				var sql =
					string.Format(
						" IF OBJECT_ID('{0}', 'F') IS NULL ALTER TABLE [{1}] ADD CONSTRAINT {0} FOREIGN KEY ([{2}]) REFERENCES [{3}] (Id);",
						fkName, enumReference.ReferencingTable, enumReference.ReferencingField, TableName(enumReference.EnumType.Name));
				runSql(sql);
			}
		}

		private void PopulateLookups(IEnumerable<Type> enums, Action<string> runSql)
		{
			foreach (var lookup in enums)
			{
				PopulateLookup(lookup, runSql);
			}
		}

		private void PopulateLookup(Type lookup, Action<string> runSql)
		{
			if (!lookup.IsEnum)
			{
				throw new ArgumentException("Lookup type must be an enum", "lookup");
			}

			var sb = new StringBuilder();
			sb.AppendLine(string.Format("CREATE TABLE #lookups (Id int, Name nvarchar({0}));", NameFieldLength));
			foreach (var value in Enum.GetValues(lookup))
			{
				var id = (int)value;
				var name = value.ToString();
				if (SplitWords)
				{
					// http://stackoverflow.com/questions/773303/splitting-camelcase/25876326#25876326
					name = Regex.Replace(name, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled);
				}
				sb.AppendLine(string.Format("INSERT INTO #lookups (Id, Name) VALUES ({0}, '{1}');", id, name));
			}

			sb.AppendLine(string.Format(@"
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

			sb.AppendLine("DROP TABLE #lookups;");
			runSql(sb.ToString());
		}

		private void CreateTables(IEnumerable<Type> enums, Action<string> runSql)
		{
			foreach (var lookup in enums)
			{
				runSql(string.Format(
					@"CREATE TABLE [{0}] (Id int PRIMARY KEY, Name nvarchar({1}));",
					TableName(lookup.Name), NameFieldLength));
			}
		}

		private string TableName(string enumName)
		{
			return string.Format("{0}{1}", TableNamePrefix, enumName);
		}

		internal IList<EnumReference> FindReferences(DbContext context)
		{
			var enumReferences = new List<EnumReference>();
			var metadata = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;

			// Get the part of the model that contains info about the actual CLR types
			var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));

			// Get the entity type from the model that maps to the CLR type
			var entityTypes = metadata
				.GetItems<EntityType>(DataSpace.OSpace);
			//var entitiesWithEnums = entityTypes.Where(e => e.Properties.Any(p => p.IsEnumType));
			foreach (var entity in entityTypes)
			{
				var tableName = GetTableName(metadata, entity);

				foreach (var property in entity.Properties)
				{
					if (property.IsEnumType)
					{
						var clrType = objectItemCollection.GetClrType(property.EnumType);
						enumReferences.Add(new EnumReference
						{
							ReferencingTable = tableName,
							ReferencingField = property.Name,
							EnumType = clrType,
						});
					}
				}
			}
			return enumReferences;
		}

		private static string GetTableName(MetadataWorkspace metadata, EntityType entity)
		{
			// http://romiller.com/2014/04/08/ef6-1-mapping-between-types-tables/
			// Get the entity type from the model that maps to the CLR type
			var entityType = metadata
				.GetItems<EntityType>(DataSpace.OSpace)
				.Single(e => e == entity);
			// Get the entity set that uses this entity type
			var entitySet = metadata
				.GetItems<EntityContainer>(DataSpace.CSpace)
				.Single()
				.EntitySets
				.Single(s => s.ElementType.Name == entityType.Name);
			// Find the mapping between conceptual and storage model for this entity set
			var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
				.Single()
				.EntitySetMappings
				.Single(s => s.EntitySet == entitySet);

			// Find the storage entity set (table) that the entity is mapped
			var table = mapping
				.EntityTypeMappings.Single()
				.Fragments.Single()
				.StoreEntitySet;
			var tableName = (string)table.MetadataProperties["Table"].Value ?? table.Name;
			return tableName;
		}

		internal IList<PropertyInfo> FindDbSets(Type contextType)
		{
			return contextType.GetProperties()
				.Where(p => p.PropertyType.IsGenericType
										&& p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
				.ToList();
		}

		internal IList<PropertyInfo> FindEnums(Type type)
		{
			return type.GetProperties()
				.Where(p => p.PropertyType.IsEnum
										|| (p.PropertyType.IsGenericType && p.PropertyType.GenericTypeArguments.First().IsEnum))
				.ToList();
		}
	}
}
