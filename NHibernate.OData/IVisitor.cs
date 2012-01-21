using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal interface IVisitor<T>
    {
        T LiteralExpression(LiteralExpression expression);

        T MemberExpression(MemberExpression expression);

        T ParenExpression(ParenExpression expression);

        T BoolUnaryExpression(BoolUnaryExpression expression);

        T ArithmeticUnaryExpression(ArithmeticUnaryExpression expression);

        T LogicalExpression(LogicalExpression expression);

        T ComparisonExpression(ComparisonExpression expression);

        T ArithmeticExpression(ArithmeticExpression expression);

        T MethodCallExpression(MethodCallExpression expression);

        T ResolvedMemberExpression(ResolvedMemberExpression expression);
    }
}
