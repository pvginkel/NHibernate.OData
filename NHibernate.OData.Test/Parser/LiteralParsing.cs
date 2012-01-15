using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Parser
{
    [TestFixture]
    internal class LiteralParsing : ParserTestFixture
    {
        [Test]
        public void Bool()
        {
            VerifyBool("true", TrueLiteral);
            VerifyBool("false", FalseLiteral);
            VerifyBool("0", FalseLiteral);
            VerifyBool("1", TrueLiteral);
            VerifyBool("(1)", new ParenExpression(TrueLiteral));
        }
    }
}
