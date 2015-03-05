using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Domain;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Criterions
{
    [TestFixture]
    internal class ImplicitVariable : DomainTestFixture
    {
        [Test]
        public void SimpleQuery()
        {
            Verify("$filter=$it/Int32 eq 2", Session.QueryOver<Parent>().Where(x => x.Int32 == 2).List());
        }

        [Test]
        public void ThrowsOnEmptyMemberExpression()
        {
            VerifyThrows<ODataException>("$filter=$it");
        }
    }
}
