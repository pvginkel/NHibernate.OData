using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    /// <summary>
    /// OData parser for NHibernate.
    /// </summary>
    public static class ODataParser
    {
        /// <summary>
        /// Parses an OData query string and builds an ICriteria for it.
        /// </summary>
        /// <param name="session">NHibernate session to build the
        /// ICriteria for.</param>
        /// <param name="entityName">Name of the entity to query.</param>
        /// <param name="queryString">OData query string.</param>
        /// <returns>An <see cref="ICriteria"/> based on the provided
        /// query string.</returns>
        public static ICriteria ODataQuery(this ISession session, string entityName, string queryString)
        {
            Require.NotNull(session, "session");
            Require.NotNull(entityName, "entityName");
            Require.NotNull(queryString, "queryString");

            var expression = new ODataExpression(queryString);

            return expression.BuildCriteria(session, entityName);
        }

        /// <summary>
        /// Parses an OData query string and builds an ICriteria for it.
        /// </summary>
        /// <param name="session">NHibernate session to build the
        /// ICriteria for.</param>
        /// <param name="persistentClass">Type of the entity to query.</param>
        /// <param name="queryString">OData query string.</param>
        /// <returns>An <see cref="ICriteria"/> based on the provided
        /// query string.</returns>
        public static ICriteria ODataQuery(this ISession session, System.Type persistentClass, string queryString)
        {
            Require.NotNull(session, "session");
            Require.NotNull(persistentClass, "persistentClass");
            Require.NotNull(queryString, "queryString");

            var expression = new ODataExpression(queryString);

            return expression.BuildCriteria(session, persistentClass);
        }

        /// <summary>
        /// Parses an OData query string and builds an ICriteria for it.
        /// </summary>
        /// <param name="session">NHibernate session to build the
        /// ICriteria for.</param>
        /// <param name="queryString">OData query string.</param>
        /// <typeparam name="T">Type of the entity to query.</typeparam>
        /// <returns>An <see cref="ICriteria"/> based on the provided
        /// query string.</returns>
        public static ICriteria ODataQuery<T>(this ISession session, string queryString)
        {
            return ODataQuery(session, typeof(T), queryString);
        }
    }
}
