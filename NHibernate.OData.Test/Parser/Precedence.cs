using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                new ArithmicExpression(
                    Operator.Mul,
                    new LiteralExpression(1),
                    new ArithmicExpression(
                        Operator.Add,
                        new LiteralExpression(2),
                        new LiteralExpression(3)
                    )
                )
            );
        }

        [Test]
        public void MulAndAddWithParens()
        {
            Verify(
                "(1 mul 2) add 3",
                new ArithmicExpression(
                    Operator.Add,
                    new ParenExpression(
                        new ArithmicExpression(
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
                    new ArithmicUnaryExpression(Operator.Negative, ZeroLiteral),
                    OneLiteral
                )
            );
        }
    }
}
