using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Parser
{
    [TestFixture]
    internal class Precedence : ParserTestFixture
    {
        [Test]
        public void MulAndAdd()
        {
            Verify(
                "1 mul 2 add 3",
                new ArithmeticExpression(
                    Operator.Add,
                    new ArithmeticExpression(
                        Operator.Mul,
                        new LiteralExpression(1),
                        new LiteralExpression(2)
                    ),
                    new LiteralExpression(3)
                )
            );
        }

        [Test]
        public void MulAndAddWithParens()
        {
            Verify(
                "(1 mul 2) add 3",
                new ArithmeticExpression(
                    Operator.Add,
                    new ParenExpression(
                        new ArithmeticExpression(
                            Operator.Mul,
                            new LiteralExpression(1),
                            new LiteralExpression(2)
                        )
                    ),
                    new LiteralExpression(3)
                )
            );
        }

        [Test]
        public void UnaryNot()
        {
            Verify(
                "not true eq false",
                new ComparisonExpression(
                    Operator.Eq,
                    new BoolUnaryExpression(Operator.Not, TrueLiteral),
                    FalseLiteral
                )
            );
        }

        [Test]
        public void UnaryNegative()
        {
            Verify(
                "- 0 eq 1",
                new ComparisonExpression(
                    Operator.Eq,
                    new ArithmeticUnaryExpression(Operator.Negative, ZeroLiteral),
                    OneLiteral
                )
            );
        }

        [Test]
        public void AndAndComparison()
        {
            Verify(
                "1 gt 2 and 3 gt 4",
                new LogicalExpression(
                    Operator.And,
                    new ComparisonExpression(
                        Operator.Gt,
                        new LiteralExpression(1),
                        new LiteralExpression(2)
                    ),
                    new ComparisonExpression(
                        Operator.Gt,
                        new LiteralExpression(3),
                        new LiteralExpression(4)
                    )
                )
            );
        }
    }
}
