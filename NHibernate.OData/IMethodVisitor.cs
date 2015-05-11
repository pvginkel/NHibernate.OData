using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal interface IMethodVisitor<TResult, TArg>
    {
        TResult IsOfMethod(IsOfMethod method, TArg arg);

        TResult CastMethod(CastMethod method, TArg arg);

        TResult EndsWithMethod(EndsWithMethod method, TArg arg);

        TResult IndexOfMethod(IndexOfMethod method, TArg arg);

        TResult ReplaceMethod(ReplaceMethod method, TArg arg);

        TResult StartsWithMethod(StartsWithMethod method, TArg arg);

        TResult ToLowerMethod(ToLowerMethod method, TArg arg);

        TResult ToUpperMethod(ToUpperMethod method, TArg arg);

        TResult TrimMethod(TrimMethod method, TArg arg);

        TResult SubStringMethod(SubStringMethod method, TArg arg);

        TResult SubStringOfMethod(SubStringOfMethod method, TArg arg);

        TResult ConcatMethod(ConcatMethod method, TArg arg);

        TResult LengthMethod(LengthMethod method, TArg arg);

        TResult YearMethod(YearMethod method, TArg arg);

        TResult MonthMethod(MonthMethod method, TArg arg);

        TResult DayMethod(DayMethod method, TArg arg);

        TResult HourMethod(HourMethod method, TArg arg);

        TResult MinuteMethod(MinuteMethod method, TArg arg);

        TResult SecondMethod(SecondMethod method, TArg arg);

        TResult RoundMethod(RoundMethod method, TArg arg);

        TResult FloorMethod(FloorMethod method, TArg arg);

        TResult CeilingMethod(CeilingMethod method, TArg arg);

        TResult AnyMethod(AnyMethod method, TArg arg);

        TResult AllMethod(AllMethod method, TArg arg);
    }
}
