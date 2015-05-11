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

        public virtual T ArithmeticUnaryExpression(ArithmeticUnaryExpression expression)
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

        public virtual T ArithmeticExpression(ArithmeticExpression expression)
        {
            throw new QueryNotSupportException();
        }

        public virtual T MethodCallExpression(MethodCallExpression expression)
        {
            throw new QueryNotSupportException();
        }

        public virtual T ResolvedMemberExpression(ResolvedMemberExpression expression)
        {
            throw new QueryNotSupportException();
        }

        public virtual T LambdaExpression(LambdaExpression expression)
        {
            throw new QueryNotSupportException();
        }
    }
}
