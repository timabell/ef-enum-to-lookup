namespace EfEnumToLookup.LookupGenerator
{
	public class EnumToLookupConfiguration
	{
		public EnumToLookupConfiguration()
		{
			NameFieldLength = 255;
			TableNamePrefix = "Enum_";
			TableNameSuffix = string.Empty;
			SchemaName = "dbo";
			UseTransaction = true;
		}

		/// <summary>
		///     The size of the Name field that will be added to the generated lookup tables.
		///     Adjust to suit your data if required, defaults to 255.
		/// </summary>
		public int NameFieldLength { get; set; }

		/// <summary>
		///     Prefix to add to all the generated tables to separate help group them together
		///     and make them stand out as different from other tables.
		///     Defaults to "Enum_" set to null or "" to not have any prefix.
		/// </summary>
		public string TableNamePrefix { get; set; }

		/// <summary>
		///     Suffix to add to all the generated tables to separate help group them together
		///     and make them stand out as different from other tables.
		///     Defaults to "" set to null or "" to not have any suffix.
		/// </summary>
		public string TableNameSuffix { get; set; }

		/// <summary>
		///     Whether to run the changes inside a database transaction.
		/// </summary>
		public bool UseTransaction { get; set; }

		/// <summary>
		///     The SQL Schema to put all the Lookup Tables into
		///     Defaults to "dbo"
		/// </summary>
		public string SchemaName { get; set; }

		/// <summary>
		///     If set to true (default) enum names will have spaces inserted between
		///     PascalCase words, e.g. enum SomeValue is stored as "Some Value".
		/// </summary>
		public bool SplitWords { get; set; }
	}
}