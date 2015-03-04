using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Inverse
{
    [TestFixture]
    internal class Basics : InverseTestFixture
    {
        [Test]
        public void Not()
        {
            Verify(
                "not A",
                new MemberExpression(MemberType.Boolean, "A")
            );

            Verify(
                "not(A and B)",
                new ParenExpression(
                    new LogicalExpression(
                        Operator.And,
                        new MemberExpression(MemberType.Boolean, "A"), 
                        new MemberExpression(MemberType.Boolean, "B")
                    )
                )
            );
        }

        [Test]
        public void Comparison()
        {
            Verify("A gt 2", new ComparisonExpression(Operator.Le, new MemberExpression(MemberType.Normal, "A"), new LiteralExpression(2)));
            Verify("A ge 2", new ComparisonExpression(Operator.Lt, new MemberExpression(MemberType.Normal, "A"), new LiteralExpression(2)));
            Verify("A lt 2", new ComparisonExpression(Operator.Ge, new MemberExpression(MemberType.Normal, "A"), new LiteralExpression(2)));
            Verify("A le 2", new ComparisonExpression(Operator.Gt, new MemberExpression(MemberType.Normal, "A"), new LiteralExpression(2)));
        }

        [Test]
        public void BoolMembersAnd()
        {
            Verify(
                "A and B",
                new LogicalExpression(
                    Operator.Or,
                    new BoolUnaryExpression(Operator.Not, new MemberExpression(MemberType.Boolean, "A")),
                    new BoolUnaryExpression(Operator.Not, new MemberExpression(MemberType.Boolean, "B"))
                )
            );
        }

        [Test]
        public void And()
        {
            Verify(
                "A gt 2 and B",
                new LogicalExpression(
                    Operator.Or,
                    new ComparisonExpression(
                        Operator.Le,
                        new MemberExpression(MemberType.Normal, "A"),
                        new LiteralExpression(2)
                    ), 
                    new BoolUnaryExpression(
                        Operator.Not, 
                        new MemberExpression(MemberType.Normal, "B")
                    )
                )
            );
        }

        [Test]
        public void ComplexLogic()
        {
            Verify(
                "A and B or not C",
                new LogicalExpression(
                    Operator.And, 
                    new LogicalExpression(
                        Operator.Or,
                        new BoolUnaryExpression(
                            Operator.Not,
                            new MemberExpression(MemberType.Normal, "A")
                        ), 
                        new BoolUnaryExpression(
                            Operator.Not,
                            new MemberExpression(MemberType.Normal, "B")
                        )
                    ),
                    new MemberExpression(MemberType.Normal, "C")
                )
            );
        }

        [Test]
        public void MethodCall()
        {
            Verify(
                "substringof('Test', A)",
                new BoolUnaryExpression(
                    Operator.Not,
                    new MethodCallExpression(
                        MethodCallType.Boolean,
                        Method.SubStringOfMethod,
                        new LiteralExpression("Test"),
                        new MemberExpression(MemberType.Normal, "A")
                    )
                )
            );
        }
    }
}
