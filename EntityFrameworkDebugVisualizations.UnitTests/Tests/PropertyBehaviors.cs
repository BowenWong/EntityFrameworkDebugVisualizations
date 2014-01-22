using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using EntityFramework.Debug.DebugVisualization.Graph;
using EntityFramework.Debug.UnitTests.Infrastructure;
using EntityFramework.Debug.UnitTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Debug.UnitTests.Tests
{
    [TestClass]
    public class PropertyBehaviors : Testbase
    {
        [TestMethod]
        public void ShouldSeeTwoKeyProperties()
        {
            using (var context = new TestDbContext())
            {
                const string key1 = "ABC1";
                const string key2 = "DEF2";
                var entity = new MultiKeyEntity {KeyPart1 = key1, KeyPart2 = key2};
                context.MultiKeyEntities.Add(entity);
                context.SaveChanges();

                List<EntityVertex> vertices = context.GetEntityVertices();
                Assert.AreEqual(1, vertices.Count(v => v.EntityType.Name == typeof (MultiKeyEntity).Name));

                EntityVertex entityVertex = vertices[0];
                Assert.AreEqual(2, entityVertex.Properties.Count(p => p.IsKey));
                Assert.IsFalse(entityVertex.HasTemporaryKey);

                Assert.IsTrue(entityVertex.KeyDescription.Contains(key1));
                Assert.IsTrue(entityVertex.KeyDescription.Contains(key2));
            }
        }

        [TestMethod]
        public void ShouldSeeOneConcurrencyProperty()
        {
            using (var context = new TestDbContext())
            {
                var owner = new EntityWithChild();
                context.EntitiesWithChild.Add(owner);

                var vertices = context.GetEntityVertices();

                Assert.AreEqual(1, vertices.Count);

                var concurrencyProperty = vertices[0].Properties.Single(p => p.IsConcurrencyProperty);
                
                Assert.AreEqual("RowVersion", concurrencyProperty.Name);
                Assert.IsFalse(concurrencyProperty.IsKey);
                Assert.IsFalse(concurrencyProperty.IsRelation);
            }
        }

        [TestMethod]
        public void ShouldSeeSingleKeyProperty()
        {
            using (var context = new TestDbContext())
            {
                var owner = new OwnerOwned { Name = "The Owner" };
                context.OwnerOwneds.Add(owner);
                context.SaveChanges();

                var vertices = context.GetEntityVertices();

                Assert.AreEqual(1, vertices.Count);
                Assert.AreEqual(EntityState.Unchanged, vertices[0].State);
                Assert.AreEqual(1, vertices[0].Properties.Count(p => p.IsKey));
                Assert.AreEqual("Id", vertices[0].Properties.Single(p => p.IsKey).Name);
            }
        }

        [TestMethod]
        public void ShouldSeeTwoEmptyRelationProperties()
        {
            using (var context = new TestDbContext())
            {
                var owner = new OwnerOwned { Name = "The Owner" };
                context.OwnerOwneds.Add(owner);
                context.SaveChanges();

                var vertices = context.GetEntityVertices();

                Assert.AreEqual(1, vertices.Count);
                Assert.AreEqual(EntityState.Unchanged, vertices[0].State);
                Assert.AreEqual(2, vertices[0].Properties.Count(p => p.IsRelation));
                Assert.IsTrue(vertices[0].Properties.Where(p => p.IsRelation).All(p => p.Name == "Owner" || p.Name == "Owned"));
                Assert.IsTrue(vertices[0].Properties.Where(p => p.IsRelation).All(p => p.CurrentValue == null));
            }
        }

        [TestMethod]
        public void ShouldSeeSetRelationProperty()
        {
            using (var context = new TestDbContext())
            {
                var owned = new OwnerOwned();
                var owner = new OwnerOwned { Name = "The Owner", Owned = owned};
                context.OwnerOwneds.Add(owner);
                context.SaveChanges();

                var vertices = context.GetEntityVertices();

                Assert.AreEqual(2, vertices.Count);

                var ownerVertex = GetVertexByIdProperty(vertices, owner.Id);
                Assert.AreEqual(2, ownerVertex.Properties.Count(p => p.IsRelation));
                Assert.AreEqual(1, ownerVertex.RelationProperties.Count(p => p.CurrentValue != null));

                var relationProperty = ownerVertex.RelationProperties.Single(p => p.CurrentValue != null);
                Assert.AreEqual(typeof(string), relationProperty.CurrentValue.GetType());
                Assert.IsTrue(((string)relationProperty.CurrentValue).Contains(owned.Id.ToString(CultureInfo.InvariantCulture)));
            }
        }

        [TestMethod]
        public void ShouldSeeCollectionPropertyWithTwoElements()
        {
            using (var context = new TestDbContext())
            {
                var ownedOne = new OwnerOwnedCollection();
                var ownedTwo = new OwnerOwnedCollection();
                var owner = new OwnerOwnedCollection {OwnedChildren = new List<OwnerOwnedCollection> {ownedOne, ownedTwo}};
                context.OwnerOwnedCollections.Add(owner);
                context.SaveChanges();

                var vertices = context.GetEntityVertices();

                Assert.AreEqual(3, vertices.Count);

                var ownerVertex = GetVertexByIdProperty(vertices, owner.Id);
                Assert.AreEqual(2, ownerVertex.Properties.Count(p => p.IsRelation));
                Assert.AreEqual(1, ownerVertex.RelationProperties.Count(p => p.CurrentValue != null));

                var relationProperty = ownerVertex.RelationProperties.Single(p => p.CurrentValue != null);
                Assert.AreEqual(typeof(string), relationProperty.CurrentValue.GetType());
                Assert.AreEqual("Collection [2 elements]", relationProperty.CurrentValue);
            }
        }

        [TestMethod]
        public void ShouldSeeOwnerWithUnchangedName()
        {
            using (var context = new TestDbContext())
            {
                var owner = new OwnerOwned { Name = "The Owner" };
                context.OwnerOwneds.Add(owner);
                context.SaveChanges();

                var vertices = context.GetEntityVertices();

                Assert.AreEqual(1, vertices.Count);
                Assert.AreEqual(EntityState.Unchanged, vertices[0].State);
                Assert.AreEqual(1, vertices[0].Properties.Count(p => !p.IsKey && !p.IsRelation && !p.IsConcurrencyProperty));
                Assert.IsFalse(vertices[0].Properties.Single(p => p.Name == "Name").HasValueChanged);
            }
        }

        [TestMethod]
        public void ShouldSeeOwnerWithChangedName()
        {
            using (var context = new TestDbContext())
            {
                var owner = new OwnerOwned { Name = "The Owner" };
                context.OwnerOwneds.Add(owner);
                context.SaveChanges();

                owner.Name = "The old Owner";

                var vertices = context.GetEntityVertices();

                Assert.AreEqual(1, vertices.Count);
                Assert.AreEqual(EntityState.Modified, vertices[0].State);
                Assert.AreEqual(1, vertices[0].Properties.Count(p => !p.IsKey && !p.IsRelation && !p.IsConcurrencyProperty));
                Assert.IsTrue(vertices[0].Properties.Single(p => p.Name == "Name").HasValueChanged);
            }
        }
    }
}