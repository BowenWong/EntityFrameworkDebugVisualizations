using System.Collections.Generic;
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

#warning check for relation properties: null relations (now), collection relations (List and Array), ..
#warning check descriptions
#warning check for (un-)changed properties
    }
}