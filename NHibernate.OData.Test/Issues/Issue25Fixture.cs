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
    internal class Issue25Fixture : DomainTestFixture
    {
        [Test]
        public void FailingLogicalQuery()
        {
            Session.ODataQuery<Parent>("$filter=(substringof('21906522863', Name) and Id eq 55800)").List();
            Session.ODataQuery<Parent>("$filter=(DateTime eq datetime'1981-03-13T00:00:00' and substringof('Saurin', Name) and substringof('21906522863', Name) and Id eq 55800)").List();
        }
    }
}
