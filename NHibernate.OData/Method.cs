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

        public abstract Expression Normalize(LiteralExpression[] arguments);
    }

    internal class IsOfMethod : Method
    {
        public IsOfMethod()
            : base(MethodType.IsOf, ArgumentType.Common, ArgumentType.StringLiteral)
        {
        }

        public override Expression Normalize(LiteralExpression[] arguments)
        {
            Debug.Assert(arguments[1].LiteralType == LiteralType.String);

            var type = LiteralUtil.GetCompatibleType((string)arguments[1].Value);

            return new LiteralExpression(type.IsInstanceOfType(arguments[0].Value), LiteralType.Boolean);
        }
    }

    internal class CastMethod : Method
    {
        public CastMethod()
            : base(MethodType.Cast, ArgumentType.Common, ArgumentType.StringLiteral)
        {
        }

        public override Expression Normalize(LiteralExpression[] arguments)
        {
            Debug.Assert(arguments[1].LiteralType == LiteralType.String);

            if (arguments[0].LiteralType == LiteralType.Null)
                return arguments[0];

            var type = LiteralUtil.GetCompatibleType((string)arguments[1].Value);

            try
            {
                return new LiteralExpression(Convert.ChangeType(arguments[0].Value, type, CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                throw new ODataException(
                    String.Format(
                        ErrorMessages.Method_CannotCast, arguments[1].Value
                    ),
                    ex
                );
            }
        }
    }

    internal class EndsWithMethod : Method
    {
        public EndsWithMethod()
            : base(MethodType.EndsWith, ArgumentType.Common, ArgumentType.Common)
        {
        }

        public override Expression Normalize(LiteralExpression[] arguments)
        {
            bool result;

            if (LiteralUtil.IsAnyNull(arguments))
            {
                result = false;
            }
            else
            {
                result = LiteralUtil.CoerceString(arguments[0]).EndsWith(
                    LiteralUtil.CoerceString(arguments[1]),
                    StringComparison.InvariantCultureIgnoreCase
                );
            }

            return new LiteralExpression(result, LiteralType.Boolean);
        }
    }

    internal class IndexOfMethod : Method
    {
        public IndexOfMethod()
            : base(MethodType.IndexOf, ArgumentType.Common, ArgumentType.Common)
        {
        }

        public override Expression Normalize(LiteralExpression[] arguments)
        {
            int result;

            if (LiteralUtil.IsAnyNull(arguments))
            {
                result = -1;
            }
            else
            {
                result = LiteralUtil.CoerceString(arguments[0]).IndexOf(
                    LiteralUtil.CoerceString(arguments[1]),
                    StringComparison.InvariantCultureIgnoreCase
                );
            }

            return new LiteralExpression(result, LiteralType.Int);
        }
    }

    internal class ReplaceMethod : Method
    {
        public ReplaceMethod()
            : base(MethodType.Replace, ArgumentType.Common, ArgumentType.Common, ArgumentType.Common)
        {
        }

        public override Expression Normalize(LiteralExpression[] arguments)
        {
            if (LiteralUtil.IsAnyNull(arguments))
            {
                return new LiteralExpression(null, LiteralType.Null);
            }
            else
            {
                string result = LiteralUtil.CoerceString(arguments[0]).Replace(
                    LiteralUtil.CoerceString(arguments[1]),
                    LiteralUtil.CoerceString(arguments[2])
                );

                return new LiteralExpression(result, LiteralType.String);
            }
        }
    }

    internal class StartsWithMethod : Method
    {
        public StartsWithMethod()
            : base(MethodType.StartsWith, ArgumentType.Common, ArgumentType.Common)
        {
        }

        public override Expression Normalize(LiteralExpression[] arguments)
        {
            bool result;

            if (LiteralUtil.IsAnyNull(arguments))
            {
                result = false;
            }
            else
            {
                result = LiteralUtil.CoerceString(arguments[0]).StartsWith(
                    LiteralUtil.CoerceString(arguments[1]),
                    StringComparison.InvariantCultureIgnoreCase
                );
            }

            return new LiteralExpression(result, LiteralType.Boolean);
        }
    }

    internal class ToLowerMethod : Method
    {
        public ToLowerMethod()
            : base(MethodType.ToLower, ArgumentType.Common)
        {
        }

        public override Expression Normalize(LiteralExpression[] arguments)
        {
            if (LiteralUtil.IsAnyNull(arguments))
            {
                return new LiteralExpression(null, LiteralType.Null);
            }
            else
            {
                string result = LiteralUtil.CoerceString(arguments[0]).ToLowerInvariant();

                return new LiteralExpression(result, LiteralType.String);
            }
        }
    }

    internal class ToUpperMethod : Method
    {
        public ToUpperMethod()
            : base(MethodType.ToUpper, ArgumentType.Common)
        {
        }

        public override Expression Normalize(LiteralExpression[] arguments)
        {
            if (LiteralUtil.IsAnyNull(arguments))
            {
                return new LiteralExpression(null, LiteralType.Null);
            }
            else
            {
                string result = LiteralUtil.CoerceString(arguments[0]).ToUpperInvariant();

                return new LiteralExpression(result, LiteralType.String);
            }
        }
    }

    internal class TrimMethod : Method
    {
        public TrimMethod()
            : base(MethodType.Trim, ArgumentType.Common)
        {
        }

        public override Expression Normalize(LiteralExpression[] arguments)
        {
            if (LiteralUtil.IsAnyNull(arguments))
            {
                return new LiteralExpression(null, LiteralType.Null);
            }
            else
            {
                string result = LiteralUtil.CoerceString(arguments[0]).Trim();

                return new LiteralExpression(result, LiteralType.String);
            }
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

        public override Expression Normalize(LiteralExpression[] arguments)
        {
            if (arguments[0].LiteralType == LiteralType.Null)
            {
                return arguments[0];
            }
            else
            {
                int startIndex;
                int length;
                string result;

                if (!LiteralUtil.TryCoerceInt(arguments[1], out startIndex))
                {
                    throw new ODataException(String.Format(
                        ErrorMessages.Method_InvalidArgumentType,
                        MethodType, 2, "Edm.Int32"
                    ));
                }

                if (arguments.Length == 3)
                {
                    if (!LiteralUtil.TryCoerceInt(arguments[2], out length))
                    {
                        throw new ODataException(String.Format(
                            ErrorMessages.Method_InvalidArgumentType,
                            MethodType, 3, "Edm.Int32"
                        ));
                    }

                    result = LiteralUtil.CoerceString(arguments[0]).Substring(startIndex, length);
                }
                else
                {
                    result = LiteralUtil.CoerceString(arguments[0]).Substring(startIndex);
                }

                return new LiteralExpression(result, LiteralType.String);
            }
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

        public override Expression Normalize(LiteralExpression[] arguments)
        {
            if (arguments.Length == 1)
            {
                return arguments[0];
            }
            else
            {
                bool result;

                if (LiteralUtil.IsAnyNull(arguments))
                {
                    result = false;
                }
                else
                {
                    result = LiteralUtil.CoerceString(arguments[0]).IndexOf(
                        LiteralUtil.CoerceString(arguments[1]),
                        StringComparison.InvariantCultureIgnoreCase
                    ) != -1;
                }

                return new LiteralExpression(result, LiteralType.Boolean);
            }
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

        public override Expression Normalize(LiteralExpression[] arguments)
        {
            if (arguments.Length == 1)
            {
                return arguments[0];
            }
            else if (arguments[0].LiteralType == LiteralType.Null)
            {
                if (arguments[1].LiteralType == LiteralType.Null)
                    return new LiteralExpression(null, LiteralType.Null);
                else
                    return arguments[1];
            }
            else if (arguments[1].LiteralType == LiteralType.Null)
            {
                return arguments[0];
            }
            else
            {
                string result =
                    LiteralUtil.CoerceString(arguments[0]) +
                    LiteralUtil.CoerceString(arguments[1]);

                return new LiteralExpression(result, LiteralType.String);
            }
        }
    }

    internal class LengthMethod : Method
    {
        public LengthMethod()
            : base(MethodType.Length, ArgumentType.Common)
        {
        }

        public override Expression Normalize(LiteralExpression[] arguments)
        {
            if (LiteralUtil.IsAnyNull(arguments))
            {
                return new LiteralExpression(null, LiteralType.Null);
            }
            else
            {
                int result = LiteralUtil.CoerceString(arguments[0]).Length;

                return new LiteralExpression(result, LiteralType.Int);
            }
        }
    }

    internal abstract class DatePartMethod : Method
    {
        protected DatePartMethod(MethodType methodType, params ArgumentType[] argumentTypes)
            : base(methodType, argumentTypes)
        {
        }

        public override Expression Normalize(LiteralExpression[] arguments)
        {
            if (LiteralUtil.IsAnyNull(arguments))
            {
                return new LiteralExpression(null, LiteralType.Null);
            }
            else if (arguments[0].LiteralType != LiteralType.DateTime)
            {
                throw new ODataException(String.Format(
                    ErrorMessages.Method_InvalidArgumentType,
                    MethodType, 1, "Edm.DateTime"
                ));
            }
            else
            {
                int result = GetDatePart((DateTime)arguments[0].Value);

                return new LiteralExpression(result, LiteralType.Int);
            }
        }

        protected abstract int GetDatePart(DateTime value);
    }

    internal class YearMethod : DatePartMethod
    {
        public YearMethod()
            : base(MethodType.Year, ArgumentType.Common)
        {
        }

        protected override int GetDatePart(DateTime value)
        {
            return value.Year;
        }
    }

    internal class MonthMethod : DatePartMethod
    {
        public MonthMethod()
            : base(MethodType.Month, ArgumentType.Common)
        {
        }

        protected override int GetDatePart(DateTime value)
        {
            return value.Month;
        }
    }

    internal class DayMethod : DatePartMethod
    {
        public DayMethod()
            : base(MethodType.Day, ArgumentType.Common)
        {
        }

        protected override int GetDatePart(DateTime value)
        {
            return value.Day;
        }
    }

    internal class HourMethod : DatePartMethod
    {
        public HourMethod()
            : base(MethodType.Hour, ArgumentType.Common)
        {
        }

        protected override int GetDatePart(DateTime value)
        {
            return value.Hour;
        }
    }

    internal class MinuteMethod : DatePartMethod
    {
        public MinuteMethod()
            : base(MethodType.Minute, ArgumentType.Common)
        {
        }

        protected override int GetDatePart(DateTime value)
        {
            return value.Minute;
        }
    }

    internal class SecondMethod : DatePartMethod
    {
        public SecondMethod()
            : base(MethodType.Second, ArgumentType.Common)
        {
        }

        protected override int GetDatePart(DateTime value)
        {
            return value.Second;
        }
    }

    internal abstract class FloatingPointMethod : Method
    {
        protected FloatingPointMethod(MethodType methodType, params ArgumentType[] argumentTypes)
            : base(methodType, argumentTypes)
        {
        }

        public override Expression Normalize(LiteralExpression[] arguments)
        {
            object result;

            switch (arguments[0].LiteralType)
            {
                case LiteralType.Null:
                case LiteralType.Int:
                case LiteralType.Long:
                    return arguments[0];

                case LiteralType.Decimal:
                    result = Perform((decimal)arguments[0].Value);
                    break;

                case LiteralType.Double:
                    result = Perform((double)arguments[0].Value);
                    break;

                case LiteralType.Single:
                    result = Perform((float)arguments[0].Value);
                    break;

                default:
                    throw new ODataException(String.Format(
                        ErrorMessages.Method_InvalidArgumentType,
                        MethodType, 1, "Edm.Double"
                    ));
            }

            return new LiteralExpression(result, arguments[0].LiteralType);
        }

        protected abstract decimal Perform(decimal value);

        protected abstract double Perform(double value);

        protected abstract float Perform(float value);
    }

    internal class RoundMethod : FloatingPointMethod
    {
        public RoundMethod()
            : base(MethodType.Round, ArgumentType.Common)
        {
        }

        protected override decimal Perform(decimal value)
        {
            return Math.Round(value);
        }

        protected override double Perform(double value)
        {
            return Math.Round(value);
        }

        protected override float Perform(float value)
        {
            return (float)Math.Round(value);
        }
    }

    internal class FloorMethod : FloatingPointMethod
    {
        public FloorMethod()
            : base(MethodType.Floor, ArgumentType.Common)
        {
        }

        protected override decimal Perform(decimal value)
        {
            return Math.Floor(value);
        }

        protected override double Perform(double value)
        {
            return Math.Floor(value);
        }

        protected override float Perform(float value)
        {
            return (float)Math.Floor(value);
        }
    }

    internal class CeilingMethod : FloatingPointMethod
    {
        public CeilingMethod()
            : base(MethodType.Ceiling, ArgumentType.Common)
        {
        }

        protected override decimal Perform(decimal value)
        {
            return Math.Ceiling(value);
        }

        protected override double Perform(double value)
        {
            return Math.Ceiling(value);
        }

        protected override float Perform(float value)
        {
            return (float)Math.Ceiling(value);
        }
    }
}
