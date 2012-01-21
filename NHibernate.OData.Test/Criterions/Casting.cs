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
    internal class Casting : DomainTestFixture
    {
        [Test]
        public void Tests()
        {
            // The only thing we do with casting is that we round when we're
            // casting to a whole number.

            Verify<Parent>("cast(Int32 div 2, 'Edm.Int32') eq 3", q => q.Where(p => p.Int32 == 6 || p.Int32 == 7));
            Verify<Parent>("cast(Int32 div 2, 'Edm.Int64') eq 3", q => q.Where(p => p.Int32 == 6 || p.Int32 == 7));
            Verify<Parent>("cast(Int32 div 2.0, 'Edm.Double') eq 2.5", q => q.Where(p => p.Int32 == 5));
            Verify<Parent>("cast(Name, 'Edm.String') eq 'Parent 1'", q => q.Where(p => p.Name == "Parent 1"));
        }
    }
}
