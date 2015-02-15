using System.Data.Entity.Core.Metadata.Edm;

namespace EfEnumToLookup.LookupGenerator
{
	/// <summary>
	/// This is a workaround to be able to handle ComplexType and EntityType together
	/// </summary>
	public interface IEfTypeWrapper
	{
		/// <summary>
		/// Access the `Properties` property of the wrapped object
		/// </summary>
		ReadOnlyMetadataCollection<EdmProperty> Properties { get; }

		/// <summary>
		/// Gets the object that's been wrapped
		/// </summary>
		StructuralType WrappedObject { get; }
	}
}
