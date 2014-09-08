using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NHibernate.OData.Test.SupportFixtures
{
    [TestFixture]
    internal class HttpUtil
    {
        [Test]
        public void UriDecode()
        {
            Assert.AreEqual("abc", OData.HttpUtil.UriDecode("abc", false));
            Assert.AreEqual(" ", OData.HttpUtil.UriDecode("%20", false));
            Assert.AreEqual(" ", OData.HttpUtil.UriDecode("+", false));
            Assert.AreEqual("+", OData.HttpUtil.UriDecode("%2b", false));

            Assert.AreEqual("Я", OData.HttpUtil.UriDecode("%d0%af", true)); // UTF-8 encoded character
            Assert.AreEqual("+", OData.HttpUtil.UriDecode("%2b", true));

            Assert.IsFalse(OData.HttpUtil.IsHex('X'));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InvalidHex()
        {
            OData.HttpUtil.HexToInt('x');
        }
    }
}
