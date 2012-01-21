using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal class NormalizeMethodVisitor : IMethodVisitor<Expression, LiteralExpression[]>
    {
        private static readonly NormalizeMethodVisitor _instance = new NormalizeMethodVisitor();

        private NormalizeMethodVisitor()
        {
        }

        public static Expression Normalize(Method method, LiteralExpression[] arguments)
        {
            return method.Visit(_instance, arguments);
        }

        public Expression IsOfMethod(IsOfMethod method, LiteralExpression[] arguments)
        {
            Debug.Assert(arguments[1].LiteralType == LiteralType.String);

            var type = LiteralUtil.GetCompatibleType((string)arguments[1].Value);

            return new LiteralExpression(type.IsInstanceOfType(arguments[0].Value), LiteralType.Boolean);
        }

        public Expression CastMethod(CastMethod method, LiteralExpression[] arguments)
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

        public Expression EndsWithMethod(EndsWithMethod method, LiteralExpression[] arguments)
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

        public Expression IndexOfMethod(IndexOfMethod method, LiteralExpression[] arguments)
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

            if (result == -1)
                return new LiteralExpression(null, LiteralType.Null);
            else
                return new LiteralExpression(result + 1, LiteralType.Int);
        }

        public Expression ReplaceMethod(ReplaceMethod method, LiteralExpression[] arguments)
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

        public Expression StartsWithMethod(StartsWithMethod method, LiteralExpression[] arguments)
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

        public Expression ToLowerMethod(ToLowerMethod method, LiteralExpression[] arguments)
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

        public Expression ToUpperMethod(ToUpperMethod method, LiteralExpression[] arguments)
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

        public Expression TrimMethod(TrimMethod method, LiteralExpression[] arguments)
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

        public Expression SubStringMethod(SubStringMethod method, LiteralExpression[] arguments)
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
                        method.MethodType, 2, "Edm.Int32"
                    ));
                }

                if (arguments.Length == 3)
                {
                    if (!LiteralUtil.TryCoerceInt(arguments[2], out length))
                    {
                        throw new ODataException(String.Format(
                            ErrorMessages.Method_InvalidArgumentType,
                            method.MethodType, 3, "Edm.Int32"
                        ));
                    }

                    result = LiteralUtil.CoerceString(arguments[0]).Substring(startIndex - 1, length);
                }
                else
                {
                    result = LiteralUtil.CoerceString(arguments[0]).Substring(startIndex - 1);
                }

                return new LiteralExpression(result, LiteralType.String);
            }
        }

        public Expression SubStringOfMethod(SubStringOfMethod method, LiteralExpression[] arguments)
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

        public Expression ConcatMethod(ConcatMethod method, LiteralExpression[] arguments)
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

        public Expression LengthMethod(LengthMethod method, LiteralExpression[] arguments)
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

        private Expression NormalizeDatePart(DatePartMethod method, LiteralExpression[] arguments)
        {
            if (LiteralUtil.IsAnyNull(arguments))
            {
                return new LiteralExpression(null, LiteralType.Null);
            }
            else if (arguments[0].LiteralType != LiteralType.DateTime)
            {
                throw new ODataException(String.Format(
                    ErrorMessages.Method_InvalidArgumentType,
                    method.MethodType, 1, "Edm.DateTime"
                ));
            }
            else
            {
                var argument = (DateTime)arguments[0].Value;
                int result;

                switch (method.MethodType)
                {
                    case MethodType.Year: result = argument.Year; break;
                    case MethodType.Month: result = argument.Month; break;
                    case MethodType.Day: result = argument.Day; break;
                    case MethodType.Hour: result = argument.Hour; break;
                    case MethodType.Minute: result = argument.Minute; break;
                    case MethodType.Second: result = argument.Second; break;
                    default: throw new NotSupportedException();
                }

                return new LiteralExpression(result, LiteralType.Int);
            }
        }

        public Expression YearMethod(YearMethod method, LiteralExpression[] arguments)
        {
            return NormalizeDatePart(method, arguments);
        }

        public Expression MonthMethod(MonthMethod method, LiteralExpression[] arguments)
        {
            return NormalizeDatePart(method, arguments);
        }

        public Expression DayMethod(DayMethod method, LiteralExpression[] arguments)
        {
            return NormalizeDatePart(method, arguments);
        }

        public Expression HourMethod(HourMethod method, LiteralExpression[] arguments)
        {
            return NormalizeDatePart(method, arguments);
        }

        public Expression MinuteMethod(MinuteMethod method, LiteralExpression[] arguments)
        {
            return NormalizeDatePart(method, arguments);
        }

        public Expression SecondMethod(SecondMethod method, LiteralExpression[] arguments)
        {
            return NormalizeDatePart(method, arguments);
        }

        private Expression NormalizeFloatingPoint(FloatingPointMethod method, LiteralExpression[] arguments)
        {
            object result;

            switch (arguments[0].LiteralType)
            {
                case LiteralType.Null:
                case LiteralType.Int:
                case LiteralType.Long:
                    return arguments[0];

                case LiteralType.Decimal:
                    switch (method.MethodType)
                    {
                        case MethodType.Round: result = Math.Round((decimal)arguments[0].Value); break;
                        case MethodType.Floor: result = Math.Floor((decimal)arguments[0].Value); break;
                        case MethodType.Ceiling: result = Math.Ceiling((decimal)arguments[0].Value); break;
                        default: throw new NotSupportedException();
                    }
                    break;

                case LiteralType.Double:
                    switch (method.MethodType)
                    {
                        case MethodType.Round: result = Math.Round((double)arguments[0].Value); break;
                        case MethodType.Floor: result = Math.Floor((double)arguments[0].Value); break;
                        case MethodType.Ceiling: result = Math.Ceiling((double)arguments[0].Value); break;
                        default: throw new NotSupportedException();
                    }
                    break;

                case LiteralType.Single:
                    switch (method.MethodType)
                    {
                        case MethodType.Round: result = (float)Math.Round((float)arguments[0].Value); break;
                        case MethodType.Floor: result = (float)Math.Floor((float)arguments[0].Value); break;
                        case MethodType.Ceiling: result = (float)Math.Ceiling((float)arguments[0].Value); break;
                        default: throw new NotSupportedException();
                    }
                    break;

                default:
                    throw new ODataException(String.Format(
                        ErrorMessages.Method_InvalidArgumentType,
                        method.MethodType, 1, "Edm.Double"
                    ));
            }

            return new LiteralExpression(result, arguments[0].LiteralType);
        }

        public Expression RoundMethod(RoundMethod method, LiteralExpression[] arguments)
        {
            return NormalizeFloatingPoint(method, arguments);
        }

        public Expression FloorMethod(FloorMethod method, LiteralExpression[] arguments)
        {
            return NormalizeFloatingPoint(method, arguments);
        }

        public Expression CeilingMethod(CeilingMethod method, LiteralExpression[] arguments)
        {
            return NormalizeFloatingPoint(method, arguments);
        }
    }
}
