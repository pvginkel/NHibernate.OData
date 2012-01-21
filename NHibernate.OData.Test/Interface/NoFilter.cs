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
    internal class NoFilter : DomainTestFixture
    {
        [Test]
        public void Test()
        {
            Verify<Parent>("", q => q);
        }
    }
}
