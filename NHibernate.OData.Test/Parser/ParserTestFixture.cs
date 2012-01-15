using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NHibernate.OData.Test.Parser
{
    internal abstract class ParserTestFixture
    {
        protected static readonly LiteralExpression TrueLiteral = new LiteralExpression(true);
        protected static readonly LiteralExpression FalseLiteral = new LiteralExpression(false);
        protected static readonly LiteralExpression ZeroLiteral = new LiteralExpression(0);
        protected static readonly LiteralExpression OneLiteral = new LiteralExpression(1);

        protected void Verify(string source, Expression expression)
        {
            Verify(new Parser(source).Parse(), expression);
        }

        protected void VerifyBool(string source, Expression expression)
        {
            Verify(new BoolParser(source).Parse(), expression);
        }

        private void Verify(Expression actual, Expression expected)
        {
            Assert.AreEqual(expected, actual);
        }

        protected void VerifyThrows(string source)
        {
            VerifyThrows(source, typeof(ODataException));
        }

        protected void VerifyThrows(string source, System.Type exceptionType)
        {
            try
            {
                var result = new Parser(source).Parse();

                Assert.Fail("Expected exception");
            }
            catch (AssertionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Assert.AreEqual(exceptionType, ex.GetType());
            }
        }

        private class Parser : OData.Parser
        {
            public Parser(string source)
                : base(source)
            {
            }

            public override Expression Parse()
            {
                var result = ParseCommon();

                ExpectAtEnd();

                return result;
            }
        }

        private class BoolParser : OData.Parser
        {
            public BoolParser(string source)
                : base(source)
            {
            }

            public override Expression Parse()
            {
                var result = ParseBool();

                ExpectAtEnd();

                return result;
            }
        }
    }
}
