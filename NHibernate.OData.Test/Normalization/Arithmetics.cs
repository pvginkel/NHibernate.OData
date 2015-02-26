using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Normalization
{
    [TestFixture]
    internal class Arithmetics : NormalizedTestFixture
    {
        [Test]
        public void Add()
        {
            Verify("1 add 1", 2);
            Verify("1.1 add 1", 2.1);
            Verify("1.1f add 1", (double)1.1f + 1);
            Verify("1.1m add 1", 2.1m);
            Verify("1.1f add 1f", 2.1f);
            Verify("'a' add 'b'", "ab");
            Verify("1l add 1l", 2L);
            Verify("1.1m add 1m", 2.1m);
            VerifyThrows("X'00' add X'00'");
        }

        [Test]
        public void Sub()
        {
            Verify("1 sub 1", 0);
            Verify("1.1 sub 1", 1.1 - 1);
            Verify("1.1f sub 1", (double)1.1f - 1);
            Verify("1.1m sub 1", 0.1m);
            Verify("1.1f sub 1f", 1.1f - 1f);
            VerifyThrows("'a' sub 'b'");
            Verify("1l sub 1l", 0L);
            Verify("1.1m sub 1m", 0.1m);
            VerifyThrows("X'00' sub X'00'");
        }

        [Test]
        public void Mul()
        {
            Verify("2 mul 3", 6);
            Verify("1.1 mul 2", 1.1 * 2);
            Verify("1.1f mul 2", (double)1.1f * 2);
            Verify("1.1m mul 2", 2.2m);
            Verify("1.1f mul 2f", 1.1f * 2f);
            VerifyThrows("'a' mul 'b'");
            Verify("2l mul 3l", 6L);
            Verify("1.1m mul 2m", 2.2m);
            VerifyThrows("X'00' mul X'00'");
        }

        [Test]
        public void Div()
        {
            Verify("6 div 3", 2);
            Verify("1.1 div 2", 1.1 / 2);
            Verify("1.1f div 2", (double)1.1f / 2);
            Verify("1.1m div 2", 0.55m);
            Verify("1.1f div 2f", 1.1f / 2f);
            VerifyThrows("'a' div 'b'");
            Verify("6l div 3l", 2L);
            Verify("1.1m div 2m", 0.55m);
            VerifyThrows("X'00' div X'00'");
        }

        [Test]
        public void Mod()
        {
            Verify("7 mod 3", 1);
            Verify("1.1 mod 2", 1.1 % 2);
            Verify("1.1f mod 2", (double)1.1f % 2);
            Verify("1.1m mod 2", 1.1m % 2);
            Verify("1.1f mod 2f", 1.1f % 2f);
            VerifyThrows("'a' mod 'b'");
            Verify("7l mod 3l", 1L);
            Verify("1.1m mod 2m", 1.1m % 2m);
            VerifyThrows("X'00' mod X'00'");
        }

        [Test]
        public void Negative()
        {
            Verify("- 1", -1);
            Verify("- 1.1", -1.1);
            Verify("- 1.1f", -1.1f);
            Verify("- 1.1m", -1.1m);
            Verify("- 1l", -1L);
            Verify("- time'P1D'", new XmlTimeSpan(false, 0, 0, 1, 0, 0, 0));
            VerifyThrows("- 'a'");
        }

        [Test]
        public void Ceiling()
        {
            Verify("ceiling(1.1)", 2d);
            Verify("ceiling(1.1f)", 2f);
            Verify("ceiling(1.1m)", 2m);
        }

        [Test]
        public void Floor()
        {
            Verify("floor(1.1)", 1d);
            Verify("floor(1.1f)", 1f);
            Verify("floor(1.1m)", 1m);
        }

        [Test]
        public void Round()
        {
            Verify("round(1.1)", 1d);
            Verify("round(1.1f)", 1f);
            Verify("round(1.1m)", 1m);
        }

        [Test]
        public void FloatingPoints()
        {
            Verify("ceiling(null)", null);
            Verify("ceiling(1)", 1);
            Verify("ceiling(1l)", 1L);
            VerifyThrows("ceiling('a')");
        }

        [Test]
        public void Unchanged()
        {
            Verify(
                "A add B",
                new ArithmeticExpression(
                    Operator.Add,
                    new ResolvedMemberExpression(MemberType.Normal, "A", null),
                    new ResolvedMemberExpression(MemberType.Normal, "B", null)
                )
            );
            Verify(
                "- A",
                new ArithmeticUnaryExpression(
                    Operator.Negative,
                    new ResolvedMemberExpression(MemberType.Normal, "A", null)
                )
            );
        }
    }
}
