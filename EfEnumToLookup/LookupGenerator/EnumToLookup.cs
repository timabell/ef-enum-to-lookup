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
        public void Apply(DbContext context)
        {
            // recurese through dbsets and references finding anything that uses an enum
            var refs = FindReferences(context.GetType());
            // for the list of enums generate tables
            // t-sql merge values into table
            // add fks from all referencing tables
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
                            EnumType = enumProp.PropertyType,
                        }
                    ));
            }
            return enumReferences;
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
