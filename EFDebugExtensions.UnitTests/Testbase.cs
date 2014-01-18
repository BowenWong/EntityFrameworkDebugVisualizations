using System;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Debug.UnitTests
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
    }
}