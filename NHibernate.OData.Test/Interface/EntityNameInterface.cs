using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Domain;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Interface
{
    [TestFixture]
    internal class EntityNameInterface : DomainTestFixture
    {
        [Test]
        public void Tests()
        {
            Assert.AreEqual(
                Session.QueryOver<Parent>().Where(p => p.Name == "Parent 1").List(),
                Session.ODataQuery("Parent", "$filter=Name eq 'Parent 1'").List()
            );
        }
    }
}
