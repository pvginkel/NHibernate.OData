using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Domain;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Interface
{
    [TestFixture]
    internal class IllegalQueryString : DomainTestFixture
    {
        [Test]
        public void UnknownPart()
        {
            VerifyThrows<Parent>("$foo", typeof(ODataException));
        }
    }
}
