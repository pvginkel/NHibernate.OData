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

        [Test]
        public void AnyWithSimpleCondition()
        {
            Verify(
                "RelatedParents/any(x:x/Int32 lt 5)",
                Session.QueryOver<Parent>().Where(x => x.Name == "Parent 9").List()
            );
        }

        [Test]
        public void ThrowsOnDuplicateLambdaParameters()
        {
            VerifyThrows<Parent>("RelatedParents/any(x:x/RelatedParents/any(x:x/Int32 gt 0))", typeof(ODataException));
            VerifyThrows<Parent>("RelatedParents/any($it:$it ne null)", typeof(ODataException));
        }

        [Test]
        public void ThrowsOnNonCollection()
        {
            VerifyThrows<Parent>("Int32/any()", typeof(ODataException));
        }

        [Test]
        public void RootScopeVariable()
        {
            Verify(
                "RelatedParents/any(x:x/Int32 eq $it/Int32 sub 8)",
                Session.QueryOver<Parent>().Where(x => x.Name == "Parent 9").List()
            );

            Verify(
                "RelatedParents/any(x:x/RelatedParents/any(y:y/Int32 eq $it/Int32 sub 6))",
                Session.QueryOver<Parent>().Where(x => x.Name == "Parent 10").List()
            );
        }

        [Test]
        public void NestedAny()
        {
            Verify(
                "RelatedParents/any(x:x/RelatedParents/any())",
                Session.QueryOver<Parent>().Where(x => x.Name == "Parent 10").List()
            );
        }

        [Test]
        public void JoinInLambda()
        {
            Verify(
                "RelatedParents/Any(x:x/Child/Int32 eq 3)",
                Session.QueryOver<Parent>().Where(x => x.Name == "Parent 9").List()
            );
        }

        [Test]
        public void NestedAnyWithJoin()
        {
            Verify(
                "RelatedParents/any(x:x/RelatedParents/any(y:y/Child/Int32 eq 2))",
                Session.QueryOver<Parent>().Where(x => x.Name == "Parent 10").List()
            );
        }

        [Test]
        public void AllWithSimpleCondition()
        {
            Verify(
                "RelatedParents/all(x:x/Int32 ge 5)",
                Session.QueryOver<Parent>().Where(x => x.Name != "Parent 9").List()
            );

            Verify(
                "RelatedParents/all(x:x/Int32 ge 999)",
                Session.QueryOver<Parent>().Where(x => x.Name != "Parent 9" && x.Name != "Parent 10").List()
            );
        }

        [Test]
        public void AllWithJoin()
        {
            Verify(
                "RelatedParents/All(x:x/Child/Int32 ge 5)",
                Session.QueryOver<Parent>().Where(x => x.Name != "Parent 9").List()
            );
        }
    }
}
