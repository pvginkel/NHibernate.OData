using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Parser
{
    [TestFixture]
    internal class Basics : ParserTestFixture
    {
        [Test]
        public void EmptyExpression()
        {
            VerifyThrows("");
        }
    }
}
