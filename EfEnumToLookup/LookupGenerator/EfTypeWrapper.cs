using System;
using System.Data.Entity.Core.Metadata.Edm;

namespace EfEnumToLookup.LookupGenerator
{
	public class EfTypeWrapper : IEfTypeWrapper
	{
		private readonly StructuralType _item;

		public EfTypeWrapper(StructuralType itemToWrap)
		{
			if (itemToWrap == null) throw new ArgumentNullException("itemToWrap");

			if (!(itemToWrap is ComplexType) && !(itemToWrap is EntityType))
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
	}
}
