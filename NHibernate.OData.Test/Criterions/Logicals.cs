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
    internal class Logicals : DomainTestFixture
    {
        [Test]
        public void And()
        {
            Verify<Parent>("Int32 gt 3 and Int32 lt 6", q => q.Where(p => p.Int32 > 3 && p.Int32 < 6));
        }

        [Test]
        public void Or()
        {
            Verify<Parent>("Int32 lt 3 or Int32 gt 6", q => q.Where(p => p.Int32 < 3 || p.Int32 > 6));
        }

        [Test]
        public void Not()
        {
            Verify<Parent>("not (Int32 gt 3)", q => q.Where(p => !(p.Int32 > 3)));
        }

        [Test]
        public void NullRight()
        {
            Verify<Parent>("Child eq null", q => q.Where(p => p.Child == null));
        }

        [Test]
        public void NullLeft()
        {
            Verify<Parent>("null eq Child", q => q.Where(p => p.Child == null));
        }

        [Test]
        [ExpectedException]
        public void NullIllegalOperator()
        {
            Verify<Parent>("null gt Child", q => q.Where(p => p.Child == null));
        }

        [Test]
        [ExpectedException]
        public void NullEqNull()
        {
            Verify<Parent>("null eq null", q => q);
        }
    }
}
