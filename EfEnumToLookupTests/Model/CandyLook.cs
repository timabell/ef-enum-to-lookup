using System.ComponentModel.DataAnnotations.Schema;

namespace EfEnumToLookupTests.Model
{
    [ComplexType]
    public class CandyLook
    {
        public string Shape { get; set; }
        public CandySize Size { get; set; }
    }
}