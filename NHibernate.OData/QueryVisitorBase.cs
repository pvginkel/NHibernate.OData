using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal class QueryVisitorBase<T> : IVisitor<T>
    {
        public virtual T LiteralExpression(LiteralExpression expression)
        {
            throw new QueryNotSupportException();
        }

        public virtual T MemberExpression(MemberExpression expression)
        {
            throw new QueryNotSupportException();
        }

        public virtual T ParenExpression(ParenExpression expression)
        {
            throw new QueryNotSupportException();
        }

        public virtual T BoolUnaryExpression(BoolUnaryExpression expression)
        {
            throw new QueryNotSupportException();
        }

        public virtual T ArithmicUnaryExpression(ArithmicUnaryExpression expression)
        {
            throw new QueryNotSupportException();
        }

        public virtual T LogicalExpression(LogicalExpression expression)
        {
            throw new QueryNotSupportException();
        }

        public virtual T ComparisonExpression(ComparisonExpression expression)
        {
            throw new QueryNotSupportException();
        }

        public virtual T ArithmicExpression(ArithmicExpression expression)
        {
            throw new QueryNotSupportException();
        }

        public virtual T MethodCallExpression(MethodCallExpression expression)
        {
            throw new QueryNotSupportException();
        }

        public virtual T AliasedMemberExpression(AliasedMemberExpression expression)
        {
            throw new QueryNotSupportException();
        }
    }
}
