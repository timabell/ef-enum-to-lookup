using System.ComponentModel.DataAnnotations.Schema;

namespace EFTests.Model
{
	[Table("Cars")] // force Table-per-Type (TPT)
	public class Car : Vehicle
	{
	}
}
