using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NHibernate.OData
{
    /// <summary>
    /// Name resolver to resolve names from queries to entity names.
    /// </summary>
    public class NameResolver
    {
        /// <summary>
        /// Resolve a query name to an entity name.
        /// </summary>
        /// <param name="name">The name to map.</param>
        /// <param name="type">The type of the entity to map the name for.</param>
        /// <param name="caseSensitive">Whether the <param name="name"> parameter must be treated case sensitive.</param></param>
        /// <returns>The mapped name and member type or null when the name could not be resolved.</returns>
        public virtual ResolvedName ResolveName(string name, System.Type type, bool caseSensitive)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            if (!caseSensitive)
                bindingFlags |= BindingFlags.IgnoreCase;

            var property = type.GetProperty(name, bindingFlags);

            if (property != null)
                return new ResolvedName(property.PropertyType, property.Name);

            var field = type.GetField(name, bindingFlags);

            if (field != null)
                return new ResolvedName(field.FieldType, field.Name);

            return null;
        }
    }
}
