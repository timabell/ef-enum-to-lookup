using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("EFTests")]

namespace EfEnumToLookup.LookupGenerator
{
    public class EnumToLookup : IEnumToLookup
    {
        public EnumToLookup()
        {
            NameFieldLength = 255; // default
        }

        /// <summary>
        /// The size of the Name field that will be added to the generated lookup tables.
        /// Adjust to suit your data if required, defaults to 255.
        /// </summary>
        public int NameFieldLength { get; set; }

        public void Apply(DbContext context)
        {
            // recurese through dbsets and references finding anything that uses an enum
            var refs = FindReferences(context.GetType());
            // for the list of enums generate tables
            var enums = refs.Select(r => r.EnumType).Distinct();
            CreateTables(enums, (sql) => context.Database.ExecuteSqlCommand(sql));
            // t-sql merge values into table
            // add fks from all referencing tables
        }

        private void CreateTables(IEnumerable<Type> enums, Action<string> runSql)
        {
            foreach (var lookup in enums)
            {
                runSql(string.Format(
                    @"CREATE TABLE [{0}] (Id int, Name nvarchar({1}));",
                    lookup.Name, NameFieldLength));
            }
        }

        internal IList<EnumReference> FindReferences(Type contextType)
        {
            var dbSets = FindDbSets(contextType);
            var enumReferences = new List<EnumReference>();
            foreach (var dbSet in dbSets)
            {
                var dbSetType = DbSetType(dbSet);
                var enumProperties = FindEnums(dbSetType);
                enumReferences.AddRange(enumProperties
                    .Select(enumProp => new EnumReference
                        {
                            // todo: apply fluent / attribute name changes
                            ReferencingTable = dbSet.Name,
                            ReferencingField = enumProp.Name,
                            EnumType = UnwrapIfNullable(enumProp.PropertyType),
                        }
                    ));
            }
            return enumReferences;
        }

        private static Type UnwrapIfNullable(Type type)
        {
            if (!type.IsGenericType)
            {
                return type;
            }
            if (type.GetGenericTypeDefinition() != typeof(Nullable<>))
            {
                throw new NotSupportedException(string.Format("Unexpected generic enum type in model: {0}, expected non-generic or nullable.", type));
            }
            return type.GenericTypeArguments.First();
        }

        /// <summary>
        /// Unwraps the type inside a DbSet&lt;&gt;
        /// </summary>
        private static Type DbSetType(PropertyInfo dbSet)
        {
            return dbSet.PropertyType.GenericTypeArguments.First();
        }

        internal IList<PropertyInfo> FindDbSets(Type contextType)
        {
            return contextType.GetProperties()
                .Where(p => p.PropertyType.IsGenericType
                    && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .ToList();
        }

        public IList<PropertyInfo> FindEnums(Type type)
        {
            return type.GetProperties()
                .Where(p => p.PropertyType.IsEnum
                    || (p.PropertyType.IsGenericType && p.PropertyType.GenericTypeArguments.First().IsEnum))
                .ToList();
        }
    }
}
