using System;
using System.Diagnostics;

namespace EfEnumToLookup.LookupGenerator
{
	/// <summary>
	/// Information needed to construct a foreign key from a referencing
	/// table to a generated enum lookup.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	internal class EnumReference
	{
		public string ReferencingTable { get; set; }
		public string ReferencingField { get; set; }
		public string ReferencingSchema { get; set; }
		public Type EnumType { get; set; }

		// ReSharper disable once UnusedMember.Local
		private string DebuggerDisplay
		{
			get { return $"EnumReference: {ReferencingSchema}.{ReferencingTable}.{ReferencingField} ({EnumType.Name})"; }
		}
	}
}
