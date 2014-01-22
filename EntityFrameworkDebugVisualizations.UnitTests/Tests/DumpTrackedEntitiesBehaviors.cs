using EntityFramework.Debug.UnitTests.Infrastructure;
using EntityFramework.Debug.UnitTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Debug.UnitTests.Tests
{
    [TestClass]
    public class DumpTrackedEntitiesBehaviors : Testbase
    {
        [TestMethod]
        public void ShouldDumpNonNullText()
        {
            using (var context = new TestDbContext())
            {
                var child = context.EntitiesWithChild.Add(new EntityWithChild());
                context.EntitiesWithChild.Add(new EntityWithChild { FavoriteChild = child });
                context.SaveChanges();

                context.EntitiesWithChild.Add(new EntityWithChild());

                var dump = context.DumpTrackedEntities();
                Assert.IsNotNull(dump);

                context.SaveChanges();
            }
        }
    }
}
