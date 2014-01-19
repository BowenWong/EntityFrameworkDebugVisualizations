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

                ShowVisualizer(context);

                context.SaveChanges();
            }
        }

        private static void ShowVisualizer(object obj)
        {
            new VisualizerDevelopmentHost(obj, typeof(ContextDebuggerVisualizer), typeof(ContextVisualizerObjectSource)).ShowVisualizer();
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