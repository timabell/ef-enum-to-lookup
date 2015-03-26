using System.ComponentModel.DataAnnotations.Schema;

namespace EfEnumToLookupTests.Model
{
	[Table("FunkyChickens")]
	public class Chicken
	{
		public int Id { get; set; }
	}
}
