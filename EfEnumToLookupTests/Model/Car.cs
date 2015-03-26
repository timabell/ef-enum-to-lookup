using System.ComponentModel.DataAnnotations.Schema;

namespace EfEnumToLookupTests.Model
{
	[Table("Cars")] // force Table-per-Type (TPT)
	public class Car : Vehicle
	{
	}
}
