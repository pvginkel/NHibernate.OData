using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NHibernate.OData.Test.Parser
{
    [TestFixture]
    internal class Unaries : ParserTestFixture
    {
        [Test]
        public void Not()
        {
            Verify("not true", new BoolUnaryExpression(Operator.Not, TrueLiteral));
        }
    }
}
