namespace EFTests.Model
{
	/// <summary>
	/// For testing handling of non-int enum types
	/// https://github.com/timabell/ef-enum-to-lookup/issues/20
	/// </summary>
	public enum Fur : byte
	{
		Brown,
		Blue,
		White,
		Invisible
	}
}
