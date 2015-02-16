using System.Collections.Generic;

namespace EFTests.Model
{
	public class Warren
	{
		public int Id { get; set; }

		public Cave MasterChamber { get; set; }
		public int CaveId { get; set; }

		public Heat? HowHot { get; set; }

		public ICollection<Aspirations> HopesAndDreams { get; set; }

		// complex type:
		public Geology Geology { get; set; }
	}
}
