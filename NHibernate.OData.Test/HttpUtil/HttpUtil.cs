using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NHibernate.OData.Test.HttpUtil
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
        }
    }
}
