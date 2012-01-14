using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    public static class ODataParser
    {
        public static ICriteria Parse(this ISession session, string entityName, string queryString)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            if (entityName == null)
                throw new ArgumentNullException("entityName");
            if (queryString == null)
                throw new ArgumentNullException("queryString");

            var expression = new ODataExpression(queryString);

            return expression.BuildCriteria(session, entityName);
        }

        public static ICriteria Parse(this ISession session, System.Type persistentClass, string queryString)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            if (persistentClass == null)
                throw new ArgumentNullException("persistentClass");
            if (queryString == null)
                throw new ArgumentNullException("queryString");

            var expression = new ODataExpression(queryString);

            return expression.BuildCriteria(session, persistentClass);
        }

        public static ICriteria Parse<T>(this ISession session, string queryString)
        {
            return Parse(session, typeof(T), queryString);
        }
    }
}
