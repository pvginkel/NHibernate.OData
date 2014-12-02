using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Domain;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Issues
{
    [TestFixture]
    internal class Issue13Fixture : DomainTestFixture
    {
        [Test]
        public void IncorrectOperatorPrecedence()
        {
            Verify(
                "substringof('Child 10', Name) and Int32 eq 10",
                Session.QueryOver<Child>().Where(x => x.Name == "Child 10" && x.Int32 == 10).List()
            );
        }
    }
}
