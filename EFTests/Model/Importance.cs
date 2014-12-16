using System.ComponentModel;

namespace EFTests.Model
{
	public enum Importance
	{
		Bovverd = 1,

		[Description(Constants.AintBovveredDisplay)]
		AintBovverd,

		[Description(Constants.BovveredDisplay)]
		NotBovverd
	}
}
