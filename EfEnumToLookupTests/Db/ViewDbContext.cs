using System.Data.Entity;
using EfEnumToLookupTests.Model;

namespace EfEnumToLookupTests.Db
{
	/// <summary>Context containing a view</summary>
	public class ViewDbContext : DbContext
	{
		public ViewDbContext(string nameOrConnectionString) : base(nameOrConnectionString) { } // to override db naming convention to use existing db created by MagicContext

		public DbSet<ViewRabbit> ViewRabbits { get; set; }
	}
}
