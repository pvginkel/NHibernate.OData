using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Domain;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Issues
{
    [TestFixture]
    internal class Issue24Fixture : DomainTestFixture
    {
        [Test]
        public void CountWithoutFilter()
        {
            var actual = Session.ODataQuery<Parent>("$count=true").List();

            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(11, actual[0]);
        }

        [Test]
        public void CountWithFilter()
        {
            var actual = Session.ODataQuery<Parent>("$filter=Id eq 1&$count=true").List();

            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(1, actual[0]);
        }

        [Test]
        public void CountIgnoresTopSkip()
        {
            var actual = Session.ODataQuery<Parent>("$count=true&$top=1&$skip=1").List();

            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(11, actual[0]);
        }
    }
}
