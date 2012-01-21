using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Criterion;
using NHibernate.OData.Test.Domain;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Criterions
{
    [TestFixture]
    internal class Comparisons : DomainTestFixture
    {
        [Test]
        public void Eq()
        {
            Verify<Parent>("Name eq 'Parent 1'", q => q.Where(p => p.Name == "Parent 1"));
            Verify<Parent>("Int32 eq 1", q => q.Where(p => p.Int32 == 1));
            Verify<Parent>("'Parent 1' eq Name", q => q.Where(p => p.Name == "Parent 1"));
        }

        [Test]
        public void Ne()
        {
            Verify<Parent>("Name ne 'Parent 1'", q => q.Where(p => p.Name != "Parent 1"));
            Verify<Parent>("Int32 ne 1", q => q.Where(p => p.Int32 != 1));
        }

        [Test]
        public void Gt()
        {
            Verify<Parent>("Int32 gt 5", q => q.Where(p => p.Int32 > 5));
            Verify<Parent>("Name gt 'Parent 5'", Restrictions.Gt("Name", "Parent 5"));
        }

        [Test]
        public void Ge()
        {
            Verify<Parent>("Int32 ge 5", q => q.Where(p => p.Int32 >= 5));
            Verify<Parent>("Name ge 'Parent 5'", Restrictions.Ge("Name", "Parent 5"));
        }

        [Test]
        public void Lt()
        {
            Verify<Parent>("Int32 lt 5", q => q.Where(p => p.Int32 < 5));
            Verify<Parent>("Name lt 'Parent 5'", Restrictions.Lt("Name", "Parent 5"));
        }

        [Test]
        public void Le()
        {
            Verify<Parent>("Int32 le 5", q => q.Where(p => p.Int32 <= 5));
            Verify<Parent>("Name le 'Parent 5'", Restrictions.Le("Name", "Parent 5"));
        }
    }
}
