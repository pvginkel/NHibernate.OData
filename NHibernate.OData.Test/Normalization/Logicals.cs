using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Normalization
{
    [TestFixture]
    internal class Logicals : NormalizedTestFixture
    {
        [Test]
        public void And()
        {
            Verify("true and true", true);
            Verify("true and false", false);
            VerifyThrows("1 and 2");
        }

        [Test]
        public void Or()
        {
            Verify("true or true", true);
            Verify("true or false", true);
            Verify("false or false", false);
            VerifyThrows("1 or 2");
        }

        [Test]
        public void Not()
        {
            Verify("not true", false);
            Verify("not false", true);
            VerifyThrows("not 'a'");
        }

        [Test]
        public void Unchanged()
        {
            Verify(
                "not A",
                new BoolUnaryExpression(
                    Operator.Not,
                    new ResolvedMemberExpression(MemberType.Boolean, "A", null)
                )
            );
            Verify(
                "A or B",
                new LogicalExpression(
                    Operator.Or,
                    new ResolvedMemberExpression(MemberType.Normal, "A", null),
                    new ResolvedMemberExpression(MemberType.Normal, "B", null)
                )
            );
        }
    }
}
