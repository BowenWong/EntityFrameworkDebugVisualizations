using EntityFramework.Debug.DebugVisualization;
using EntityFramework.Debug.UnitTests.Models;
using Microsoft.VisualStudio.DebuggerVisualizers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Debug.UnitTests
{
    [TestClass]
    public class DebugExtensionTests
    {
        [TestMethod]
        public void TestDumpNotNull()
        {
            using (var context = new TestDbContext())
            {
                context.Entities.Add(new Entity());

                TestShowVisualizer("Andy");

                var dump = context.DumpTrackedEntities();
                Assert.IsNotNull(dump);

                context.SaveChanges();
            }
        }

        private static void TestShowVisualizer(object obj)
        {
            var host = new VisualizerDevelopmentHost(obj, typeof(DbContextDebuggerVisualizer));
            host.ShowVisualizer();
        }
    }
}