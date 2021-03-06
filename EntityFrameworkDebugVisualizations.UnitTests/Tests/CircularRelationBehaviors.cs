﻿using System.Collections.Generic;
using System.Linq;
using EntityFramework.Debug.UnitTests.Infrastructure;
using EntityFramework.Debug.UnitTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Debug.UnitTests.Tests
{
    [TestClass]
    public class CircularRelationBehaviors : Testbase
    {
        [TestMethod]
        public void ShouldSeeCircularRelationWithItself()
        {
            using (var context = new TestDbContext())
            {
                var owner = new OwnerOwned();
                context.OwnerOwneds.Add(owner);

                context.SaveChanges();

                owner.Owned = owner;

                var vertices = context.GetEntityVertices();
                Assert.AreEqual(1, vertices.Count(v => v.EntityType.Name == typeof(OwnerOwned).Name));
                Assert.IsTrue(vertices.All(v => v.Relations.Count == 1));
                Assert.IsTrue(vertices.All(v => v.Relations.All(r => r.Relations.Count == 2)));
            }
        }

        [TestMethod]
        public void ShouldSeeOwnerOwnedWithCircularRelation()
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
        public void ShouldSeeCircularRelationshipInCollection()
        {
            using (var context = new TestDbContext())
            {
                var owner = new OwnerOwnedCollection();
                context.OwnerOwnedCollections.Add(owner);

                var owned = new OwnerOwnedCollection();
                context.OwnerOwnedCollections.Add(owned);

                context.SaveChanges();

                owner.OwnedChildren = new List<OwnerOwnedCollection> { owned };
                owned.Owner = owner;

                var vertices = context.GetEntityVertices();
                Assert.AreEqual(2, vertices.Count(v => v.EntityType.Name == typeof(OwnerOwnedCollection).Name));
                Assert.IsTrue(vertices.All(v => v.Relations.Count == 1));
            }
        }

        [TestMethod]
        public void ShouldSeeMultiLevelOwnerOwnedCircularRelation()
        {
            using (var context = new TestDbContext())
            {
                var owner1 = new OwnerOwned();
                context.OwnerOwneds.Add(owner1);

                var owner2 = new OwnerOwned();
                context.OwnerOwneds.Add(owner2);

                var owner3 = new OwnerOwned();
                context.OwnerOwneds.Add(owner3);

                owner1.Owned = owner2;
                owner2.Owned = owner3;
                owner3.Owned = owner1;

                var vertices = context.GetEntityVertices();

                Assert.AreEqual(3, vertices.Count(v => v.EntityType.Name == typeof(OwnerOwned).Name));
                Assert.IsTrue(vertices.All(v => v.Relations.Count == 2));
            }
        }
    }
}