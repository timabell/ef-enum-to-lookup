using System.ComponentModel.DataAnnotations.Schema;

namespace EFTests.Model
{
    [Table("FunkyChickens")]
    public class Chicken
    {
        public int Id { get; set; }
    }
}
