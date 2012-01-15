using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            { MethodType.Ceiling, CeilingMethod }
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
            if (argumentTypes == null)
                throw new ArgumentNullException("argumentTypes");

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
            if (methodName == null)
                throw new ArgumentNullException("methodName");

            MethodType methodType;

            if (_methodNames.TryGetValue(methodName, out methodType))
                return _methods[methodType];

            return null;
        }
    }

    internal class IsOfMethod : Method
    {
        public IsOfMethod()
            : base(MethodType.IsOf, ArgumentType.Common, ArgumentType.StringLiteral)
        {
        }
    }

    internal class CastMethod : Method
    {
        public CastMethod()
            : base(MethodType.Cast, ArgumentType.Common, ArgumentType.StringLiteral)
        {
        }
    }

    internal class EndsWithMethod : Method
    {
        public EndsWithMethod()
            : base(MethodType.EndsWith, ArgumentType.Common, ArgumentType.Common)
        {
        }
    }

    internal class IndexOfMethod : Method
    {
        public IndexOfMethod()
            : base(MethodType.IndexOf, ArgumentType.Common, ArgumentType.Common)
        {
        }
    }

    internal class ReplaceMethod : Method
    {
        public ReplaceMethod()
            : base(MethodType.Replace, ArgumentType.Common, ArgumentType.Common, ArgumentType.Common)
        {
        }
    }

    internal class StartsWithMethod : Method
    {
        public StartsWithMethod()
            : base(MethodType.StartsWith, ArgumentType.Common, ArgumentType.Common)
        {
        }
    }

    internal class ToLowerMethod : Method
    {
        public ToLowerMethod()
            : base(MethodType.ToLower, ArgumentType.Common)
        {
        }
    }

    internal class ToUpperMethod : Method
    {
        public ToUpperMethod()
            : base(MethodType.ToUpper, ArgumentType.Common)
        {
        }
    }

    internal class TrimMethod : Method
    {
        public TrimMethod()
            : base(MethodType.Trim, ArgumentType.Common)
        {
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
    }

    internal class SubStringOfMethod : Method
    {
        public SubStringOfMethod()
            : base(MethodType.SubStringOf, ArgumentType.Common, ArgumentType.OptionalCommon)
        {
            // Spec states that the second parameter is optional. Normalization
            // will remove this method when the second parameter is omitted.
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
    }

    internal class LengthMethod : Method
    {
        public LengthMethod()
            : base(MethodType.Length, ArgumentType.Common)
        {
        }
    }

    internal class YearMethod : Method
    {
        public YearMethod()
            : base(MethodType.Year, ArgumentType.Common)
        {
        }
    }

    internal class MonthMethod : Method
    {
        public MonthMethod()
            : base(MethodType.Month, ArgumentType.Common)
        {
        }
    }

    internal class DayMethod : Method
    {
        public DayMethod()
            : base(MethodType.Day, ArgumentType.Common)
        {
        }
    }

    internal class HourMethod : Method
    {
        public HourMethod()
            : base(MethodType.Hour, ArgumentType.Common)
        {
        }
    }

    internal class MinuteMethod : Method
    {
        public MinuteMethod()
            : base(MethodType.Minute, ArgumentType.Common)
        {
        }
    }

    internal class SecondMethod : Method
    {
        public SecondMethod()
            : base(MethodType.Second, ArgumentType.Common)
        {
        }
    }

    internal class RoundMethod : Method
    {
        public RoundMethod()
            : base(MethodType.Round, ArgumentType.Common)
        {
        }
    }

    internal class FloorMethod : Method
    {
        public FloorMethod()
            : base(MethodType.Floor, ArgumentType.Common)
        {
        }
    }

    internal class CeilingMethod : Method
    {
        public CeilingMethod()
            : base(MethodType.Ceiling, ArgumentType.Common)
        {
        }
    }
}
