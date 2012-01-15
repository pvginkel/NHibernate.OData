using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal static class ExpressionUtil
    {
        public static Expression CoerceBoolExpression(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            if (!expression.IsBool)
            {
                // Recurs into paren expressions.

                var paren = expression as ParenExpression;

                if (paren != null)
                    return new ParenExpression(CoerceBoolExpression(paren.Expression));

                // Force 0 and 1 literals to boolean.

                var literal = expression as LiteralExpression;

                if (literal != null && literal.Value is int)
                {
                    switch ((int)literal.Value)
                    {
                        case 0:
                            return new LiteralExpression(false, LiteralType.Boolean);

                        case 1:
                            return new LiteralExpression(true, LiteralType.Boolean);
                    }
                }

                // Coerce member expressions to boolean.

                var member = expression as MemberExpression;

                if (member != null)
                    return new MemberExpression(MemberType.Boolean, member.Members);

                throw new ODataException(ErrorMessages.Parser_ExpectedBooleanExpression);
            }

            return expression;
        }
    }
}
