using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NHibernate.OData.Test.Parser
{
    [TestFixture]
    internal class Singles : ParserTestFixture
    {
        [Test]
        public void Not()
        {
            Verify("not true", new BoolSingleExpression(KeywordType.Not, TrueLiteral));
        }
    }
}
