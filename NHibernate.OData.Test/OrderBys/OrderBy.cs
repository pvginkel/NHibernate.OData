using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Domain;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.OrderBys
{
    [TestFixture]
    internal class OrderBy : DomainTestFixture
    {
        [Test]
        public void WithoutDirection()
        {
            VerifyOrdered<Parent>("$orderby=Name", q => q.OrderBy(p => p.Name).Asc);
        }

        [Test]
        public void Ascending()
        {
            VerifyOrdered<Parent>("$orderby=Name asc", q => q.OrderBy(p => p.Name).Asc);
        }

        [Test]
        public void Descending()
        {
            VerifyOrdered<Parent>("$orderby=Name desc", q => q.OrderBy(p => p.Name).Desc);
        }

        [Test]
        public void DescendingThenAscending()
        {
            VerifyOrdered<Parent>("$orderby=Name desc, Int32 asc", q => q.OrderBy(p => p.Name).Desc.OrderBy(p => p.Int32).Asc);
        }

        [Test]
        public void AscendingThenDescending()
        {
            VerifyOrdered<Parent>("$orderby=Name asc, Int32 desc", q => q.OrderBy(p => p.Name).Asc.OrderBy(p => p.Int32).Desc);
        }

        [Test]
        public void TwoWithoutDirection()
        {
            VerifyOrdered<Parent>("$orderby=Name, Int32", q => q.OrderBy(p => p.Name).Asc.OrderBy(p => p.Int32).Asc);
        }

        [Test]
        public void CannotParse()
        {
            VerifyThrows<Parent>("$orderby=Name asc desc", typeof(ODataException));
        }
    }
}
