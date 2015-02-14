namespace EFTests.Model
{
	public class Geology
	{
		// no id to make this a complex type to be included in the Warren
		public string Soil { get; set; }
		public int Density { get; set; }
		public Eon? Eon { get; set; }
	}
}
