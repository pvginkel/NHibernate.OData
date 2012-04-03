using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;
using NHibernate.OData.Test.Domain;

namespace NHibernate.OData.Test.Criterions
{
    [TestFixture]
    internal class NestedMembers : DomainTestFixture
    {
        [Test]
        public void SelectByChild()
        {
            Verify(
                "Child/Name eq 'Child 1'",
                Session.QueryOver<Parent>().JoinQueryOver<Child>(p => p.Child).Where(p => p.Name == "Child 1").List()
            );
        }

        [Test]
        public void OrderByChild()
        {
            VerifyOrdered<Parent>("$orderby=Child/Name desc", q => q.Where(p => p.Child != null).OrderBy(p => p.Name).Desc);
        }

        [Test]
        public void SelectByParentAndChild()
        {
            Verify(
                "Child/Name eq 'Child 1' and Int32 lt 5",
                Session.QueryOver<Parent>().JoinQueryOver<Child>(p => p.Child).Where(p => p.Name == "Child 1").List()
            );
        }
    }
}
