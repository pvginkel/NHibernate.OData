using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal class Alias
    {
        public string Name { get; private set; }
        public string AssociationPath { get; private set; }
        public System.Type ReturnedType { get; private set; }

        public Alias(string name, string associationPath, System.Type returnedType)
        {
            Require.NotEmpty(name, "name");
            Require.NotNull(associationPath, "associationPath");
            Require.NotNull(returnedType, "returnedType");

            Name = name;
            AssociationPath = associationPath;
            ReturnedType = returnedType;
        }
    }
}
