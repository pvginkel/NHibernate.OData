using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Engine;

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
            return ODataQuery(session, entityName, queryString, null);
        }

        /// <summary>
        /// Builds an ICriteria for an already parsed query string.
        /// </summary>
        /// <param name="session">NHibernate session to build the
        /// ICriteria for.</param>
        /// <param name="entityName">Name of the entity to query.</param>
        /// <param name="queryStringParts">Unescaped query string parts.</param>
        /// <returns>An <see cref="ICriteria"/> based on the provided
        /// query string.</returns>
        public static ICriteria ODataQuery(this ISession session, string entityName, IEnumerable<KeyValuePair<string, string>> queryStringParts)
        {
            return ODataQuery(session, entityName, queryStringParts, null);
        }

        /// <summary>
        /// Parses an OData query string and builds an ICriteria for it.
        /// </summary>
        /// <param name="session">NHibernate session to build the
        /// ICriteria for.</param>
        /// <param name="entityName">Name of the entity to query.</param>
        /// <param name="queryString">OData query string.</param>
        /// <param name="configuration">Extra configuration.</param>
        /// <returns>An <see cref="ICriteria"/> based on the provided
        /// query string.</returns>
        public static ICriteria ODataQuery(this ISession session, string entityName, string queryString, ODataParserConfiguration configuration)
        {
            Require.NotNull(session, "session");
            Require.NotNull(entityName, "entityName");
            Require.NotNull(queryString, "queryString");

            var persistenceClass = ResolvePersistenceClass(session, entityName);

            var expression = new ODataExpression(queryString, persistenceClass, configuration ?? new ODataParserConfiguration());

            return expression.BuildCriteria(session, persistenceClass);
        }

        /// <summary>
        /// Builds an ICriteria for an already parsed query string.
        /// </summary>
        /// <param name="session">NHibernate session to build the
        /// ICriteria for.</param>
        /// <param name="entityName">Name of the entity to query.</param>
        /// <param name="queryStringParts">Unescaped query string parts.</param>
        /// <param name="configuration">Extra configuration.</param>
        /// <returns>An <see cref="ICriteria"/> based on the provided
        /// query string.</returns>
        public static ICriteria ODataQuery(this ISession session, string entityName, IEnumerable<KeyValuePair<string, string>> queryStringParts, ODataParserConfiguration configuration)
        {
            Require.NotNull(session, "session");
            Require.NotNull(entityName, "entityName");
            Require.NotNull(queryStringParts, "queryStringParts");

            var persistenceClass = ResolvePersistenceClass(session, entityName);

            var expression = new ODataExpression(queryStringParts, persistenceClass, configuration ?? new ODataParserConfiguration());

            return expression.BuildCriteria(session, persistenceClass);
        }

        private static System.Type ResolvePersistenceClass(ISession session, string entityName)
        {
            var factory = (ISessionFactoryImplementor)session.SessionFactory;

            var implementors = factory.GetImplementors(entityName);

            if (implementors != null && implementors.Length == 1)
            {
                var entityPersister = factory.GetEntityPersister(implementors[0]);

                if (entityPersister != null)
                    return entityPersister.EntityMetamodel.RootType;
            }

            throw new QueryException("Cannot resolve entity name '{0}'", entityName);
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
            return ODataQuery(session, persistentClass, queryString, null);
        }

        /// <summary>
        /// Builds an ICriteria for an already parsed query string.
        /// </summary>
        /// <param name="session">NHibernate session to build the
        /// ICriteria for.</param>
        /// <param name="persistentClass">Type of the entity to query.</param>
        /// <param name="queryStringParts">Unescaped query string parts.</param>
        /// <returns>An <see cref="ICriteria"/> based on the provided
        /// query string.</returns>
        public static ICriteria ODataQuery(this ISession session, System.Type persistentClass, IEnumerable<KeyValuePair<string, string>> queryStringParts)
        {
            return ODataQuery(session, persistentClass, queryStringParts, null);
        }

        /// <summary>
        /// Parses an OData query string and builds an ICriteria for it.
        /// </summary>
        /// <param name="session">NHibernate session to build the
        /// ICriteria for.</param>
        /// <param name="persistentClass">Type of the entity to query.</param>
        /// <param name="queryString">OData query string.</param>
        /// <param name="configuration">Extra configuration.</param>
        /// <returns>An <see cref="ICriteria"/> based on the provided
        /// query string.</returns>
        public static ICriteria ODataQuery(this ISession session, System.Type persistentClass, string queryString, ODataParserConfiguration configuration)
        {
            Require.NotNull(session, "session");
            Require.NotNull(persistentClass, "persistentClass");
            Require.NotNull(queryString, "queryString");

            var expression = new ODataExpression(queryString, persistentClass, configuration ?? new ODataParserConfiguration());

            return expression.BuildCriteria(session, persistentClass);
        }

        /// <summary>
        /// Builds an ICriteria for an already parsed query string.
        /// </summary>
        /// <param name="session">NHibernate session to build the
        /// ICriteria for.</param>
        /// <param name="persistentClass">Type of the entity to query.</param>
        /// <param name="queryStringParts">Unescaped query string parts.</param>
        /// <param name="configuration">Extra configuration.</param>
        /// <returns>An <see cref="ICriteria"/> based on the provided
        /// query string.</returns>
        public static ICriteria ODataQuery(this ISession session, System.Type persistentClass, IEnumerable<KeyValuePair<string, string>> queryStringParts, ODataParserConfiguration configuration)
        {
            Require.NotNull(session, "session");
            Require.NotNull(persistentClass, "persistentClass");
            Require.NotNull(queryStringParts, "queryStringParts");

            var expression = new ODataExpression(queryStringParts, persistentClass, configuration ?? new ODataParserConfiguration());

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
            return ODataQuery<T>(session, queryString, null);
        }

        /// <summary>
        /// Parses an OData query string and builds an ICriteria for it.
        /// </summary>
        /// <param name="session">NHibernate session to build the
        /// ICriteria for.</param>
        /// <param name="queryString">OData query string.</param>
        /// <param name="configuration">Extra configuration.</param>
        /// <typeparam name="T">Type of the entity to query.</typeparam>
        /// <returns>An <see cref="ICriteria"/> based on the provided
        /// query string.</returns>
        public static ICriteria ODataQuery<T>(this ISession session, string queryString, ODataParserConfiguration configuration)
        {
            return ODataQuery(session, typeof(T), queryString, configuration);
        }

        /// <summary>
        /// Builds an ICriteria for an already parsed query string.
        /// </summary>
        /// <param name="session">NHibernate session to build the
        /// ICriteria for.</param>
        /// <param name="queryStringParts">Unescaped query string parts.</param>
        /// <typeparam name="T">Type of the entity to query.</typeparam>
        /// <returns>An <see cref="ICriteria"/> based on the provided
        /// query string.</returns>
        public static ICriteria ODataQuery<T>(this ISession session, IEnumerable<KeyValuePair<string, string>> queryStringParts)
        {
            return ODataQuery<T>(session, queryStringParts, null);
        }

        /// <summary>
        /// Builds an ICriteria for an already parsed query string.
        /// </summary>
        /// <param name="session">NHibernate session to build the
        /// ICriteria for.</param>
        /// <param name="queryStringParts">Unescaped query string parts.</param>
        /// <param name="configuration">Extra configuration.</param>
        /// <typeparam name="T">Type of the entity to query.</typeparam>
        /// <returns>An <see cref="ICriteria"/> based on the provided
        /// query string.</returns>
        public static ICriteria ODataQuery<T>(this ISession session, IEnumerable<KeyValuePair<string, string>> queryStringParts, ODataParserConfiguration configuration)
        {
            return ODataQuery(session, typeof(T), queryStringParts, configuration);
        }
    }
}
