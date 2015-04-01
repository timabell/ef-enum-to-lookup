namespace EfEnumToLookup.LookupGenerator
{
	using System;
	using System.Collections.Generic;
	using System.Data.Entity.Core.Mapping;
	using System.Data.Entity.Core.Metadata.Edm;
	using System.Linq;

	class MetadataHandler
	{
		// refs:
		// * http://romiller.com/2014/04/08/ef6-1-mapping-between-types-tables/
		// * http://blogs.msdn.com/b/appfabriccat/archive/2010/10/22/metadataworkspace-reference-in-wcf-services.aspx
		// * http://msdn.microsoft.com/en-us/library/system.data.metadata.edm.dataspace.aspx - describes meaning of OSpace etc
		// * http://stackoverflow.com/questions/22999330/mapping-from-iedmentity-to-clr

		internal IList<EnumReference> FindEnumReferences(MetadataWorkspace metadataWorkspace)
		{
			// Get the part of the model that contains info about the actual CLR types
			var objectItemCollection = ((ObjectItemCollection)metadataWorkspace.GetItemCollection(DataSpace.OSpace));
			// OSpace = Object Space

			var entities = metadataWorkspace.GetItems<EntityType>(DataSpace.OSpace);

			// find and return all the references to enum types
			var references = new List<EnumReference>();
			foreach (var entityType in entities)
			{
				var mappingFragment = FindSchemaMappingFragment(metadataWorkspace, entityType);

				// child types in TPH don't get mappings
				if (mappingFragment == null)
				{
					continue;
				}

				references.AddRange(ProcessEdmProperties(entityType.Properties, mappingFragment, objectItemCollection));
			}
			return references;
		}

		/// <summary>
		/// Loop through all the specified properties, including the children of any complex type properties, looking for enum types.
		/// </summary>
		/// <param name="properties">The properties to search.</param>
		/// <param name="mappingFragment">Information needed from ef metadata to map table and its columns</param>
		/// <param name="objectItemCollection">For looking up ClrTypes of any enums encountered</param>
		/// <returns>All the references that were found in a form suitable for creating lookup tables and foreign keys</returns>
		private static IEnumerable<EnumReference> ProcessEdmProperties(IEnumerable<EdmProperty> properties, MappingFragment mappingFragment, ObjectItemCollection objectItemCollection)
		{
			var references = new List<EnumReference>();

			// get mapped table name from mapping, or fall-back to just the name if no mapping is set,
			// I have no idea what causes Table to be null, and I have no unit test for it yet, but I have seen it.
			var table = mappingFragment.StoreEntitySet.Table ?? mappingFragment.StoreEntitySet.Name;

			foreach (var edmProperty in properties)
			{
				if (edmProperty.IsEnumType)
				{
					references.Add(new EnumReference
					{
						ReferencingTable = table,
						ReferencingField = GetColumnName(mappingFragment, edmProperty),
						EnumType = objectItemCollection.GetClrType(edmProperty.EnumType),
					});
					continue;
				}

				if (edmProperty.IsComplexType)
				{
					// Note that complex types can't be nested (ref http://stackoverflow.com/a/20332503/10245 )
					// so it's safe to not recurse even though the data model suggests you should have to.
					references.AddRange(
						from nestedProperty in edmProperty.ComplexType.Properties
						where nestedProperty.IsEnumType
						select new EnumReference
						{
							ReferencingTable = table,
							ReferencingField = GetColumnName(mappingFragment, edmProperty, nestedProperty),
							EnumType = objectItemCollection.GetClrType(nestedProperty.EnumType),
						});
				}
			}

			return references;
		}

		/// <summary>
		/// Gets the name of the column for the property from the metadata.
		/// Set nestedProperty for the property of a complex type to lookup.
		/// </summary>
		/// <param name="mappingFragment">EF metadata for finding mappings.</param>
		/// <param name="edmProperty">The of the model to find the column for (for simple types), or for complex types this is the containing complex type.</param>
		/// <param name="nestedProperty">Only required to map complex types. The property of the complex type to find the column name for.</param>
		/// <returns>The column name for the property</returns>
		/// <exception cref="EnumGeneratorException">
		/// </exception>
		private static string GetColumnName(StructuralTypeMapping mappingFragment, EdmProperty edmProperty, EdmProperty nestedProperty = null)
		{
			var propertyMapping = GetPropertyMapping(mappingFragment, edmProperty);

			if (nestedProperty != null)
			{
				var complexPropertyMapping = propertyMapping as ComplexPropertyMapping;
				if (complexPropertyMapping == null)
				{
					throw new EnumGeneratorException(string.Format(
						"Failed to cast complex property mapping for {0}.{1} to ComplexPropertyMapping", edmProperty, nestedProperty));
				}
				var complexTypeMappings = complexPropertyMapping.TypeMappings;
				if (complexTypeMappings.Count() != 1)
				{
					throw new EnumGeneratorException(string.Format(
						"{0} complexPropertyMapping TypeMappings found for property {1}.{2}", complexTypeMappings.Count(), edmProperty, nestedProperty));
				}
				var complexTypeMapping = complexTypeMappings.Single();
				var propertyMappings = complexTypeMapping.PropertyMappings.Where(pm => pm.Property.Name == nestedProperty.Name).ToList();
				if (propertyMappings.Count() != 1)
				{
					throw new EnumGeneratorException(string.Format(
						"{0} complexMappings found for property {1}.{2}", propertyMappings.Count(), edmProperty, nestedProperty));
				}

				propertyMapping = propertyMappings.Single();
			}

			return GetColumnNameFromPropertyMapping(edmProperty, propertyMapping);
		}

		private static string GetColumnNameFromPropertyMapping(EdmProperty edmProperty, PropertyMapping propertyMapping)
		{
			var colMapping = propertyMapping as ScalarPropertyMapping;
			if (colMapping == null)
			{
				throw new EnumGeneratorException(string.Format(
					"Expected ScalarPropertyMapping but found {0} when mapping property {1}", propertyMapping.GetType(), edmProperty));
			}
			return colMapping.Column.Name;
		}

		private static PropertyMapping GetPropertyMapping(StructuralTypeMapping mappingFragment, EdmProperty edmProperty)
		{
			var matches = mappingFragment.PropertyMappings.Where(m => m.Property.Name == edmProperty.Name).ToList();
			if (matches.Count() != 1)
			{
				throw new EnumGeneratorException(string.Format(
					"{0} matches found for property {1}", matches.Count(), edmProperty));
			}
			var match = matches.Single();
			return match;
		}

		private static MappingFragment FindSchemaMappingFragment(MetadataWorkspace metadata, EntityType entityType)
		{
			try
			{
				var conceptualEntitySet = FindConceptualEntity(metadata, entityType);

				// Child types in Table-per-Hierarchy don't have any mappings defined as they don't add any new tables, so skip them.
				if (conceptualEntitySet == null)
				{
					return null;
				}

				return FindStorageMappingFragmentFromConceptual(metadata, conceptualEntitySet);
			}
			catch (Exception exception)
			{
				throw new EnumGeneratorException(string.Format("Error getting schema mappings for entity type '{0}'", entityType.Name), exception);
			}
		}

		private static EntitySet FindConceptualEntity(MetadataWorkspace metadata, EntityType entityType)
		{
			var entityMetadata = FindObjectSpaceEntityMetadata(metadata, entityType);

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
				// doesn't seem to be possible to get at the Object-Conceptual mappings from the public API so match on name.
				.ToList();

			// Child types in Table-per-Hierarchy don't have any mappings defined as they don't add any new tables, so skip them.
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

			return entitySet;
		}

		private static EntityType FindObjectSpaceEntityMetadata(MetadataWorkspace metadata, EntityType entityType)
		{
			// Get the entity type from the model that maps to the CLR type
			var entityTypes = metadata
				.GetItems<EntityType>(DataSpace.OSpace) // OSpace = Object Space
				.Where(e => e == entityType)
				.ToList();
			if (entityTypes.Count() != 1)
			{
				throw new EnumGeneratorException(string.Format("{0} entities of type {1} found in mapping.", entityTypes.Count(),
					entityType));
			}
			var entityMetadata = entityTypes.Single();
			return entityMetadata;
		}

		private static MappingFragment FindStorageMappingFragmentFromConceptual(MetadataWorkspace metadata, EntitySet conceptualEntitySet)
		{
			var storageMapping = FindStorageMapping(metadata, conceptualEntitySet);

			return FindStorageMappingFragmentInStorageMapping(storageMapping);
		}

		private static MappingFragment FindStorageMappingFragmentInStorageMapping(EntitySetMapping storageMapping)
		{
			// Find the storage mapping fragment that the entity is mapped to
			var entityTypeMappings = storageMapping.EntityTypeMappings;
			var entityTypeMapping = entityTypeMappings.First();
			// using First() because Table-per-Hierarchy (TPH) produces multiple copies of the entity type mapping
			var fragments = entityTypeMapping.Fragments;
			if (fragments.Count() != 1)
			{
				throw new EnumGeneratorException(string.Format("{0} Fragments found.", fragments.Count()));
			}
			var fragment = fragments.Single();
			return fragment;
		}

		private static EntitySetMapping FindStorageMapping(MetadataWorkspace metadata, EntitySet conceptualEntitySet)
		{
			// Find the mapping between conceptual and storage model for this entity set
			var entityContainerMappings = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace);
			// CSSpace = Conceptual model to Storage model mappings
			if (entityContainerMappings.Count() != 1)
			{
				throw new EnumGeneratorException(string.Format("{0} EntityContainerMappings found.", entityContainerMappings.Count()));
			}
			var containerMapping = entityContainerMappings.Single();
			var mappings = containerMapping.EntitySetMappings.Where(s => s.EntitySet == conceptualEntitySet).ToList();
			if (mappings.Count() != 1)
			{
				throw new EnumGeneratorException(string.Format(
					"{0} EntitySetMappings found for entitySet '{1}'.", mappings.Count(), conceptualEntitySet.Name));
			}
			var mapping = mappings.Single();
			return mapping;
		}
	}
}
