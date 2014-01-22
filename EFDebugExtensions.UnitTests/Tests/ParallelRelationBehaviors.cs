using System.Data.Entity;
using System.Linq;
using EntityFramework.Debug.UnitTests.Infrastructure;
using EntityFramework.Debug.UnitTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Debug.UnitTests.Tests
{
    [TestClass]
    public class ParallelRelationBehaviors : Testbase
    {
        [TestMethod]
        public void ShouldSeeAddedParallelRelations()
        {
            using (var context = new TestDbContext())
            {
                var child = new EntityWithChild();
                context.EntitiesWithChild.Add(child);
                var parent = new EntityWithChild();
                context.EntitiesWithChild.Add(parent);
                context.SaveChanges();

                parent.FavoriteChild = child;
                parent.Children.Add(child);

                var vertices = context.GetEntityVertices();

                Assert.AreEqual(2, vertices.Count);
                var parentVertex = GetVertexByIdProperty(vertices, parent.Id);
                var childVertex = GetVertexByIdProperty(vertices, child.Id);

                Assert.IsTrue(parentVertex.Relations.All(r => r.ContainsMultipleRelations));
                Assert.IsTrue(parentVertex.Relations.All(r => r.Target == childVertex));
                Assert.AreEqual(1, parentVertex.Relations.Count(r => r.State == EntityState.Added));
                Assert.AreEqual(2, parentVertex.Relations.SelectMany(r => r.Relations).Count(r => r.State == EntityState.Added));

                Assert.AreEqual(0, childVertex.Relations.Count());
            }
        }

        [TestMethod]
        public void ShouldSeeUnchangedParallelRelations()
        {
            using (var context = new TestDbContext())
            {
                var child = new EntityWithChild();
                context.EntitiesWithChild.Add(child);
                var parent = new EntityWithChild();
                context.EntitiesWithChild.Add(parent);
                context.SaveChanges();

                parent.FavoriteChild = child;
                parent.Children.Add(child);
                context.SaveChanges();

                var vertices = context.GetEntityVertices();

                Assert.AreEqual(2, vertices.Count);
                var parentVertex = GetVertexByIdProperty(vertices, parent.Id);
                var childVertex = GetVertexByIdProperty(vertices, child.Id);

                Assert.IsTrue(parentVertex.Relations.All(r => r.ContainsMultipleRelations));
                Assert.IsTrue(parentVertex.Relations.All(r => r.Target == childVertex));
                Assert.AreEqual(1, parentVertex.Relations.Count(r => r.State == EntityState.Unchanged));
                Assert.AreEqual(2, parentVertex.Relations.SelectMany(r => r.Relations).Count(r => r.State == EntityState.Unchanged));

                Assert.AreEqual(0, childVertex.Relations.Count());
            }
        }

        [TestMethod]
        public void ShouldSeeRemovedParallelRelations()
        {
            using (var context = new TestDbContext())
            {
                var child = new EntityWithChild();
                context.EntitiesWithChild.Add(child);
                var parent = new EntityWithChild();
                context.EntitiesWithChild.Add(parent);
                context.SaveChanges();

                parent.FavoriteChild = child;
                parent.Children.Add(child);
                context.SaveChanges();

                parent.FavoriteChild = null;
                parent.Children.Remove(child);

                var vertices = context.GetEntityVertices();

                Assert.AreEqual(2, vertices.Count);
                var parentVertex = GetVertexByIdProperty(vertices, parent.Id);
                var childVertex = GetVertexByIdProperty(vertices, child.Id);

                Assert.IsTrue(parentVertex.Relations.All(r => r.ContainsMultipleRelations));
                Assert.IsTrue(parentVertex.Relations.All(r => r.Target == childVertex));
                Assert.AreEqual(1, parentVertex.Relations.Count(r => r.State == EntityState.Deleted));
                Assert.AreEqual(2, parentVertex.Relations.SelectMany(r => r.Relations).Count(r => r.State == EntityState.Deleted));

                Assert.AreEqual(0, childVertex.Relations.Count());
            }
        }

        [TestMethod]
        public void ShouldSeeAddedAndUnchangedParallelRelations()
        {
            using (var context = new TestDbContext())
            {
                var child = new EntityWithChild();
                context.EntitiesWithChild.Add(child);
                var parent = new EntityWithChild();
                context.EntitiesWithChild.Add(parent);
                context.SaveChanges();

                parent.FavoriteChild = child;
                context.SaveChanges();

                parent.Children.Add(child);

                var vertices = context.GetEntityVertices();

                Assert.AreEqual(2, vertices.Count);
                var parentVertex = GetVertexByIdProperty(vertices, parent.Id);
                var childVertex = GetVertexByIdProperty(vertices, child.Id);

                Assert.IsTrue(parentVertex.Relations.All(r => r.ContainsMultipleRelations));
                Assert.IsTrue(parentVertex.Relations.All(r => r.Target == childVertex));
                Assert.AreEqual(1, parentVertex.Relations.Count(r => r.State == EntityState.Added));
                Assert.AreEqual(1, parentVertex.Relations.SelectMany(r => r.Relations).Count(r => r.State == EntityState.Added));
                Assert.AreEqual(1, parentVertex.Relations.SelectMany(r => r.Relations).Count(r => r.State == EntityState.Unchanged));

                Assert.AreEqual(0, childVertex.Relations.Count());
            }
        }

        [TestMethod]
        public void ShouldSeeAddedAndRemovedParallelRelations()
        {
            using (var context = new TestDbContext())
            {
                var child = new EntityWithChild();
                context.EntitiesWithChild.Add(child);
                var parent = new EntityWithChild();
                context.EntitiesWithChild.Add(parent);
                context.SaveChanges();

                parent.FavoriteChild = child;
                context.SaveChanges();

                parent.Children.Add(child);
                parent.FavoriteChild = null;

                var vertices = context.GetEntityVertices();

                Assert.AreEqual(2, vertices.Count);
                var parentVertex = GetVertexByIdProperty(vertices, parent.Id);
                var childVertex = GetVertexByIdProperty(vertices, child.Id);

                Assert.IsTrue(parentVertex.Relations.All(r => r.ContainsMultipleRelations));
                Assert.IsTrue(parentVertex.Relations.All(r => r.Target == childVertex));
                Assert.AreEqual(1, parentVertex.Relations.Count(r => r.State == EntityState.Deleted));
                Assert.AreEqual(1, parentVertex.Relations.SelectMany(r => r.Relations).Count(r => r.State == EntityState.Added));
                Assert.AreEqual(1, parentVertex.Relations.SelectMany(r => r.Relations).Count(r => r.State == EntityState.Deleted));

                Assert.AreEqual(0, childVertex.Relations.Count());
            }
        }

        [TestMethod]
        public void ShouldSeeUnchangedAndRemovedParallelRelations()
        {
            using (var context = new TestDbContext())
            {
                var child = new EntityWithChild();
                context.EntitiesWithChild.Add(child);
                var parent = new EntityWithChild();
                context.EntitiesWithChild.Add(parent);
                context.SaveChanges();

                parent.FavoriteChild = child;
                parent.Children.Add(child);
                context.SaveChanges();

                parent.Children.Remove(child);

                var vertices = context.GetEntityVertices();

                Assert.AreEqual(2, vertices.Count);
                var parentVertex = GetVertexByIdProperty(vertices, parent.Id);
                var childVertex = GetVertexByIdProperty(vertices, child.Id);

                Assert.IsTrue(parentVertex.Relations.All(r => r.ContainsMultipleRelations));
                Assert.IsTrue(parentVertex.Relations.All(r => r.Target == childVertex));
                Assert.AreEqual(1, parentVertex.Relations.Count(r => r.State == EntityState.Deleted));
                Assert.AreEqual(1, parentVertex.Relations.SelectMany(r => r.Relations).Count(r => r.State == EntityState.Unchanged));
                Assert.AreEqual(1, parentVertex.Relations.SelectMany(r => r.Relations).Count(r => r.State == EntityState.Deleted));

                Assert.AreEqual(0, childVertex.Relations.Count());
            }
        }
    }
}