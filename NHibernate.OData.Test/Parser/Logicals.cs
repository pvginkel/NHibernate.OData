using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Parser
{
    [TestFixture]
    internal class Logicals : ParserTestFixture
    {
        [Test]
        public void And()
        {
            Verify(
                "true and true",
                new LogicalExpression(Operator.And, TrueLiteral, TrueLiteral)
            );

            VerifyThrows("true and");
        }

        [Test]
        public void Or()
        {
            Verify(
                "true or true",
                new LogicalExpression(Operator.Or, TrueLiteral, TrueLiteral)
            );

            VerifyThrows("true or");
        }

        [Test]
        public void Nested()
        {
            Verify(
                "true and true or true",
                new LogicalExpression(
                    Operator.Or,
                    new LogicalExpression(Operator.And, TrueLiteral, TrueLiteral),
                    TrueLiteral
                )
            );
        }

        [Test]
        public void MultipleNested()
        {
            Verify(
                "true and true or true or true",
                new LogicalExpression(
                    Operator.Or,
                    new LogicalExpression(
                        Operator.And,
                        TrueLiteral,
                        TrueLiteral
                    ),
                    new LogicalExpression(
                        Operator.Or,
                        TrueLiteral,
                        TrueLiteral
                    )
                )
            );
        }

        [Test]
        public void NestedWithParensLeft()
        {
            Verify(
                "(true and true) or true",
                new LogicalExpression(
                    Operator.Or,
                    new ParenExpression(
                        new LogicalExpression(Operator.And, TrueLiteral, TrueLiteral)
                    ),
                    TrueLiteral
                )
            );
        }

        [Test]
        public void NestedWithParensRight()
        {
            Verify(
                "true or (true and true)",
                new LogicalExpression(
                    Operator.Or,
                    TrueLiteral,
                    new ParenExpression(
                        new LogicalExpression(Operator.And, TrueLiteral, TrueLiteral)
                    )
                )
            );
        }
    }
}
