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
            Assert.AreEqual("abc", OData.HttpUtil.UriDecode("abc"));
            Assert.AreEqual(" ", OData.HttpUtil.UriDecode("%20"));
            Assert.AreEqual(" ", OData.HttpUtil.UriDecode("+"));
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
