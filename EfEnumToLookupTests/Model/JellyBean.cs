using System.ComponentModel.DataAnnotations.Schema;

namespace EfEnumToLookupTests.Model
{
    [ComplexType]
    public class JellyBean
    {     
        public int Count { get; set; }
        public CandyLook Look { get; set; }
    }
}