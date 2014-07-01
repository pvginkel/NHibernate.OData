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
    internal class Issue6Fixture : DomainTestFixture
    {
        [Test]
        public void SelectByParentAndChild()
        {
            Verify(
                "child/name eq 'Child 1' and int32 lt 5",
                Session.QueryOver<Parent>().JoinQueryOver<Child>(p => p.Child).Where(p => p.Name == "Child 1").List(),
                new ODataParserConfiguration
                {
                    CaseSensitive = false
                }
            );
        }

        [Test]
        public void OrderByChild()
        {
            VerifyOrdered<Parent>(
                "$orderby=child/name desc",
                q => q.OrderBy(p => p.Name).Desc,
                new ODataParserConfiguration
                {
                    CaseSensitive = false
                },
                x => x.Child == null
            );
        }
    }
}
