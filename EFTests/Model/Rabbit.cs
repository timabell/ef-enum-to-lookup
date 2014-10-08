namespace EFTests.Model
{
	public class Rabbit
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public Ears TehEars { get; set; }

		public Legs? FrontLegs { get; set; }

		public Legs? BackLegs { get; set; }
	}
}
