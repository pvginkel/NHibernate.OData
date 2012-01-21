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
    internal class QueryTest : DomainTestFixture
    {
        [Test]
        public void Test()
        {
            using (var session = OpenSession())
            {
                foreach (var parent in session.QueryOver<Parent>().List())
                {
                    Console.WriteLine(parent.Name);
                }
            }
        }
    }
}
