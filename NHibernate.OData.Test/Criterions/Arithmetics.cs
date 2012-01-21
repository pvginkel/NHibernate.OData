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
    internal class Arithmetics : DomainTestFixture
    {
        [Test]
        public void Add()
        {
            Verify<Parent>("Int32 add 5 lt 10", q => q.Where(p => p.Int32 < 5));
        }

        [Test]
        public void Sub()
        {
            Verify<Parent>("Int32 sub 5 lt 0", q => q.Where(p => p.Int32 < 5));
        }

        [Test]
        public void Mul()
        {
            Verify<Parent>("Int32 mul 2 gt 10", q => q.Where(p => p.Int32 > 5));
        }

        [Test]
        public void Div()
        {
            Verify<Parent>("Int32 div 2 gt 3", q => q.Where(p => p.Int32 >= 8));
        }

        [Test]
        public void Mod()
        {
            Verify<Parent>("Int32 mod 3 eq 1", q => q.Where(p => p.Int32 == 1 || p.Int32 == 4 || p.Int32 == 7 || p.Int32 == 10));
        }

        [Test]
        public void Negative()
        {
            Verify<Parent>("-Int32 eq -5", q => q.Where(p => p.Int32 == 5));
        }

        [Test]
        [ExpectedException(typeof(HibernateException))] // SQLite doesn't support ceil
        public void Ceiling()
        {
            Verify<Parent>("ceiling(Int32 div 2) eq 3", q => q.Where(p => p.Int32 == 5 || p.Int32 == 6));
        }

        [Test]
        [ExpectedException(typeof(HibernateException))] // SQLite doesn't support floor
        public void Floor()
        {
            Verify<Parent>("floor(Int32 div 2) eq 3", q => q.Where(p => p.Int32 == 4 || p.Int32 == 5));
        }

        [Test]
        public void Round()
        {
            Verify<Parent>("round(Int32 div 2) eq 3", q => q.Where(p => p.Int32 == 6 || p.Int32 == 7));
        }
    }
}
