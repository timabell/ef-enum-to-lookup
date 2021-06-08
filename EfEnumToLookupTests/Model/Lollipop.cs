using System.ComponentModel.DataAnnotations.Schema;

namespace EfEnumToLookupTests.Model
{
    [ComplexType]
    public class Lollipop
    {
        public int Length { get; set; }                
        public CandyLook Look { get; set; }
    }
}