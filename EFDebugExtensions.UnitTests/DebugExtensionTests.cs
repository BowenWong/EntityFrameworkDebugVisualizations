using EntityFramework.Debug.UnitTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace EntityFramework.Debug.UnitTests
{
    [TestClass]
    public class DebugExtensionTests : Testbase
    {
        [TestMethod]
        public void TestCircularRelationship()
        {
            using (var context = new TestDbContext())
            {
                var owner = new OwnerOwned();
                context.OwnerOwneds.Add(owner);

                var owned = new OwnerOwned();
                context.OwnerOwneds.Add(owned);

                context.SaveChanges();

                owner.Owned = owned;
                owned.Owner = owner;

                var vertices = context.GetEntityVertices();
                Assert.AreEqual(2, vertices.Count(v => v.EntityType.Name == typeof(OwnerOwned).Name));
                Assert.IsTrue(vertices.All(v => v.Relations.Count == 1));
            }
        }

        [TestMethod]
        public void TestVisualizer()
        {
            using (var context = new TestDbContext())
            {
                var parent = context.EntitiesWithChild.Add(new EntityWithChild{Name = "Parent"});
                var child = context.EntitiesWithChild.Add(new EntityWithChild{Name = "FavoriteChild"});
                var secondChild = new EntityWithChild { Name = "2nd Child" };
                parent.Children.Add(secondChild);
                parent.Children.Add(new EntityWithChild {Name = "3nd Child"});
                parent.Children.Add(new EntityWithChild {Name = "4th Child"});
                context.SaveChanges();

                parent.FavoriteChild = child;
                parent.Children.Add(child);
                context.SaveChanges();

                var toDelete = context.EntitiesWithChild.Add(new EntityWithChild {Name = "Deleted"});
                context.SaveChanges();

                context.EntitiesWithChild.Remove(toDelete);
                parent.Children.Add(new EntityWithChild { Name = "1st addeded Child" });
                parent.Children.Add(new EntityWithChild { Name = "2nd addeded Child" });

                secondChild.Name = "Removed child";
                parent.Children.Remove(secondChild);

                context.ShowVisualizer();

                context.SaveChanges();
            }
        }

        [TestMethod]
        public void TestDumpNotNull()
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