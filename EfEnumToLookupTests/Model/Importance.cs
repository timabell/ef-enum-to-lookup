using System.ComponentModel;

namespace EfEnumToLookupTests.Model
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
