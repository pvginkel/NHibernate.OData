using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NHibernate.OData.Test.Parser
{
    [TestFixture]
    internal class LiteralParsing : ParserTestFixture
    {
        [Test]
        public void Bool()
        {
            Verify("true", TrueLiteral);
            Verify("false", FalseLiteral);
            Verify("0", FalseLiteral);
            Verify("1", TrueLiteral);
        }
    }
}
