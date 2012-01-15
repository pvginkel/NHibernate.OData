using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Parser
{
    [TestFixture]
    internal class Unaries : ParserTestFixture
    {
        [Test]
        public void Not()
        {
            Verify("not true", new BoolUnaryExpression(Operator.Not, TrueLiteral));
        }

        [Test]
        public void NoMethodCall()
        {
            Verify(
                "true and not (true)",
                new LogicalExpression(
                    Operator.And,
                    TrueLiteral,
                    new BoolUnaryExpression(
                        Operator.Not,
                        new ParenExpression(TrueLiteral)
                    )
                )
            );
        }

        [Test]
        public void IllegalUnary()
        {
            VerifyThrows("true not false");
        }
    }
}
