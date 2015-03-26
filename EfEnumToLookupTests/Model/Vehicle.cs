namespace EfEnumToLookupTests.Model
{
	public abstract class Vehicle
	{
		public int Id { get; set; }
		public string RegistrationPlate { get; set; }
		public Category Category { get; set; }
	}
}
