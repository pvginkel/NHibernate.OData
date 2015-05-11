using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace NHibernate.OData
{
    internal abstract class Method
    {
        public static readonly IsOfMethod IsOfMethod = new IsOfMethod();
        public static readonly CastMethod CastMethod = new CastMethod();
        public static readonly EndsWithMethod EndsWithMethod = new EndsWithMethod();
        public static readonly IndexOfMethod IndexOfMethod = new IndexOfMethod();
        public static readonly ReplaceMethod ReplaceMethod = new ReplaceMethod();
        public static readonly StartsWithMethod StartsWithMethod = new StartsWithMethod();
        public static readonly ToLowerMethod ToLowerMethod = new ToLowerMethod();
        public static readonly ToUpperMethod ToUpperMethod = new ToUpperMethod();
        public static readonly TrimMethod TrimMethod = new TrimMethod();
        public static readonly SubStringMethod SubStringMethod = new SubStringMethod();
        public static readonly SubStringOfMethod SubStringOfMethod = new SubStringOfMethod();
        public static readonly ConcatMethod ConcatMethod = new ConcatMethod();
        public static readonly LengthMethod LengthMethod = new LengthMethod();
        public static readonly YearMethod YearMethod = new YearMethod();
        public static readonly MonthMethod MonthMethod = new MonthMethod();
        public static readonly DayMethod DayMethod = new DayMethod();
        public static readonly HourMethod HourMethod = new HourMethod();
        public static readonly MinuteMethod MinuteMethod = new MinuteMethod();
        public static readonly SecondMethod SecondMethod = new SecondMethod();
        public static readonly RoundMethod RoundMethod = new RoundMethod();
        public static readonly FloorMethod FloorMethod = new FloorMethod();
        public static readonly CeilingMethod CeilingMethod = new CeilingMethod();
        public static readonly AnyMethod AnyMethod = new AnyMethod();
        public static readonly AllMethod AllMethod = new AllMethod();

        private static readonly Dictionary<MethodType, Method> _methods = new Dictionary<MethodType,Method>
        {
            { MethodType.IsOf, IsOfMethod },
            { MethodType.Cast, CastMethod },
            { MethodType.EndsWith, EndsWithMethod },
            { MethodType.IndexOf, IndexOfMethod },
            { MethodType.Replace, ReplaceMethod },
            { MethodType.StartsWith, StartsWithMethod },
            { MethodType.ToLower, ToLowerMethod },
            { MethodType.ToUpper, ToUpperMethod },
            { MethodType.Trim, TrimMethod },
            { MethodType.SubString, SubStringMethod },
            { MethodType.SubStringOf, SubStringOfMethod },
            { MethodType.Concat, ConcatMethod },
            { MethodType.Length, LengthMethod },
            { MethodType.Year, YearMethod },
            { MethodType.Month, MonthMethod },
            { MethodType.Day, DayMethod },
            { MethodType.Hour, HourMethod },
            { MethodType.Minute, MinuteMethod },
            { MethodType.Second, SecondMethod },
            { MethodType.Round, RoundMethod },
            { MethodType.Floor, FloorMethod },
            { MethodType.Ceiling, CeilingMethod },
            { MethodType.Any, AnyMethod },
            { MethodType.All, AllMethod }
        };

        private static readonly Dictionary<string, MethodType> _methodNames = CreateMethodNames();

        private static Dictionary<string, MethodType> CreateMethodNames()
        {
            var result = new Dictionary<string, MethodType>(StringComparer.OrdinalIgnoreCase);

            foreach (MethodType methodType in Enum.GetValues(typeof(MethodType)))
            {
                result.Add(methodType.ToString().ToLowerInvariant(), methodType);
            }

            return result;
        }

        public MethodType MethodType { get; private set; }
        public IList<ArgumentType> ArgumentTypes { get; private set; }
        public int ArgumentCount { get; private set; }
        public int MaxArgumentCount { get; private set; }

        protected Method(MethodType methodType, params ArgumentType[] argumentTypes)
        {
            Require.NotNull(argumentTypes, "argumentTypes");

            MethodType = methodType;
            ArgumentTypes = argumentTypes;

            foreach (var argumentType in argumentTypes)
            {
                MaxArgumentCount++;

                if (argumentType != ArgumentType.OptionalCommon)
                    ArgumentCount++;
            }
        }

        public static Method FindMethod(MethodType methodType)
        {
            Method method;

            _methods.TryGetValue(methodType, out method);

            return method;
        }

        public static Method FindMethodByName(string methodName)
        {
            Require.NotNull(methodName, "methodName");

            MethodType methodType;

            if (_methodNames.TryGetValue(methodName, out methodType))
                return _methods[methodType];

            return null;
        }

        public abstract TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg);
    }

    internal class IsOfMethod : Method
    {
        public IsOfMethod()
            : base(MethodType.IsOf, ArgumentType.Common, ArgumentType.StringLiteral)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.IsOfMethod(this, arg);
        }
    }

    internal class CastMethod : Method
    {
        public CastMethod()
            : base(MethodType.Cast, ArgumentType.Common, ArgumentType.StringLiteral)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.CastMethod(this, arg);
        }
    }

    internal class EndsWithMethod : Method
    {
        public EndsWithMethod()
            : base(MethodType.EndsWith, ArgumentType.Common, ArgumentType.Common)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.EndsWithMethod(this, arg);
        }
    }

    internal class IndexOfMethod : Method
    {
        public IndexOfMethod()
            : base(MethodType.IndexOf, ArgumentType.Common, ArgumentType.Common)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.IndexOfMethod(this, arg);
        }
    }

    internal class ReplaceMethod : Method
    {
        public ReplaceMethod()
            : base(MethodType.Replace, ArgumentType.Common, ArgumentType.Common, ArgumentType.Common)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.ReplaceMethod(this, arg);
        }
    }

    internal class StartsWithMethod : Method
    {
        public StartsWithMethod()
            : base(MethodType.StartsWith, ArgumentType.Common, ArgumentType.Common)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.StartsWithMethod(this, arg);
        }
    }

    internal class ToLowerMethod : Method
    {
        public ToLowerMethod()
            : base(MethodType.ToLower, ArgumentType.Common)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.ToLowerMethod(this, arg);
        }
    }

    internal class ToUpperMethod : Method
    {
        public ToUpperMethod()
            : base(MethodType.ToUpper, ArgumentType.Common)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.ToUpperMethod(this, arg);
        }
    }

    internal class TrimMethod : Method
    {
        public TrimMethod()
            : base(MethodType.Trim, ArgumentType.Common)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.TrimMethod(this, arg);
        }
    }

    internal class SubStringMethod : Method
    {
        public SubStringMethod()
            : base(MethodType.SubString, ArgumentType.Common, ArgumentType.Common, ArgumentType.OptionalCommon)
        {
            // Error in the spec at page 39: substring takes three parameters
            // with the third optional; not two with the second optional.
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.SubStringMethod(this, arg);
        }
    }

    internal class SubStringOfMethod : Method
    {
        public SubStringOfMethod()
            : base(MethodType.SubStringOf, ArgumentType.Common, ArgumentType.OptionalCommon)
        {
            // Spec states that the second parameter is optional. Normalization
            // will remove this method when the second parameter is omitted.
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.SubStringOfMethod(this, arg);
        }
    }

    internal class ConcatMethod : Method
    {
        public ConcatMethod()
            : base(MethodType.Concat, ArgumentType.Common, ArgumentType.OptionalCommon)
        {
            // Spec states that the second parameter is optional. Normalization
            // will remove this method when the second parameter is omitted.
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.ConcatMethod(this, arg);
        }
    }

    internal class LengthMethod : Method
    {
        public LengthMethod()
            : base(MethodType.Length, ArgumentType.Common)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.LengthMethod(this, arg);
        }
    }

    internal abstract class DatePartMethod : Method
    {
        protected DatePartMethod(MethodType methodType, params ArgumentType[] argumentTypes)
            : base(methodType, argumentTypes)
        {
        }
    }

    internal class YearMethod : DatePartMethod
    {
        public YearMethod()
            : base(MethodType.Year, ArgumentType.Common)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.YearMethod(this, arg);
        }
    }

    internal class MonthMethod : DatePartMethod
    {
        public MonthMethod()
            : base(MethodType.Month, ArgumentType.Common)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.MonthMethod(this, arg);
        }
    }

    internal class DayMethod : DatePartMethod
    {
        public DayMethod()
            : base(MethodType.Day, ArgumentType.Common)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.DayMethod(this, arg);
        }
    }

    internal class HourMethod : DatePartMethod
    {
        public HourMethod()
            : base(MethodType.Hour, ArgumentType.Common)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.HourMethod(this, arg);
        }
    }

    internal class MinuteMethod : DatePartMethod
    {
        public MinuteMethod()
            : base(MethodType.Minute, ArgumentType.Common)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.MinuteMethod(this, arg);
        }
    }

    internal class SecondMethod : DatePartMethod
    {
        public SecondMethod()
            : base(MethodType.Second, ArgumentType.Common)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.SecondMethod(this, arg);
        }
    }

    internal abstract class FloatingPointMethod : Method
    {
        protected FloatingPointMethod(MethodType methodType, params ArgumentType[] argumentTypes)
            : base(methodType, argumentTypes)
        {
        }
    }

    internal class RoundMethod : FloatingPointMethod
    {
        public RoundMethod()
            : base(MethodType.Round, ArgumentType.Common)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.RoundMethod(this, arg);
        }
    }

    internal class FloorMethod : FloatingPointMethod
    {
        public FloorMethod()
            : base(MethodType.Floor, ArgumentType.Common)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.FloorMethod(this, arg);
        }
    }

    internal class CeilingMethod : FloatingPointMethod
    {
        public CeilingMethod()
            : base(MethodType.Ceiling, ArgumentType.Common)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.CeilingMethod(this, arg);
        }
    }

    internal abstract class CollectionMethod : Method
    {
        protected CollectionMethod(MethodType methodType, params ArgumentType[] argumentTypes)
            : base(methodType, argumentTypes)
        {
        }
    }

    internal class AnyMethod : CollectionMethod
    {
        public AnyMethod()
            : base(MethodType.Any, ArgumentType.Common, ArgumentType.OptionalCommon)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.AnyMethod(this, arg);
        }
    }

    internal class AllMethod : CollectionMethod
    {
        public AllMethod()
            : base(MethodType.All, ArgumentType.Common, ArgumentType.Common)
        {
        }

        public override TResult Visit<TResult, TArg>(IMethodVisitor<TResult, TArg> visitor, TArg arg)
        {
            return visitor.AllMethod(this, arg);
        }
    }
}
