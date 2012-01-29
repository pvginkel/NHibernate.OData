using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal class PathParser : Parser
    {
        public PathParser(string source)
            : base(source, ParserMode.Path)
        {
        }

        public MemberExpression Parse()
        {
            var result = ParseCommon();

            ExpectAtEnd();

            result.Visit(new NormalizeVisitor());

            var memberExpression = result as MemberExpression;

            if (memberExpression == null)
                throw new ODataException(ErrorMessages.PathParser_InvalidPath);

            return memberExpression;
        }
    }
}
