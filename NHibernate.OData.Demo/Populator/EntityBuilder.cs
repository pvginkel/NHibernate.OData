using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FastMember;

namespace NHibernate.OData.Demo.Populator
{
    internal class EntityBuilder
    {
        public System.Type Type { get; private set; }

        public TypeAccessor Accessor { get; private set; }

        public IDictionary<string, PropertyInfo> Properties { get; private set; }

        public EntityBuilder(System.Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            Type = type;

            Accessor = TypeAccessor.Create(type);

            Properties = new Dictionary<string, PropertyInfo>();

            foreach (var property in type.GetProperties())
            {
                Properties[property.Name] = property;
            }
        }

        public object CreateInstance()
        {
            return Activator.CreateInstance(Type);
        }
    }
}
