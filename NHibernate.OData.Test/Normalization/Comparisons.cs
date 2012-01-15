using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Normalization
{
    [TestFixture]
    internal class Comparisons : NormalizedTestFixture
    {
        [Test]
        public void Eq()
        {
            Verify("1 eq 1", true);
            Verify("1 eq 2", false);
            Verify("1d eq 1", true);
            Verify("X'00' eq X'00'", true);
            Verify("X'00' eq X'01'", false);
            Verify("X'00' eq X'0000'", false);
            Verify("1 eq 1L", true);
            Verify("1 eq '1'", true);
            Verify("1d eq 1f", true);
            Verify("1d eq 1m", true);
            Verify("time'P1D' eq time'P1D'", true);
            Verify("time'P1D' eq time'P1Y'", false);
        }

        [Test]
        public void Ne()
        {
            Verify("1 ne 1", false);
            Verify("1 ne 2", true);
            Verify("1d ne 1", false);
            Verify("X'00' ne X'00'", false);
            Verify("X'00' ne X'01'", true);
            Verify("X'00' ne X'0000'", true);
            Verify("time'P1D' ne time'P1D'", false);
            Verify("time'P1D' ne time'P1Y'", true);
        }

        [Test]
        public void Gt()
        {
            Verify("1 gt 1", false);
            Verify("1 gt 2", false);
            Verify("1d gt 1", false);
            Verify("2 gt 1", true);
            VerifyThrows("X'00' gt X'00'");
        }

        [Test]
        public void Ge()
        {
            Verify("1 ge 1", true);
            Verify("1 ge 2", false);
            Verify("1d ge 1", true);
            Verify("2 ge 1", true);
            VerifyThrows("X'00' ge X'00'");
        }

        [Test]
        public void Lt()
        {
            Verify("1 lt 1", false);
            Verify("1 lt 2", true);
            Verify("1d lt 1", false);
            Verify("2 lt 1", false);
            VerifyThrows("X'00' lt X'00'");
        }

        [Test]
        public void Le()
        {
            Verify("1 le 1", true);
            Verify("1 le 2", true);
            Verify("1d le 1", true);
            Verify("2 le 1", false);
            VerifyThrows("X'00' le X'00'");
        }

        [Test]
        public void Unchanged()
        {
            Verify(
                "A eq B",
                new ComparisonExpression(
                    Operator.Eq,
                    new MemberExpression(MemberType.Normal, "A"),
                    new MemberExpression(MemberType.Normal, "B")
                )
            );
        }
    }
}
