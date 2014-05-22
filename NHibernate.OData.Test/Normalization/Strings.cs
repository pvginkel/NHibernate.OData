using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Normalization
{
    [TestFixture]
    internal class Strings : NormalizedTestFixture
    {
        [Test]
        public void Concat()
        {
            Verify("concat('a', 'b')", "ab");
            Verify("concat(null, 'a')", "a");
            Verify("concat('a', null)", "a");
            Verify("concat(null, null)", null);
        }

        [Test]
        public void SubString()
        {
            Verify("substring('abc', 2)", "bc");
            Verify("substring('abc', 2, 1)", "b");
            Verify("substring('abc', 2l)", "bc");
            Verify("substring(null, 2)", null);
            Verify("substring(null, null)", null);
            VerifyThrows("substring('a', 'b')");
            Verify("substring(X'00', 2)", "ystem.Byte[]");
            VerifyThrows("substring('a', 2, 'b')");
        }

        [Test]
        public void IndexOf()
        {
            Verify("indexof('abc', 'b')", 2);
            Verify("indexof('abc', 'd')", null);
            Verify("indexof(null, 'a')", null);
            Verify("indexof(X'00', 'a')", null);
        }

        [Test]
        public void SubStringOf()
        {
            Verify("substringof('b', 'abc')", true);
            Verify("substringof('d', 'abc')", false);
            Verify("substringof('a', null)", false);
            Verify("substringof('a', X'00')", false);
        }

        [Test]
        public void StartsWith()
        {
            Verify("startswith('abc', 'a')", true);
            Verify("startswith('abc', 'b')", false);
            Verify("startswith(null, 'b')", false);
        }

        [Test]
        public void EndsWith()
        {
            Verify("endswith('abc', 'c')", true);
            Verify("endswith('abc', 'b')", false);
            Verify("endswith(null, 'b')", false);
        }

        [Test]
        public void Length()
        {
            Verify("length('abc')", 3);
            Verify("length(1)", 1);
            Verify("length(null)", null);
        }

        [Test]
        public void Replace()
        {
            Verify("replace('abc', 'a', 'd')", "dbc");
            Verify("replace(null, 'a', 'd')", null);
            Verify("replace('abc', null, 'd')", null);
            Verify("replace('abc', 'a', null)", null);
            Verify("replace(123, 2, 4)", "143");
        }

        [Test]
        public void ToUpper()
        {
            Verify("toupper('abc')", "ABC");
            Verify("toupper('ABC')", "ABC");
            Verify("toupper(null)", null);
        }

        [Test]
        public void ToLower()
        {
            Verify("tolower('abc')", "abc");
            Verify("tolower('ABC')", "abc");
            Verify("tolower(null)", null);
        }

        [Test]
        public void Trim()
        {
            Verify("trim(' abc ')", "abc");
            Verify("trim('abc')", "abc");
            Verify("trim(null)", null);
        }
    }
}
