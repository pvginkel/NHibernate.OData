using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Domain;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Pagings
{
    [TestFixture]
    internal class Paging : DomainTestFixture
    {
        [Test]
        public void Tests()
        {
            Verify<Parent>("$skip=5", q => q.Where(p => p.Int32 > 5));
            Verify<Parent>("$top=5", q => q.Where(p => p.Int32 <= 5));
            Verify<Parent>("$top=3&$skip=3", q => q.Where(p => p.Int32 > 3 && p.Int32 <= 6));
            Verify<Parent>("$filter=Int32 gt 6&$top=3", q => q.Where(p => p.Int32 > 6 && p.Int32 <= 9));
        }

        [Test]
        public void SkipNotNumber()
        {
            VerifyThrows<Parent>("$skip=foo", typeof(ODataException));
        }

        [Test]
        public void SkipNegative()
        {
            VerifyThrows<Parent>("$skip=-1", typeof(ODataException));
        }
    }
}
