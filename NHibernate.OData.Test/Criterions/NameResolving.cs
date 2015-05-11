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
    internal class NameResolving : DomainTestFixture
    {
        [Test]
        public void AllowNamesToBeCustomResolved()
        {
            Verify(
                "ComponentDTO/ValueDTO eq 'Value 1'",
                Session.QueryOver<Child>().Where(x => x.Name == "Child 1").List(),
                new ODataParserConfiguration
                {
                    NameResolver = new MyNameResolver()
                }
            );
        }

        private class MyNameResolver : NameResolver
        {
            public override ResolvedName ResolveName(string name, System.Type type, bool caseSensitive)
            {
                if (name.EndsWith("DTO"))
                    name = name.Substring(0, name.Length - 3);

                return base.ResolveName(name, type, caseSensitive);
            }
        }
    }
}
