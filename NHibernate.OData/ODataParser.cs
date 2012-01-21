using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    public static class ODataParser
    {
        public static ICriteria ODataQuery(this ISession session, string entityName, string queryString)
        {
            Require.NotNull(session, "session");
            Require.NotNull(entityName, "entityName");
            Require.NotNull(queryString, "queryString");

            var expression = new ODataExpression(queryString);

            return expression.BuildCriteria(session, entityName);
        }

        public static ICriteria ODataQuery(this ISession session, System.Type persistentClass, string queryString)
        {
            Require.NotNull(session, "session");
            Require.NotNull(persistentClass, "persistentClass");
            Require.NotNull(queryString, "queryString");

            var expression = new ODataExpression(queryString);

            return expression.BuildCriteria(session, persistentClass);
        }

        public static ICriteria ODataQuery<T>(this ISession session, string queryString)
        {
            return ODataQuery(session, typeof(T), queryString);
        }
    }
}
