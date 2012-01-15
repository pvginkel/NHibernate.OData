using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Normalization
{
    [TestFixture]
    internal class Removals : NormalizedTestFixture
    {
        [Test]
        public void Parens()
        {
            Verify(
                "(true)",
                TrueLiteral
            );
        }

        [Test]
        public void Methods()
        {
            Verify(
                "concat('a')",
                new LiteralExpression("a")
            );
            Verify(
                "substringof('a')",
                new LiteralExpression("a")
            );
        }
    }
}
