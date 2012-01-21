using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;
using NHibernate.OData.Test.Domain;

namespace NHibernate.OData.Test.Criterions
{
    [TestFixture]
    internal class DateParts : DomainTestFixture
    {
        [Test]
        public void Tests()
        {
            Verify<Parent>("year(DateTime) eq 2001", q => q.Where(p => p.Int32 == 1));
            Verify<Parent>("month(DateTime) eq 1", q => q.Where(p => p.Int32 == 1));
            Verify<Parent>("day(DateTime) eq 1", q => q.Where(p => p.Int32 == 1));
            Verify<Parent>("hour(DateTime) eq 1", q => q.Where(p => p.Int32 == 1));
            Verify<Parent>("minute(DateTime) eq 1", q => q.Where(p => p.Int32 == 1));
            Verify<Parent>("second(DateTime) eq 1", q => q.Where(p => p.Int32 == 1));
        }
    }
}
