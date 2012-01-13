using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    public static class ODataExtensions
    {
        public static ICriteria ODataQuery(this ISession session, string entityName, string queryString)
        {
            return ODataParser.Parse(session, entityName, queryString);
        }

        public static ICriteria ODataQuery(this ISession session, System.Type persistentClass, string queryString)
        {
            return ODataParser.Parse(session, persistentClass, queryString);
        }

        public static ICriteria ODataQuery<T>(this ISession session, string queryString)
        {
            return ODataQuery(session, typeof(T), queryString);
        }
    }
}
