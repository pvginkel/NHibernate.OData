using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal class QueryMethodVisitorBase<TResult> : IMethodVisitor<TResult, Expression[]>
    {
        public virtual TResult IsOfMethod(IsOfMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult CastMethod(CastMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult EndsWithMethod(EndsWithMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult IndexOfMethod(IndexOfMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult ReplaceMethod(ReplaceMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult StartsWithMethod(StartsWithMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult ToLowerMethod(ToLowerMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult ToUpperMethod(ToUpperMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult TrimMethod(TrimMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult SubStringMethod(SubStringMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult SubStringOfMethod(SubStringOfMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult ConcatMethod(ConcatMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult LengthMethod(LengthMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult YearMethod(YearMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult MonthMethod(MonthMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult DayMethod(DayMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult HourMethod(HourMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult MinuteMethod(MinuteMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult SecondMethod(SecondMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult RoundMethod(RoundMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult FloorMethod(FloorMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }

        public virtual TResult CeilingMethod(CeilingMethod method, Expression[] arguments)
        {
            throw new QueryNotSupportException();
        }
    }
}
