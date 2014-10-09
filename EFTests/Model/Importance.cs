using System.ComponentModel;

namespace EFTests.Model
{
	public enum Importance
	{
		Bovverd = 1,

		[Description(Constants.BovveredDisplay)]
		NotBovverd
	}
}
