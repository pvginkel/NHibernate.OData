using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NHibernate.OData.Test.Parser
{
    [TestFixture]
    internal class MethodCalls : ParserTestFixture
    {
        [Test]
        public void BooleanCast()
        {
            VerifyBool(
                "cast(1, 'Edm.Boolean')",
                new MethodCallExpression(
                    MethodCallType.Boolean,
                    Method.CastMethod,
                    OneLiteral,
                    new LiteralExpression("Edm.Boolean")
                )
            );

            // Throws because not requires a bool expression and the cast
            // isn't a bool expression.

            VerifyThrows("not cast(1, 'Edm.Double')");
        }

        [Test]
        public void ArgumentCount()
        {
            Verify(
                "toupper('text')",
                new MethodCallExpression(
                    MethodCallType.Normal,
                    Method.ToUpperMethod,
                    new LiteralExpression("text")
                )
            );

            VerifyThrows("toupper()");
            VerifyThrows("toupper(1, 2)");
        }

        [Test]
        public void OptionalArgumentCount()
        {
            Verify(
                "concat('a')",
                new MethodCallExpression(
                    MethodCallType.Normal,
                    Method.ConcatMethod,
                    new LiteralExpression("a")
                )
            );

            Verify(
                "concat('a', 'b')",
                new MethodCallExpression(
                    MethodCallType.Normal,
                    Method.ConcatMethod,
                    new LiteralExpression("a"),
                    new LiteralExpression("b")
                )
            );

            VerifyThrows("concat()");
            VerifyThrows("concat('a', 'b', 'c')");
        }
    }
}
