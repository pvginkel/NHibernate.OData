using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Parser
{
    [TestFixture]
    internal class Members : ParserTestFixture
    {
        [Test]
        public void SimpleMember()
        {
            Verify(
                "A",
                new MemberExpression(MemberType.Normal, "A")
            );
        }

        [Test]
        public void MemberPath()
        {
            Verify(
                "A/B",
                new MemberExpression(MemberType.Normal, "A", "B")
            );
            Verify(
                "A / B",
                new MemberExpression(MemberType.Normal, "A", "B")
            );
        }

        [Test]
        public void BoolMember()
        {
            VerifyBool(
                "A",
                new MemberExpression(MemberType.Boolean, "A")
            );
        }

        [Test]
        public void MemberInComparison()
        {
            Verify(
                "A eq B",
                new ComparisonExpression(
                    Operator.Eq,
                    new MemberExpression(MemberType.Normal, "A"),
                    new MemberExpression(MemberType.Normal, "B")
                )
            );
        }

        [Test]
        public void MemberPathInComparison()
        {
            Verify(
                "A / B eq C / D",
                new ComparisonExpression(
                    Operator.Eq,
                    new MemberExpression(MemberType.Normal, "A", "B"),
                    new MemberExpression(MemberType.Normal, "C", "D")
                )
            );
        }

        [Test]
        public void IllegalMember()
        {
            VerifyThrows("A /");
        }
    }
}
