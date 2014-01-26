using System;
using System.Data.Entity;
using EntityFramework.Debug.UnitTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Debug.UnitTests.Infrastructure
{
    [TestClass]
    public class TestBootstrapper
    {
        internal static readonly string DbName = "EntityFrameworkDebugVisualizations.UnitTests" + Guid.NewGuid();

        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<TestDbContext>());

            using (var db = new TestDbContext())
            {
                if (db.Database.Exists())
                    db.Database.Delete();

                db.Database.Create();
            }
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            using (var db = new TestDbContext())
            {
                if (db.Database.Exists())
                    db.Database.Delete();
            }
        }
    }
}