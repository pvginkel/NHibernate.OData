using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Parser
{
    [TestFixture]
    internal class CollectionMethodCalls : ParserTestFixture
    {      
        [Test]
        public void CollectionAny()
        {
            Verify(
                "Collection/any()",
                new MethodCallExpression(
                    MethodCallType.Boolean,
                    Method.AnyMethod,
                    new MemberExpression(MemberType.Normal, "Collection")
                )
            );
        }

        [Test]
        public void Parenthesis()
        {
            Verify(
                "(Collection/any())",
                new ParenExpression(
                    new MethodCallExpression(
                        MethodCallType.Boolean,
                        Method.AnyMethod,
                        new MemberExpression(MemberType.Normal, "Collection")
                    )
                )
            );            
        }

        [Test]
        public void CollectionAnyWithSimplePredicate()
        {
            Verify(
                "Collection/any(x : x/Bool)",
                new MethodCallExpression(
                    MethodCallType.Boolean,
                    Method.AnyMethod,
                    new MemberExpression(MemberType.Normal, "Collection"),
                    new LambdaExpression(
                        "x",
                        new MemberExpression(MemberType.Normal, "x", "Bool")
                    )
                )
            );
        }

        [Test]
        public void CollectionAnyWithNestedCollectionAll()
        {
            Verify(
                "Collection/any(x : x/OtherCollection/all(y:y/Bool))",
                new MethodCallExpression(
                    MethodCallType.Boolean,
                    Method.AnyMethod,
                    new MemberExpression(MemberType.Normal, "Collection"),
                    new LambdaExpression(
                        "x",
                        new MethodCallExpression(
                            MethodCallType.Boolean, 
                            Method.AllMethod,
                            new MemberExpression(MemberType.Normal, "x", "OtherCollection"),
                            new LambdaExpression(
                                "y",
                                new MemberExpression(MemberType.Normal, "y", "Bool")
                            )
                        )
                    )
                )
            );
        }

        [Test]
        public void CollectionAnyWithComplexPredicate()
        {
            Verify(
                "Collection/any(x: x/A eq 1 or x/B)",
                new MethodCallExpression(
                    MethodCallType.Boolean,
                    Method.AnyMethod,
                    new MemberExpression(MemberType.Normal, "Collection"),
                    new LambdaExpression(
                        "x",
                        new LogicalExpression(
                            Operator.Or,
                            new ComparisonExpression(Operator.Eq, new MemberExpression(MemberType.Normal, "x", "A"),  new LiteralExpression(1)), 
                            new MemberExpression(MemberType.Boolean, "x", "B")
                        )
                    )
                )
            );
        }

        [Test]
        public void CollectionAnyInsideLogicExpression()
        {
            Verify(
                "A and Collection/any() or B",
                new LogicalExpression(
                    Operator.Or, 
                    new LogicalExpression(
                        Operator.And,
                        new MemberExpression(MemberType.Boolean, "A"),
                        new MethodCallExpression(
                            MethodCallType.Boolean,
                            Method.AnyMethod,
                            new MemberExpression(MemberType.Normal, "Collection")
                        )
                    ),
                    new MemberExpression(MemberType.Boolean, "B")
                )
            );
        }

        [Test]
        public void InvalidLambda()
        {
            VerifyThrows("Collection/any(x)");
            VerifyThrows("Collection/any(x:)");
        }

        [Test]
        public void IllegalCalls()
        {
            VerifyThrows("any()");
            VerifyThrows("Collection/any");

            VerifyThrows("Collection/all()"); // An argument is required
        }
    }
}
