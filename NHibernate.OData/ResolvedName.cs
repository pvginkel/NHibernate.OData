using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    public class ResolvedName
    {
        public System.Type Type { get; private set; }
        public string Name { get; private set; }

        public ResolvedName(System.Type type, string name)
        {
            Require.NotNull(type, "type");
            Require.NotNull(name, "name");

            Type = type;
            Name = name;
        }
    }
}
