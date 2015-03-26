using System.ComponentModel.DataAnnotations.Schema;

namespace EfEnumToLookupTests.Model
{
	[Table("Furniture")] // otherwise will be pluralised to furnitures. nothing to do with tests.
	public abstract class Furniture
	{
		public int Id { get; set; }
		public Pattern Pattern { get; set; }
	}
}
