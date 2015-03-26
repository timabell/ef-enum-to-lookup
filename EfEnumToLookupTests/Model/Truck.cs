using System.ComponentModel.DataAnnotations.Schema;

namespace EfEnumToLookupTests.Model
{
	[Table("Trucks")] // force Table-per-Type (TPT)
	public class Truck : Vehicle
	{
	}
}
