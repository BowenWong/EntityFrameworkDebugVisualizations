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

#warning test tooltip text
#warning think about the relation state code and what's to test there!
    }
}