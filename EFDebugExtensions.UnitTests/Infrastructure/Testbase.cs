using System;
using System.Collections.Generic;
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
            return vertices.Single(v => (int) v.Properties.Single(p => p.Name == "Id").CurrentValue == id);
        }
    }
}