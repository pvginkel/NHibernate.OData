using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Lexer
{
    [TestFixture]
    internal class Basics : LexerTestFixture
    {
        [Test]
        public void EmptyString()
        {
            Verify("", new Token[] { });
        }

        [Test]
        public void WhitespaceString()
        {
            Verify(" \t \r \n", new Token[] { });
        }

        [Test]
        public void MultipleTokens()
        {
            Verify("1 'a' 7d INF", 1, "a", 7d, double.PositiveInfinity);
        }


    }
}
