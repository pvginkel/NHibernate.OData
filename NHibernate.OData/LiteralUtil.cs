using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal static class LiteralUtil
    {
        private static readonly Dictionary<string, System.Type> _edmTypes = new Dictionary<string,System.Type>
        {
            { "Edm.Binary", typeof(byte[]) },
            { "Edm.Boolean", typeof(bool) },
            { "Edm.Byte", typeof(byte) },
            { "Edm.DateTime", typeof(DateTime) },
            { "Edm.Decimal", typeof(decimal) },
            { "Edm.Double", typeof(double) },
            { "Edm.Single", typeof(float) },
            { "Edm.Guid", typeof(Guid) },
            { "Edm.Int16", typeof(short) },
            { "Edm.Int32", typeof(int) },
            { "Edm.Int64", typeof(long) },
            { "Edm.SByte", typeof(sbyte) },
            { "Edm.String", typeof(string) },
            { "Edm.Time", typeof(XmlTimeSpan) },
            { "Edm.DateTimeOffset", typeof(DateTime) }
        };

        public static LiteralType GetLiteralType(object value)
        {
            if (value == null)
                return LiteralType.Null;
            else if (value is string)
                return LiteralType.String;
            else if (value is bool)
                return LiteralType.Boolean;
            else if (value is float)
                return LiteralType.Single;
            else if (value is double)
                return LiteralType.Double;
            else if (value is decimal)
                return LiteralType.Decimal;
            else if (value is int)
                return LiteralType.Int;
            else if (value is long)
                return LiteralType.Long;
            else if (value is byte[])
                return LiteralType.Binary;
            else if (value is DateTime)
                return LiteralType.DateTime;
            else if (value is Guid)
                return LiteralType.Guid;
            else if (value is XmlTimeSpan)
                return LiteralType.Duration;
            else
                throw new NotSupportedException();
        }

        public static LiteralType CoerceLiteralValues(ref object left, LiteralType leftType, ref object right, LiteralType rightType)
        {
            if (leftType == rightType)
                return leftType;

            if (leftType != LiteralType.Null && rightType != LiteralType.Null)
            {
                if (leftType == LiteralType.String || rightType == LiteralType.String)
                {
                    CoerceString(ref left, leftType);
                    CoerceString(ref right, rightType);

                    return LiteralType.String;
                }
                else if (IsInteger(leftType) && IsInteger(rightType))
                {
                    CoerceLong(ref left, leftType);
                    CoerceLong(ref right, rightType);

                    return LiteralType.Long;
                }
                else if (IsFloatingOrInteger(leftType) && IsFloatingOrInteger(rightType))
                {
                    if (leftType == LiteralType.Decimal || rightType == LiteralType.Decimal)
                    {
                        CoerceDecimal(ref left, leftType);
                        CoerceDecimal(ref right, rightType);

                        return LiteralType.Decimal;
                    }
                    else
                    {
                        CoerceDouble(ref left, leftType);
                        CoerceDouble(ref right, rightType);

                        return LiteralType.Double;
                    }
                }
            }

            throw new ODataException(String.Format(
                ErrorMessages.LiteralUtil_IncompatibleTypes,
                leftType, rightType
            ));
        }

        private static void CoerceString(ref object value, LiteralType type)
        {
            if (type != LiteralType.String)
                value = value.ToString();
        }

        private static void CoerceDecimal(ref object value, LiteralType type)
        {
            switch (type)
            {
                case LiteralType.Long:
                    value = (decimal)(long)value;
                    break;

                case LiteralType.Int:
                    value = (decimal)(int)value;
                    break;

                case LiteralType.Single:
                    value = (decimal)(float)value;
                    break;

                case LiteralType.Double:
                    value = (decimal)(double)value;
                    break;

                case LiteralType.Decimal:
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private static void CoerceDouble(ref object value, LiteralType type)
        {
            switch (type)
            {
                case LiteralType.Long:
                    value = (double)(long)value;
                    break;

                case LiteralType.Int:
                    value = (double)(int)value;
                    break;

                case LiteralType.Single:
                    value = (double)(float)value;
                    break;

                case LiteralType.Double:
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private static void CoerceLong(ref object value, LiteralType type)
        {
            switch (type)
            {
                case LiteralType.Long:
                    break;

                case LiteralType.Int:
                    value = (long)(int)value;
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private static bool IsFloatingOrInteger(LiteralType type)
        {
            return IsFloating(type) || IsInteger(type);
        }

        private static bool IsInteger(LiteralType type)
        {
            return type == LiteralType.Long || type == LiteralType.Int;
        }

        private static bool IsFloating(LiteralType type)
        {
            return type == LiteralType.Single || type == LiteralType.Double || type == LiteralType.Decimal;
        }

        public static bool ByteArrayEquals(byte[] a, byte[] b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }

        public static System.Type GetCompatibleType(string edmType)
        {
            if (edmType == null)
                throw new ArgumentNullException("edmType");

            System.Type result;

            if (!_edmTypes.TryGetValue(edmType, out result))
            {
                throw new ODataException(String.Format(
                    ErrorMessages.LiteralUtil_UnknownEdmType, edmType
                ));
            }

            return result;
        }

        public static string CoerceString(LiteralExpression literal)
        {
            if (literal.LiteralType == LiteralType.String)
                return (string)literal.Value;
            else
                return literal.Value.ToString();
        }

        public static bool IsAnyNull(IEnumerable<LiteralExpression> expressions)
        {
            foreach (var expression in expressions)
            {
                if (expression.LiteralType == LiteralType.Null)
                    return true;
            }

            return false;
        }

        internal static bool TryCoerceInt(LiteralExpression expression, out int result)
        {
            switch (expression.LiteralType)
            {
                case LiteralType.Int:
                    result = (int)expression.Value;
                    return true;

                case LiteralType.Long:
                    result = (int)(long)expression.Value;
                    return true;

                default:
                    result = 0;
                    return false;
            }
        }
    }
}
