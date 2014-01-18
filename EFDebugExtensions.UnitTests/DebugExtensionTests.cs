using EntityFramework.Debug.DebugVisualization;
using EntityFramework.Debug.UnitTests.Models;
using Microsoft.VisualStudio.DebuggerVisualizers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Debug.UnitTests
{
    [TestClass]
    public class DebugExtensionTests : Testbase
    {
        [TestMethod]
        public void TestVisualizer()
        {
            EntityWithChild parent;

            using (var context = new TestDbContext())
            {
                parent = context.EntitiesWithChild.Add(new EntityWithChild{Name = "Parent"});
                var child = context.EntitiesWithChild.Add(new EntityWithChild{Name = "Child"});
                parent.Child = child;
                context.SaveChanges();
            }

            using (var context = new TestDbContext())
            {
                context.EntitiesWithChild.Attach(parent);
                var toDelete = context.EntitiesWithChild.Add(new EntityWithChild {Name = "Deleted"});
                context.SaveChanges();

                context.EntitiesWithChild.Remove(toDelete);
                context.EntitiesWithChild.Add(new EntityWithChild {Name = "Added"});

                ShowVisualizer(context);

                context.SaveChanges();
            }
        }

        private static void ShowVisualizer(object obj)
        {
            new VisualizerDevelopmentHost(obj, typeof(DbContextDebuggerVisualizer), typeof(DbContextVisualizerObjectSource)).ShowVisualizer();
        }

        [TestMethod]
        public void TestDumpNotNull()
        {
            using (var context = new TestDbContext())
            {
                var child = context.EntitiesWithChild.Add(new EntityWithChild());
                context.EntitiesWithChild.Add(new EntityWithChild { Child = child });
                context.SaveChanges();

                context.EntitiesWithChild.Add(new EntityWithChild());

                var dump = context.DumpTrackedEntities();
                Assert.IsNotNull(dump);

                context.SaveChanges();
            }
        }
    }
}