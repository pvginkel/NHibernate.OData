using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Parser
{
    [TestFixture]
    internal class Parens : ParserTestFixture
    {
        [Test]
        public void BoolParen()
        {
            Verify("(true)", new ParenExpression(new LiteralExpression(true)));
        }

        [Test]
        public void NestedBoolParen()
        {
            Verify("((true))", new ParenExpression(new ParenExpression(new LiteralExpression(true))));
        }

        [Test]
        public void NoOpenParen()
        {
            VerifyThrows("true)");
        }

        [Test]
        public void NoCloseParen()
        {
            VerifyThrows("(true");
        }

        [Test]
        public void OnlyOpenParen()
        {
            VerifyThrows("(");
        }

        [Test]
        public void OnlyCloseParen()
        {
            VerifyThrows(")");
        }
    }
}
