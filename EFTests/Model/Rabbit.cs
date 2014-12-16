namespace EFTests.Model
{
	public class Rabbit
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public Ears TehEars { get; set; }

		public Legs? SpeedyLegs { get; set; }

		public Relation? Offspring { get; set; }
	}
}
