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
            Require.NotNull(expression, "expression");

            if (!expression.IsBool)
            {
                switch (expression.Type)
                {
                    case ExpressionType.Paren:
                        return new ParenExpression(CoerceBoolExpression(((ParenExpression)expression).Expression));

                    case ExpressionType.Literal:
                        return CoerceLiteralExpression((LiteralExpression)expression);

                    case ExpressionType.Member:
                        return new MemberExpression(MemberType.Boolean, ((MemberExpression)expression).Members);

                    case ExpressionType.ResolvedMember:
                        return new ResolvedMemberExpression(MemberType.Boolean, ((ResolvedMemberExpression)expression).Member);

                    default:
                        throw new ODataException(ErrorMessages.Parser_ExpectedBooleanExpression);
                }
            }

            return expression;
        }

        private static Expression CoerceLiteralExpression(LiteralExpression literal)
        {
            // Force 0 and 1 literals to boolean.

            if (literal.Value is int)
            {
                switch ((int)literal.Value)
                {
                    case 0:
                        return new LiteralExpression(false, LiteralType.Boolean);

                    case 1:
                        return new LiteralExpression(true, LiteralType.Boolean);
                }
            }

            throw new ODataException(ErrorMessages.Parser_ExpectedBooleanExpression);
        }
    }
}
