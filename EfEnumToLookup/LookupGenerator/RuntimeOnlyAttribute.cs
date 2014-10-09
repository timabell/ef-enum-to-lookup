using System;

namespace EfEnumToLookup.LookupGenerator
{
	/// <summary>
	/// Add this to an enum value to prevent it being included
	/// in generated database lookup tables.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class RuntimeOnlyAttribute : Attribute
	{
	}
}
