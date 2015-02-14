using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Lexer
{
    [TestFixture]
    internal class LitralParsing : LexerTestFixture
    {
        [Test]
        public void Integer()
        {
            Verify("1", 1);
        }

        [Test]
        public void Double()
        {
            Verify("1.1", 1.1);
            Verify("1.", 1.0);
            Verify("1d", 1.0);
            Verify("1E1", 1E1);
            Verify("1e1", 1E1);
            Verify("12345d", 12345d);
            Verify("12345.12345", 12345.12345);

            VerifyThrows(".1");
            VerifyThrows("1E-");
            VerifyThrows("1E1234", typeof(OverflowException));
        }

        [Test]
        public void Single()
        {
            Verify("1.1f", 1.1f);
            Verify("1.f", 1.0f);
            Verify("1f", 1.0f);
            Verify("1E1f", 1E1f);
            Verify("1e1f", 1E1f);
            Verify("12345f", 12345f);
            Verify("12345.1234f", 12345.12345f);

            VerifyThrows(".1f");
            VerifyThrows("1E-f");
            VerifyThrows("1E1234f", typeof(OverflowException));
        }

        [Test]
        public void SpecialDouble()
        {
            Verify("INF", double.PositiveInfinity);
            Verify("-INF", double.NegativeInfinity);
            Verify("inf", new IdentifierToken("inf"));
            Verify("-inf", SyntaxToken.Negative, new IdentifierToken("inf"));
            Verify("Nan", double.NaN);
        }

        [Test]
        public void String()
        {
            Verify("'a'", "a");
            Verify("'abc'", "abc");
            Verify("'a b c '", "a b c ");
            Verify("'a''b'", "a'b");
            VerifyThrows("'");
            VerifyThrows("'a");
            VerifyThrows("'a''");
        }

        [Test]
        public void Binary()
        {
            Verify("X'AABB'", new byte[] { 0xaa, 0xbb });
            Verify("X'aabb'", new byte[] { 0xaa, 0xbb });
            Verify("binary'AABB'", new byte[] { 0xaa, 0xbb });
            Verify("binary'aabb'", new byte[] { 0xaa, 0xbb });
            VerifyThrows("X'a'");
        }

        [Test]
        public void Boolean()
        {
            Verify("true", true);
            Verify("false", false);
        }

        [Test]
        public void Null()
        {
            Verify("null", new object[] { null });
        }

        [Test]
        public void DateTime()
        {
            Verify("datetime'2014-01-02T3:04'", new DateTime(2014, 1, 2, 3, 4, 0));
            Verify("datetime'2014-01-02T03:04'", new DateTime(2014, 1, 2, 3, 4, 0));
            Verify("datetime'2014-01-02T03:04:05'", new DateTime(2014, 1, 2, 3, 4, 5));
            Verify("datetime'2014-01-02T03:04:05.5000'", new DateTime(2014, 1, 2, 3, 4, 5, 5000 / 1000));
            Verify("datetime'2014-01-02T03:04:05.0002000'", new DateTime(2014, 1, 2, 3, 4, 5, 2000 / 1000));

            Verify("datetime'2014-01-02T3:04Z'", new DateTimeOffset(2014, 1, 2, 3, 4, 0, TimeSpan.Zero).UtcDateTime);
            Verify("datetime'2014-01-02T3:04+01:30'", new DateTimeOffset(2014, 1, 2, 3, 4, 0, TimeSpan.FromMinutes(90)).UtcDateTime);
            Verify("datetime'2014-01-02T3:04-01:30'", new DateTimeOffset(2014, 1, 2, 3, 4, 0, TimeSpan.FromMinutes(-90)).UtcDateTime);
        }
    }
}
