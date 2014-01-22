using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using EntityFramework.Debug.DebugVisualization.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Debug.UnitTests.Infrastructure
{
    public class Testbase
    {
        private TransactionScope _transactionScope;

        [TestInitialize]
        public void CreateTransactionOnTestInitialize()
        {
            _transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { Timeout = new TimeSpan(0, 10, 0) });
        }

        [TestCleanup]
        public void DisposeTransactionOnTestCleanup()
        {
            Transaction.Current.Rollback();
            _transactionScope.Dispose();
        }

        protected static EntityVertex GetVertexByIdProperty(IEnumerable<EntityVertex> vertices, int id)
        {
            return vertices.Single(v => (int?)GetPropertyValueByName(v, "Id") == id);
        }

        protected static object GetPropertyValueByName(EntityVertex vertex, string propertyName)
        {
            var property = vertex.Properties.Single(p => p.Name == propertyName);
            return property.EntityState == EntityState.Deleted ? property.OriginalValue : property.CurrentValue;
        }
    }
}