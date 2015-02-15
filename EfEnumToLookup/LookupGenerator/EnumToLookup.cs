using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
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

		/// <summary>
		/// Suffix to add to all the generated tables to separate help group them together
		/// and make them stand out as different from other tables.
		/// Defaults to "" set to null or "" to not have any suffix.
		/// </summary>
		public string TableNameSuffix { get; set; }

		/// <summary>
		/// Create any missing lookup tables,
		/// enforce values in the lookup tables
		/// by way of a T-SQL MERGE
		/// </summary>
		/// <param name="context">EF Database context to search for enum references,
		///  context.Database.ExecuteSqlCommand() is used to apply changes.</param>
		public void Apply(DbContext context)
		{
			// recurese through dbsets and references finding anything that uses an enum
			var enumReferences = FindReferences(context);
			// for the list of enums generate tables
			var enums = enumReferences.Select(r => r.EnumType).Distinct().ToList();
			CreateTables(enums, (sql) => context.Database.ExecuteSqlCommand(sql));
			// t-sql merge values into table
			PopulateLookups(enums, (sql, parameters) => context.Database.ExecuteSqlCommand(sql, parameters.Cast<object>().ToArray()));
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

		private void PopulateLookups(IEnumerable<Type> enums, Action<string, IEnumerable<SqlParameter>> runSql)
		{
			foreach (var lookup in enums)
			{
				PopulateLookup(lookup, runSql);
			}
		}

		private void PopulateLookup(Type lookup, Action<string, IEnumerable<SqlParameter>> runSql)
		{
			if (!lookup.IsEnum)
			{
				throw new ArgumentException("Lookup type must be an enum", "lookup");
			}

			var sb = new StringBuilder();
			sb.AppendLine(string.Format("CREATE TABLE #lookups (Id int, Name nvarchar({0}) COLLATE database_default);", NameFieldLength));
			var parameters = new List<SqlParameter>();
			int paramIndex = 0;
			foreach (var value in Enum.GetValues(lookup))
			{
				if (IsRuntimeOnly(value, lookup))
				{
					continue;
				}
				var id = (int)value;
				var name = EnumName(value, lookup);
				var idParamName = string.Format("id{0}", paramIndex++);
				var nameParamName = string.Format("name{0}", paramIndex++);
				sb.AppendLine(string.Format("INSERT INTO #lookups (Id, Name) VALUES (@{0}, @{1});", idParamName, nameParamName));
				parameters.Add(new SqlParameter(idParamName, id));
				parameters.Add(new SqlParameter(nameParamName, name));
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
			runSql(sb.ToString(), parameters);
		}

		private string EnumName(object value, Type lookup)
		{
			var description = DescriptionValue(value, lookup);
			if (description != null)
			{
				return description;
			}

			var name = value.ToString();

			if (SplitWords)
			{
				return SplitCamelCase(name);
			}
			return name;
		}

		private static string SplitCamelCase(string name)
		{
			// http://stackoverflow.com/questions/773303/splitting-camelcase/25876326#25876326
			name = Regex.Replace(name, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled);
			return name;
		}

		private string DescriptionValue(object value, Type enumType)
		{
			// https://stackoverflow.com/questions/1799370/getting-attributes-of-enums-value/1799401#1799401
			var member = enumType.GetMember(value.ToString()).First();
			var description = member.GetCustomAttributes(typeof(DescriptionAttribute)).FirstOrDefault() as DescriptionAttribute;
			return description == null ? null : description.Description;
		}

		private bool IsRuntimeOnly(object value, Type enumType)
		{
			// https://stackoverflow.com/questions/1799370/getting-attributes-of-enums-value/1799401#1799401
			var member = enumType.GetMember(value.ToString()).First();
			return member.GetCustomAttributes(typeof(RuntimeOnlyAttribute)).Any();
		}

		private void CreateTables(IEnumerable<Type> enums, Action<string> runSql)
		{
			foreach (var lookup in enums)
			{
				runSql(string.Format(
					@"IF OBJECT_ID('{0}', 'U') IS NULL CREATE TABLE [{0}] (Id int PRIMARY KEY, Name nvarchar({1}));",
					TableName(lookup.Name), NameFieldLength));
			}
		}

		private string TableName(string enumName)
		{
			return string.Format("{0}{1}{2}", TableNamePrefix, enumName, TableNameSuffix);
		}

		internal IList<EnumReference> FindReferences(DbContext context)
		{
			var metadata = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;

			// Get the part of the model that contains info about the actual CLR types
			var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace)); // OSpace = Object Space

			//var everything = metadata.GetItems(DataSpace.OSpace).ToList();
			var entities = metadata.GetItems<EntityType>(DataSpace.OSpace).ToList(); // this one
			var complex = metadata.GetItems<ComplexType>(DataSpace.OSpace).ToList(); // and this one
			var structural = metadata.GetItems<StructuralType>(DataSpace.OSpace).ToList(); // common parent
			// find and return all the references to enum types
			var enumReferences = (from entity in WrapSructuralType(metadata.GetItems<StructuralType>(DataSpace.OSpace))
				from property in entity.Properties
				where property.IsEnumType
				select new EnumReference
				{
					ReferencingTable = GetTableName(metadata, entity.WrappedObject),
					ReferencingField = property.Name,
					EnumType = objectItemCollection.GetClrType(property.EnumType),
				});
			return enumReferences
				.Where(r => r.ReferencingTable != null) // filter out child-types in Table-per-Hierarchy model
				.ToList();
		}

		/// <summary>
		/// Wraps complex and entity types in a n interface that allows access to the common `Properties` property
		/// even though that property isn't in the shared base class.
		/// </summary>
		/// <param name="items">The items to wrap.</param>
		/// <returns>Wrapped items</returns>
		private IEnumerable<IEfTypeWrapper> WrapSructuralType(ReadOnlyCollection<StructuralType> items)
		{
			return from item in items
				select new EfTypeWrapper(item);
		}

		private static string GetTableName(MetadataWorkspace metadata, StructuralType entityType)
		{
			// refs:
			// * http://romiller.com/2014/04/08/ef6-1-mapping-between-types-tables/
			// * http://blogs.msdn.com/b/appfabriccat/archive/2010/10/22/metadataworkspace-reference-in-wcf-services.aspx
			// * http://msdn.microsoft.com/en-us/library/system.data.metadata.edm.dataspace.aspx - describes meaning of OSpace etc

			try
			{
				// Get the entity type from the model that maps to the CLR type
				var entityTypes = metadata
					.GetItems<EntityType>(DataSpace.OSpace) // OSpace = Object Space
					.Where(e => e == entityType)
					.ToList();
				if (entityTypes.Count() != 1)
				{
					throw new EnumGeneratorException(string.Format("{0} entities of type {1} found in mapping.", entityTypes.Count(), entityType));
				}
				var entityMetadata = entityTypes.Single();

				// Get the entity set that uses this entity type
				var containers = metadata
					.GetItems<EntityContainer>(DataSpace.CSpace); // CSpace = Conceptual model
				if (containers.Count() != 1)
				{
					throw new EnumGeneratorException(string.Format("{0} EntityContainer's found.", containers.Count()));
				}
				var container = containers.Single();

				var entitySets = container
					.EntitySets
					.Where(s => s.ElementType.Name == entityMetadata.Name)
					.ToList();
				// Child types in Table-per-Hierarchy don't have any mapping so return null for the table name. Foreign key will be from the parent/base type.
				if (!entitySets.Any())
				{
					return null;
				}
				if (entitySets.Count() != 1)
				{
					throw new EnumGeneratorException(string.Format(
						"{0} EntitySet's found for element type '{1}'.", entitySets.Count(), entityMetadata.Name));
				}
				var entitySet = entitySets.Single();

				// Find the mapping between conceptual and storage model for this entity set
				var entityContainerMappings = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace); // CSSpace = Conceptual model to Storage model mappings
				if (entityContainerMappings.Count() != 1)
				{
					throw new EnumGeneratorException(string.Format("{0} EntityContainerMappings found.", entityContainerMappings.Count()));
				}
				var containerMapping = entityContainerMappings.Single();
				var mappings = containerMapping.EntitySetMappings.Where(s => s.EntitySet == entitySet).ToList();
				if (mappings.Count() != 1)
				{
					throw new EnumGeneratorException(string.Format(
						"{0} EntitySetMappings found for entitySet '{1}'.", mappings.Count(), entitySet.Name));
				}
				var mapping = mappings.Single();

				// Find the storage entity set (table) that the entity is mapped to
				var entityTypeMappings = mapping.EntityTypeMappings;
				var entityTypeMapping = entityTypeMappings.First(); // using First() because Table-per-Hierarchy (TPH) produces multiple copies of the entity type mapping
				var fragments = entityTypeMapping.Fragments;
				if (fragments.Count() != 1)
				{
					throw new EnumGeneratorException(string.Format("{0} Fragments found.", fragments.Count()));
				}
				var table = fragments.Single().StoreEntitySet;
				var tableName = (string)table.MetadataProperties["Table"].Value ?? table.Name;
				return tableName;
			}
			catch (Exception exception)
			{
				throw new EnumGeneratorException(string.Format("Error getting table name for entity type '{0}'", entityType.Name), exception);
			}
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
