using EntityFramework.Debug.UnitTests.Models;
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

                int test = 13;

                var dump = context.DumpTrackedEntities();
                Assert.IsNotNull(dump);

                context.SaveChanges();
            }
        }
    }
}