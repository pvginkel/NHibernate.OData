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
    internal class CollectionMethods : DomainTestFixture
    {
        [Test]
        public void AnyWithoutCondition()
        {
            Verify(
                "RelatedParents/any()",
                Session.QueryOver<Parent>().Where(x => x.Name == "Parent 9" || x.Name == "Parent 10").List()
            );
        }

        [Test]
        public void AllWithoutConditionFails()
        {
            VerifyThrows<Parent>("RelatedParents/all()", typeof(ODataException));
        }
    }
}
