using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EntityFramework.Debug.UnitTests.Infrastructure;
using EntityFramework.Debug.UnitTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Debug.UnitTests.Tests
{
    [TestClass]
    public class RelationBehaviors : Testbase
    {
        [TestMethod]
        public void ShouldSeeNewOwnerWithNewOwned()
        {
            using (var context = new TestDbContext())
            {
                var owner = new OwnerOwned();
                context.OwnerOwneds.Add(owner);

                owner.Owned = new OwnerOwned();

                var vertices = context.GetEntityVertices();
                Assert.AreEqual(2, vertices.Count(v => v.EntityType.Name == typeof(OwnerOwned).Name));
                Assert.IsTrue(vertices.All(v => v.Relations.Count == 1));
                Assert.IsTrue(vertices.All(v => v.State == EntityState.Added));
                Assert.IsTrue(vertices.SelectMany(v => v.Relations).All(r => r.State == EntityState.Added));
            }
        }

        [TestMethod]
        public void ShouldSeeUnchangedOwnerWithUnchangedOwned()
        {
            using (var context = new TestDbContext())
            {
                var owner = new OwnerOwned { Owned = new OwnerOwned() };
                context.OwnerOwneds.Add(owner);
                context.SaveChanges();

                var vertices = context.GetEntityVertices();
                Assert.AreEqual(2, vertices.Count(v => v.EntityType.Name == typeof(OwnerOwned).Name));
                Assert.IsTrue(vertices.All(v => v.Relations.Count == 1));
                Assert.IsTrue(vertices.All(v => v.State == EntityState.Unchanged));
                Assert.IsTrue(vertices.SelectMany(v => v.Relations).All(v => v.State == EntityState.Unchanged));
            }
        }

        [TestMethod]
        public void ShouldSeeUnchangedOwnerWithRemovedOwned()
        {
            using (var context = new TestDbContext())
            {
                var owned = new OwnerOwned();
                var owner = new OwnerOwned { Owned = owned };
                context.OwnerOwneds.Add(owner);
                context.SaveChanges();

                owner.Owned = null;

                var vertices = context.GetEntityVertices();

                Assert.AreEqual(2, vertices.Count(v => v.EntityType.Name == typeof(OwnerOwned).Name));
                Assert.IsTrue(vertices.All(v => v.State == EntityState.Unchanged));

                Assert.IsTrue(vertices.All(v => v.Relations.Count == 1));
                Assert.AreEqual(EntityState.Deleted, vertices.Single(v => (int)v.Properties.Single(p => p.Name == "Id").CurrentValue == owner.Id).Relations.Single().State);
                Assert.AreEqual(EntityState.Deleted, vertices.Single(v => (int)v.Properties.Single(p => p.Name == "Id").CurrentValue == owned.Id).Relations.Single().State);
            }
        }

        [TestMethod]
        public void ShouldSeeUnchangedCollectionWithNewOwned()
        {
            using (var context = new TestDbContext())
            {
                var ownedOne = new OwnerOwnedCollection();
                var owner = new OwnerOwnedCollection { OwnedChildren = new List<OwnerOwnedCollection> { ownedOne } };
                context.OwnerOwnedCollections.Add(owner);
                context.SaveChanges();

                var ownedTwo = new OwnerOwnedCollection();
                owner.OwnedChildren.Add(ownedTwo);

                var vertices = context.GetEntityVertices();

                Assert.AreEqual(3, vertices.Count(v => v.EntityType.Name == typeof(OwnerOwnedCollection).Name));
                Assert.IsTrue(vertices.All(v => v.Relations.Count > 0));

                var ownedOneVertex = GetVertexByIdProperty(vertices, ownedOne.Id);
                Assert.AreEqual(EntityState.Unchanged, ownedOneVertex.State);

                var ownedTwoVertex = GetVertexByIdProperty(vertices, ownedTwo.Id);
                Assert.AreEqual(EntityState.Added, ownedTwoVertex.State);

                var ownerRelationEdges = GetVertexByIdProperty(vertices, owner.Id).Relations;
                Assert.AreEqual(2, ownerRelationEdges.Count);
                Assert.AreEqual(EntityState.Unchanged, ownerRelationEdges[0].State);
                Assert.AreEqual(EntityState.Added, ownerRelationEdges[1].State);
            }
        }

        [TestMethod]
        public void ShouldSeeUnchangedCollectionWithoutRemovedOwned()
        {
            using (var context = new TestDbContext())
            {
                var ownedOne = new OwnerOwnedCollection();
                var ownedTwo = new OwnerOwnedCollection();
                var owner = new OwnerOwnedCollection { OwnedChildren = new List<OwnerOwnedCollection> { ownedOne, ownedTwo } };
                context.OwnerOwnedCollections.Add(owner);
                context.SaveChanges();
                
                owner.OwnedChildren.Remove(ownedTwo);

                var vertices = context.GetEntityVertices();

                Assert.AreEqual(3, vertices.Count(v => v.EntityType.Name == typeof(OwnerOwnedCollection).Name));
                Assert.IsTrue(vertices.All(v => v.State == EntityState.Unchanged));

                var ownerVertex = GetVertexByIdProperty(vertices, owner.Id);
                var ownedTwoVertex = GetVertexByIdProperty(vertices, ownedTwo.Id);
                Assert.AreEqual(1, ownedTwoVertex.Relations.Count);
                Assert.AreEqual(EntityState.Deleted, ownedTwoVertex.Relations[0].State);
                Assert.AreEqual(ownerVertex, ownedTwoVertex.Relations[0].Target);

                Assert.AreEqual(2, ownerVertex.Relations.Count);
                Assert.AreEqual(EntityState.Unchanged, ownerVertex.Relations[0].State);
                Assert.AreEqual(EntityState.Deleted, ownerVertex.Relations[1].State);
                Assert.AreEqual(ownedTwoVertex, ownerVertex.Relations[1].Target);
            }
        }

        [TestMethod]
        public void ShouldSeeEntityWithChildRemoved()
        {
            using (var context = new TestDbContext())
            {
                var parent = context.EntitiesWithChild.Add(new EntityWithChild {Name = "Parent"});
                var child = context.EntitiesWithChild.Add(new EntityWithChild {Name = "FavoriteChild"});
                parent.Children.Add(child);
                context.SaveChanges();

                parent.Children.Remove(child);

                var vertices = context.GetEntityVertices();

                Assert.AreEqual(2, vertices.Count);
                Assert.AreEqual(2, vertices.Count(v => v.State == EntityState.Unchanged));

                var parentVertex = GetVertexByIdProperty(vertices, parent.Id);
                Assert.AreEqual(1, parentVertex.Relations.Count);
                Assert.AreEqual(EntityState.Deleted, parentVertex.Relations[0].State);

                var childVertex = GetVertexByIdProperty(vertices, child.Id);
                Assert.AreEqual(0, childVertex.Relations.Count);
            }
        }

        [TestMethod]
        public void ShouldSeeEntityWithChildRemovedAndDeleted()
        {
            using (var context = new TestDbContext())
            {
                var parent = context.EntitiesWithChild.Add(new EntityWithChild { Name = "Parent" });
                var child = context.EntitiesWithChild.Add(new EntityWithChild { Name = "FavoriteChild" });
                parent.Children.Add(child);
                context.SaveChanges();

                parent.Children.Remove(child);
                context.EntitiesWithChild.Remove(child);

                var vertices = context.GetEntityVertices();

                Assert.AreEqual(2, vertices.Count);

                var parentVertex = GetVertexByIdProperty(vertices, parent.Id);
                Assert.AreEqual(EntityState.Unchanged, parentVertex.State);
                Assert.AreEqual(1, parentVertex.Relations.Count);
                Assert.AreEqual(EntityState.Deleted, parentVertex.Relations[0].State);

                var childVertex = GetVertexByIdProperty(vertices, child.Id);
                Assert.AreEqual(EntityState.Deleted, childVertex.State);
                Assert.AreEqual(0, childVertex.Relations.Count);
            }
        }

        [TestMethod]
        public void ShouldSeeChildsAndRelationsInAllStates()
        {
            using (var context = new TestDbContext())
            {
                var parent = context.EntitiesWithChild.Add(new EntityWithChild { Name = "Parent" });
                var child = context.EntitiesWithChild.Add(new EntityWithChild { Name = "FavoriteChild" });
                var secondChild = new EntityWithChild { Name = "2nd Child" };
                parent.Children.Add(secondChild);
                parent.Children.Add(new EntityWithChild { Name = "3nd Child" });
                parent.Children.Add(new EntityWithChild { Name = "4th Child" });
                context.SaveChanges();

                parent.FavoriteChild = child;
                parent.Children.Add(child);
                context.SaveChanges();

                var toDelete = context.EntitiesWithChild.Add(new EntityWithChild { Name = "Deleted" });
                context.SaveChanges();

                context.EntitiesWithChild.Remove(toDelete);
                parent.Children.Add(new EntityWithChild { Name = "1st addeded Child" });
                parent.Children.Add(new EntityWithChild { Name = "2nd addeded Child" });

                secondChild.Name = "Removed child";
                parent.Children.Remove(secondChild);

                var vertices = context.GetEntityVertices();

                var parentVertex = GetVertexByIdProperty(vertices, parent.Id);
                Assert.AreEqual(EntityState.Unchanged, parentVertex.State);
                Assert.AreEqual(7, parentVertex.Relations.Count);
                Assert.AreEqual(6, parentVertex.Relations.Count(r => r.Name == "Children"));

                // modified, deleted relation
                var modifiedVertex = GetVertexByIdProperty(vertices, secondChild.Id);
                Assert.AreEqual(EntityState.Modified, modifiedVertex.State);
                Assert.AreEqual(0, modifiedVertex.Relations.Count);
                Assert.AreEqual(1, parentVertex.Relations.Count(r => r.State == EntityState.Deleted));

                // deleted, deleted relation
                var removedChild = GetVertexByIdProperty(vertices, toDelete.Id);
                Assert.AreEqual(EntityState.Deleted, removedChild.State);
                Assert.AreEqual(0, removedChild.Relations.Count);
                
                // check that two childs are added (and their relations too)
                Assert.AreEqual(2, vertices.Count(v => v.State == EntityState.Added));
                Assert.AreEqual(2, vertices.Count(v => v.State == EntityState.Added && v.Relations.All(r => r.State == EntityState.Added)));
                Assert.AreEqual(2, parentVertex.Relations.Count(r => r.State == EntityState.Added));

                // check that three childs are unchanged (and their relations too)
                Assert.AreEqual(4, vertices.Count(v => v.State == EntityState.Unchanged));
                Assert.AreEqual(3, vertices.Count(v => v.State == EntityState.Unchanged && v.Relations.All(r => r.State == EntityState.Unchanged)));
                Assert.AreEqual(3, parentVertex.Relations.Count(r => r.Name == "Children" && r.State == EntityState.Unchanged));
            }
        }
    }
}