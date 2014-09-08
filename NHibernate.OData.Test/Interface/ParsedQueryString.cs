using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Interface
{
    [TestFixture]
    internal class ParsedQueryString : DomainTestFixture
    {
        [Test]
        public void Tests()
        {
            Assert.AreEqual(
                Session.ODataQuery("Parent", "$filter=Name eq 'Parent 1'").List(),
                Session.ODataQuery("Parent", new[] { new KeyValuePair<string, string>("$filter", "Name eq 'Parent 1'") }).List()
            );
        }
    }
}
