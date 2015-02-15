using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace EfEnumToLookup.LookupGenerator
{
	public class EfTypeWrapper : IEfTypeWrapper
	{
		private readonly StructuralType _item;

		public EfTypeWrapper(StructuralType itemToWrap)
		{
			if (itemToWrap == null) throw new ArgumentNullException("itemToWrap");

			if (!IsSupported(itemToWrap))
			{
				throw new EnumGeneratorException(string.Format(
					"Unsupported StructuralType encountered while processing EF metadata. supported types: ComplexType and EntityType. Item: {0}",
					itemToWrap.FullName));
			}
			_item = itemToWrap;
		}

		public ReadOnlyMetadataCollection<EdmProperty> Properties
		{
			get
			{
				var entityType = _item as EntityType;
				if (entityType != null)
				{
					return entityType.Properties;
				}
				var complexType = _item as ComplexType;
				if (complexType != null)
				{
					return complexType.Properties;
				}
				return null; // will never happen due to check in constructor
			}
		}

		public StructuralType WrappedObject
		{
			get { return _item; }
		}

		/// <summary>
		/// Wraps complex and entity types in a n interface that allows access to the common `Properties` property
		/// even though that property isn't in the shared base class.
		/// </summary>
		/// <param name="items">The items to wrap.</param>
		/// <returns>Wrapped items</returns>
		public static IEnumerable<IEfTypeWrapper> WrapSructuralType(IEnumerable<StructuralType> items)
		{
			return from item in items
				select new EfTypeWrapper(item);
		}

		public static bool IsSupported(StructuralType type)
		{
			return type is ComplexType || type is EntityType;
		}
	}
}
