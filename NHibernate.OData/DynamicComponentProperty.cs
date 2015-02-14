using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal class DynamicComponentProperty
    {
        public string Name { get; private set; }
        public System.Type Type { get; private set; }

        public DynamicComponentProperty(string name, System.Type type)
        {
            Require.NotNull(name, "name");
            Require.NotNull(type, "type");

            Name = name;
            Type = type;
        }
    }
}
