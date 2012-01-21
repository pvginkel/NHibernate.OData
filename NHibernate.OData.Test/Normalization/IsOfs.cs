using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Normalization
{
    [TestFixture]
    internal class IsOfs : NormalizedTestFixture
    {
        [Test]
        public void Tests()
        {
            Verify("isof(1, 'Edm.Int32')", true);
            Verify("isof(1, 'Edm.Int64')", false);
            Verify("isof(null, 'Edm.Int64')", false);
            VerifyThrows("isof(null, 1)");
        }

        [Test]
        public void Unchanged()
        {
            Verify(
                "isof(A, 'Edm.Int32')",
                new MethodCallExpression(
                    MethodCallType.Boolean,
                    Method.IsOfMethod,
                    new ResolvedMemberExpression(MemberType.Normal, "A"),
                    new LiteralExpression("Edm.Int32")
                )
            );
        }
    }
}
