using System.ComponentModel.DataAnnotations.Schema;

namespace EfEnumToLookupTests.Model
{
    [Table(nameof(CandyBag))]
    public class CandyBag
    {
        public int Id { get; set; }
        public ThreeChoicesOfLollipops Lollipops { get; set; }
        public JellyBean Jellybeans { get; set; }
    }
}