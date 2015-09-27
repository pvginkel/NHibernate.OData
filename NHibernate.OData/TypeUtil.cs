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

            if (collectionType.IsInterface && IsCollectionType(collectionType))
                return collectionType.GetGenericArguments().Single();

            System.Type enumerableType = collectionType.GetInterfaces().FirstOrDefault(IsCollectionType);
            if (enumerableType == null)
                return null;

            return enumerableType.GetGenericArguments().Single();
        }

        private static bool IsCollectionType(System.Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }
    }
}
