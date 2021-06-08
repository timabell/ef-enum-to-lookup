using System.ComponentModel.DataAnnotations.Schema;

namespace EfEnumToLookupTests.Model
{
    [ComplexType]
    public class ThreeChoicesOfLollipops
    {
        public Lollipop Lollipop1 { get; set; }

        public Lollipop Lollipop2 { get; set; }

        public Lollipop Lollipop3 { get; set; }
    }
}