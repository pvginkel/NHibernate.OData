using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NHibernate.OData.Test.Parser
{
    [TestFixture]
    internal class Comparison : ParserTestFixture
    {
        [Test]
        public void Equals()
        {
            Verify(
                "1 eq 1",
                new BoolExpression(KeywordType.Eq, OneLiteral, OneLiteral)
            );
        }
    }
}
