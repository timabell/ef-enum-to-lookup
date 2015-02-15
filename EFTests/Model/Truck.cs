using System.ComponentModel.DataAnnotations.Schema;

namespace EFTests.Model
{
	[Table("Trucks")] // force Table-per-Type (TPT)
	public class Truck : Vehicle
	{
	}
}
