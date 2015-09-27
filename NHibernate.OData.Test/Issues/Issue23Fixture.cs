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
    internal class Issue23Fixture : DomainTestFixture
    {
        [Test]
        public void InterfacePropertNotRecognizedAsCollection()
        {
            Verify(
                "EnumerableRelatedParents/any(o: o/Int32 eq 9)",
                Session.QueryOver<Parent>().Where(x => x.Name == "Parent 10").List()
            );
        }
    }
}
