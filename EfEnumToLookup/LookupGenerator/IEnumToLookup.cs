using System.Data.Entity;

namespace EfEnumToLookup.LookupGenerator
{
    public interface IEnumToLookup
    {
        void Apply(DbContext context);
    }
}
