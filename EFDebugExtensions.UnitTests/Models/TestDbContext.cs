using System.Data.Entity;

namespace EntityFramework.Debug.UnitTests.Models
{
    public class TestDbContext : DbContext
    {
        public TestDbContext()
                : base("EFDebugExtensions.UnitTests")
        {
        }

        public IDbSet<EntityWithChild> EntitiesWithChild { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}