using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Domain;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Criterions
{
    [TestFixture]
    internal class Components : DomainTestFixture
    {
        [Test]
        public void SelectByComponentMemberEq()
        {
            Verify(
                "Component/Value eq 'Value 1'",
                Session.QueryOver<Child>().Where(x => x.Name == "Child 1").List()
            );
        }

        [Test]
        public void SelectByChildComponentMemberEq()
        {
            Verify(
                "Child/Component/Value eq 'Value 1'",
                Session.QueryOver<Parent>().Where(x => x.Name == "Parent 1").List()
            );
        }

        // In NHibernate a component is considered to be null when all of its mapped properties are null
        [Test]
        public void SelectByComponentIsNull()
        {
            Verify(
                "Component eq null",
                Session.QueryOver<Child>().Where(x => x.Name == "Child 10").List()
            );
        }

        [Test]
        public void SelectByChildComponentIsNull()
        {
            Verify(
                "Child/Component eq null",
                Session.QueryOver<Parent>().Where(x => x.Name == "Parent 10").List()
            );
        }
    }
}