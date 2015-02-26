using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal static class TypeUtil
    {
        public static System.Type TryGetCollectionItemType(System.Type collectionType)
        {
            if (collectionType == null)
                return null;

            System.Type enumerableType = collectionType.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            if (enumerableType == null)
                return null;

            return enumerableType.GetGenericArguments().Single();
        }
    }
}
