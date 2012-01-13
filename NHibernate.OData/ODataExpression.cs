using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal class ODataExpression
    {
        private int? _top;
        private int? _skip;

        public ODataExpression(string queryString)
        {
            if (queryString == null)
                throw new ArgumentNullException("queryString");

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
            throw new NotImplementedException();
        }

        private void ProcessOrderBy(string value)
        {
            throw new NotImplementedException();
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

        internal ICriteria BuildCriteria(ISession session, string entityName)
        {
            throw new NotImplementedException();
        }

        internal ICriteria BuildCriteria(ISession session, System.Type persistentClass)
        {
            throw new NotImplementedException();
        }
    }
}
