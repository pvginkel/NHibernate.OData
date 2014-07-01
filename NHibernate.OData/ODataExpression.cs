using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace NHibernate.OData
{
    internal class ODataExpression
    {
        private int? _top;
        private int? _skip;
        private ICriterion _criterion;
        private OrderBy[] _orderBys;
        private readonly AliasingNormalizeVisitor _normalizeVisitor;

        public ODataExpression(string queryString, System.Type persistentClass, ODataParserConfiguration configuration)
        {
            Require.NotNull(queryString, "queryString");
            Require.NotNull(persistentClass, "persistentClass");
            Require.NotNull(configuration, "configuration");

            _normalizeVisitor = new AliasingNormalizeVisitor(persistentClass, configuration.CaseSensitive);

            ParseQueryString(queryString);
        }

        private void ParseQueryString(string queryString)
        {
            if (queryString.Length == 0)
                return;

            var queryElements = queryString.Split('&');

            for (int i = 0; i < queryElements.Length; i++)
            {
                string[] elementParts = queryElements[i].Split(new[] { '=' }, 2);

                string key = HttpUtil.UriDecode(elementParts[0]);
                string value = elementParts.Length == 2 ? HttpUtil.UriDecode(elementParts[1]) : "";

                ProcessQueryStringPart(key, value);
            }
        }

        private void ProcessQueryStringPart(string key, string value)
        {
            switch (key.ToLower())
            {
                case "$filter": ProcessFilter(value); break;
                case "$orderby": ProcessOrderBy(value); break;
                case "$top": ProcessTop(value); break;
                case "$skip": ProcessSkip(value); break;

                default:
                    throw new ODataException(String.Format(
                        ErrorMessages.ODataExpression_InvalidQueryStringElement, key
                    ));
            }
        }

        private void ProcessFilter(string value)
        {
            _criterion = CriterionVisitor.CreateCriterion(
                new FilterParser(value).Parse().Visit(_normalizeVisitor)
            );
        }

        private void ProcessOrderBy(string value)
        {
            _orderBys = new OrderByParser(value, _normalizeVisitor).Parse();
        }

        private void ProcessTop(string value)
        {
            _top = GetPositiveInteger("$top", value);
        }

        private void ProcessSkip(string value)
        {
            _skip = GetPositiveInteger("$skip", value);
        }

        private int GetPositiveInteger(string key, string value)
        {
            int intValue;

            if (!int.TryParse(value, out intValue) || intValue < 0)
            {
                throw new ODataException(String.Format(
                    ErrorMessages.ODataExpression_SkipTopMustBePositive, key
                ));
            }

            return intValue;
        }

        internal ICriteria BuildCriteria(ISession session, System.Type persistentClass)
        {
            var criteria = session.CreateCriteria(persistentClass);

            foreach (var alias in _normalizeVisitor.Aliases)
            {
                criteria.CreateAlias(alias.Key, alias.Value, JoinType.LeftOuterJoin);
            }

            if (_criterion != null)
                criteria = criteria.Add(_criterion);
            if (_skip.HasValue)
                criteria = criteria.SetFirstResult(_skip.Value);
            if (_top.HasValue)
                criteria = criteria.SetMaxResults(_top.Value);

            if (_orderBys != null)
            {
                foreach (var orderBy in _orderBys)
                {
                    if (orderBy.Direction == OrderByDirection.Ascending)
                        criteria = criteria.AddOrder(Order.Asc(orderBy.Projection));
                    else
                        criteria = criteria.AddOrder(Order.Desc(orderBy.Projection));
                }
            }

            return criteria;
        }
    }
}
