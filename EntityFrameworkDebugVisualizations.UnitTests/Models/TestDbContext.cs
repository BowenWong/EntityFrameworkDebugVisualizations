using System.Data.Entity;
using EntityFramework.Debug.UnitTests.Infrastructure;

namespace EntityFramework.Debug.UnitTests.Models
{
    public class TestDbContext : DbContext
    {
        public TestDbContext()
                : base(TestBootstrapper.DbName)
        {
        }

        public IDbSet<EntityWithChild> EntitiesWithChild { get; set; }
        public IDbSet<OwnerOwned> OwnerOwneds { get; set; }
        public IDbSet<OwnerOwnedCollection> OwnerOwnedCollections { get; set; }
        public IDbSet<MultiKeyEntity> MultiKeyEntities { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OwnerOwned>().HasOptional(o => o.Owned).WithOptionalPrincipal(o => o.Owner);
            modelBuilder.Entity<OwnerOwnedCollection>().HasMany(o => o.OwnedChildren).WithOptional(o => o.Owner);
        }
    }
}