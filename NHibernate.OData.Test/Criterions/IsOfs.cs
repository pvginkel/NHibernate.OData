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
    internal class IsOfs : DomainTestFixture
    {
        [Test]
        public void Tests()
        {
            // Metadata access is not supported; isof just throws.

            VerifyThrows<Parent>("isof(Int32, 'Edm.Int32')");
        }
    }
}
