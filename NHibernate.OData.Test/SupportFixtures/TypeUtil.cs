using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NHibernate.OData.Test.SupportFixtures
{
    [TestFixture]
    internal class TypeUtil
    {
        [Test]
        public void CollectionTypes()
        {
            Assert.AreEqual(typeof(int), OData.TypeUtil.TryGetCollectionItemType(typeof(List<int>)));
            Assert.AreEqual(typeof(int), OData.TypeUtil.TryGetCollectionItemType(typeof(ISet<int>)));
            Assert.AreEqual(null, OData.TypeUtil.TryGetCollectionItemType(typeof(IEnumerable)));
        }
    }
}
