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

        T ArithmicUnaryExpression(ArithmicUnaryExpression expression);

        T LogicalExpression(LogicalExpression expression);

        T ComparisonExpression(ComparisonExpression expression);

        T ArithmicExpression(ArithmicExpression expression);

        T MethodCallExpression(MethodCallExpression expression);

        T AliasedMemberExpression(AliasedMemberExpression expression);
    }
}
