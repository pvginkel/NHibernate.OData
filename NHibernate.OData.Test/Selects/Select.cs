using NHibernate.OData.Test.Domain;
using NHibernate.OData.Test.Support;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData.Test.Selects
{
    [TestFixture]
    class Select : DomainTestFixture
    {
        [Test]
        public void SelectTwoFields()
        {
            VerifySelectedFields<Parent>("$select=Id,Name",new string[] { "Id", "Name" }, new string[] { "Child", "LengthString" });
        }
    }
}
