using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Normalization
{
    [TestFixture]
    internal class Casting : NormalizedTestFixture
    {
        [Test]
        public void Tests()
        {
            Verify("cast(true, 'Edm.Boolean')", true);
            Verify("cast(1, 'Edm.Int32')", 1);
            Verify("cast(1.1, 'Edm.Double')", 1.1);
            Verify("cast(1.1f, 'Edm.Double')", (double)1.1f);
            Verify("cast(1, 'Edm.Double')", 1.0);
            Verify("cast(1.1, 'Edm.Int32')", 1);
            Verify("cast(1, 'Edm.String')", "1");
            Verify("cast(null, 'Edm.Int32')", null);
            VerifyThrows("cast(X'00', 'Edm.Int32')");
            VerifyThrows("cast(1, 'IllegalEdmType')");
        }
    }
}
