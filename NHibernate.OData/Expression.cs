using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

// The Equals are only here for unit testing. GetHashCode has not been implemented.

#pragma warning disable 659

namespace NHibernate.OData
{
    internal abstract class Expression
    {
        public abstract bool IsBool { get; }

        public ExpressionType Type { get; private set; }

        protected Expression(ExpressionType type)
        {
            Type = type;
        }

        public abstract Expression Normalize();
    }

    internal class LiteralExpression : Expression
    {
        public object Value { get; private set; }

        public LiteralType LiteralType { get; private set; }

        public override bool IsBool
        {
            get { return LiteralType == LiteralType.Boolean; }
        }

        public LiteralExpression(object value)
            : this(value, LiteralUtil.GetLiteralType(value))
        {
            // This overload is here just for unit testing.
        }

        public LiteralExpression(object value, LiteralType literalType)
            : base(ExpressionType.Literal)
        {
            Value = value;
            LiteralType = literalType;

            Debug.Assert(literalType == LiteralUtil.GetLiteralType(value));
        }

        public override Expression Normalize()
        {
            return this;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as LiteralExpression;

            return
                other != null &&
                Equals(Value, other.Value);
        }

        public override string ToString()
        {
            return Value == null ? "null" : Value.ToString();
        }
    }

    internal class MemberExpression : Expression
    {
        public MemberType MemberType { get; private set; }
        public IList<string> Members { get; private set; }

        public override bool IsBool
        {
            get { return MemberType == MemberType.Boolean; }
        }

        public MemberExpression(MemberType type, params string[] members)
            : this(type, (IList<string>)members)
        {
        }

        public MemberExpression(MemberType type, IList<string> members)
            : base(ExpressionType.Member)
        {
            Require.NotNull(members, "members");

            Debug.Assert(members.Count > 0);

            MemberType = type;
            Members = members;
        }

        public override Expression Normalize()
        {
            return this;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as MemberExpression;

            if (
                other == null ||
                MemberType != other.MemberType ||
                Members.Count != other.Members.Count
            )
                return false;

            for (int i = 0; i < Members.Count; i++)
            {
                if (Members[i] != other.Members[i])
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            return String.Join("/", Members.ToArray());
        }
    }

    internal enum MemberType
    {
        Normal,
        Boolean
    }

    internal class ParenExpression : Expression
    {
        public Expression Expression { get; private set; }

        public override bool IsBool
        {
            get { return Expression.IsBool; }
        }

        public ParenExpression(Expression expression)
            : base(ExpressionType.Paren)
        {
            Require.NotNull(expression, "expression");

            Expression = expression;
        }

        public override Expression Normalize()
        {
            return Expression;
        }

        public override string ToString()
        {
            return "(" + Expression + ")";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as ParenExpression;

            return
                other != null &&
                Expression.Equals(other.Expression);
        }
    }

    internal abstract class OperatorExpression : Expression
    {
        public Operator Operator { get; private set; }

        protected OperatorExpression(ExpressionType type, Operator @operator)
            : base(type)
        {
            Operator = @operator;
        }
    }

    internal abstract class UnaryExpression : OperatorExpression
    {
        public Expression Expression { get; private set; }

        protected UnaryExpression(ExpressionType type, Expression expression, Operator @operator)
            : base(type, @operator)
        {
            Require.NotNull(expression, "expression");

            Expression = expression;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as UnaryExpression;

            return
                other != null &&
                GetType() == other.GetType() &&
                Operator == other.Operator &&
                Expression.Equals(other.Expression);
        }

        public override string ToString()
        {
            return Operator + " " + Expression;
        }
    }

    internal class BoolUnaryExpression : UnaryExpression
    {
        public override bool IsBool
        {
            get { return true; }
        }

        public BoolUnaryExpression(Operator @operator, Expression expression)
            : base(ExpressionType.Bool, ExpressionUtil.CoerceBoolExpression(expression), @operator)
        {
        }

        public override Expression Normalize()
        {
            var expression = Expression.Normalize();

            var literal = expression as LiteralExpression;

            if (literal != null)
                return ResolveLiteral(literal);

            return new BoolUnaryExpression(Operator, expression);
        }

        private Expression ResolveLiteral(LiteralExpression literal)
        {
            if (Operator != Operator.Not)
                throw new NotSupportedException();

            // CoerceBoolExpression takes care of the type of the literal.

            Debug.Assert(literal.LiteralType == LiteralType.Boolean);

            return new LiteralExpression(!(bool)literal.Value, LiteralType.Boolean);
        }
    }

    internal class ArithmicUnaryExpression : UnaryExpression
    {
        public override bool IsBool
        {
            get { return false; }
        }

        public ArithmicUnaryExpression(Operator @operator, Expression expression)
            : base(ExpressionType.ArithmicUnary, expression, @operator)
        {
        }

        public override Expression Normalize()
        {
            var expression = Expression.Normalize();

            var literal = expression as LiteralExpression;

            if (literal != null)
                return ResolveLiteral(literal);

            return new ArithmicUnaryExpression(Operator.Negative, expression);
        }

        private Expression ResolveLiteral(LiteralExpression literal)
        {
            if (Operator != Operator.Negative)
                throw new NotSupportedException();

            object value = literal.Value;

            switch (literal.LiteralType)
            {
                case LiteralType.Decimal:
                    value = -((decimal)value);
                    break;

                case LiteralType.Double:
                    value = -((double)value);
                    break;

                case LiteralType.Duration:
                    value = -((XmlTimeSpan)value);
                    break;

                case LiteralType.Int:
                    value = -((int)value);
                    break;

                case LiteralType.Long:
                    value = -((long)value);
                    break;

                case LiteralType.Single:
                    value = -((float)value);
                    break;

                default:
                    throw new ODataException(String.Format(
                        ErrorMessages.Expression_CannotNegate, literal.LiteralType
                    ));
            }

            return new LiteralExpression(value, literal.LiteralType);
        }
    }

    internal abstract class BinaryExpression : OperatorExpression
    {
        public Expression Left { get; private set; }

        public Expression Right { get; private set; }

        protected BinaryExpression(ExpressionType type, Operator @operator, Expression left, Expression right)
            : base(type, @operator)
        {
            Require.NotNull(left, "left");
            Require.NotNull(right, "right");

            Left = left;
            Right = right;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as BinaryExpression;

            return
                other != null &&
                GetType() == other.GetType() &&
                Operator == other.Operator &&
                Left.Equals(other.Left) &&
                Right.Equals(other.Right);
        }

        public override string ToString()
        {
            return Left + " " + Operator + " " + Right;
        }
    }

    internal class LogicalExpression : BinaryExpression
    {
        public override bool IsBool
        {
            get { return true; }
        }

        public LogicalExpression(Operator @operator, Expression left, Expression right)
            : base(ExpressionType.Bool, @operator, ExpressionUtil.CoerceBoolExpression(left), ExpressionUtil.CoerceBoolExpression(right))
        {
        }

        public override Expression Normalize()
        {
            var left = Left.Normalize();
            var right = Right.Normalize();

            var leftLiteral = left as LiteralExpression;
            var rightLiteral = right as LiteralExpression;

            if (leftLiteral != null && rightLiteral != null)
                return NormalizeLiterals(leftLiteral, rightLiteral);

            return new LogicalExpression(Operator, left, right);
        }

        private Expression NormalizeLiterals(LiteralExpression left, LiteralExpression right)
        {
            // These are verified already using CoerceBoolExpression.

            Debug.Assert(left.LiteralType == LiteralType.Boolean);
            Debug.Assert(right.LiteralType == LiteralType.Boolean);

            bool value;

            switch (Operator)
            {
                case Operator.And:
                    value = (bool)left.Value && (bool)right.Value;
                    break;

                case Operator.Or:
                    value = (bool)left.Value || (bool)right.Value;
                    break;

                default:
                    throw new NotSupportedException();
            }

            return new LiteralExpression(value, LiteralType.Boolean);
        }
    }

    internal class ComparisonExpression : BinaryExpression
    {
        public override bool IsBool
        {
            get { return true; }
        }

        public ComparisonExpression(Operator @operator, Expression left, Expression right)
            : base(ExpressionType.Comparison, @operator, left, right)
        {
        }

        public override Expression Normalize()
        {
            var left = Left.Normalize();
            var right = Right.Normalize();

            var leftLiteral = left as LiteralExpression;
            var rightLiteral = right as LiteralExpression;

            if (leftLiteral != null && rightLiteral != null)
                return ResolveLiterals(leftLiteral, rightLiteral);

            return new ComparisonExpression(Operator, left, right);
        }

        private Expression ResolveLiterals(LiteralExpression leftLiteral, LiteralExpression rightLiteral)
        {
            object left = leftLiteral.Value;
            object right = rightLiteral.Value;
            bool result;

            var type = LiteralUtil.CoerceLiteralValues(ref left, leftLiteral.LiteralType, ref right, rightLiteral.LiteralType);

            switch (Operator)
            {
                case Operator.Eq:
                    result = ResolveEquals(left, right, type);
                    break;

                case Operator.Ne:
                    result = !ResolveEquals(left, right, type);
                    break;

                case Operator.Gt:
                    result = ResolveCompare(left, right, type) > 0;
                    break;

                case Operator.Ge:
                    result = ResolveCompare(left, right, type) >= 0;
                    break;

                case Operator.Lt:
                    result = ResolveCompare(left, right, type) < 0;
                    break;

                case Operator.Le:
                    result = ResolveCompare(left, right, type) <= 0;
                    break;

                default:
                    throw new NotSupportedException();
            }

            return new LiteralExpression(result, LiteralType.Boolean);
        }

        private int ResolveCompare(object left, object right, LiteralType type)
        {
            switch (type)
            {
                case LiteralType.Binary:
                case LiteralType.Guid:
                case LiteralType.Null:
                    throw new ODataException(String.Format(
                        ErrorMessages.Expression_CannotCompareTypes, type
                    ));

                default:
                    var comparable = left as IComparable;

                    Debug.Assert(comparable != null);

                    return comparable.CompareTo(right);
            }
        }

        private bool ResolveEquals(object left, object right, LiteralType type)
        {
            if (type == LiteralType.Binary)
                return LiteralUtil.ByteArrayEquals((byte[])left, (byte[])right);
            else
                return Equals(left, right);
        }
    }

    internal class ArithmicExpression : BinaryExpression
    {
        public override bool IsBool
        {
            get { return false; }
        }

        public ArithmicExpression(Operator @operator, Expression left, Expression right)
            : base(ExpressionType.Arithmic, @operator, left, right)
        {
        }

        public override Expression Normalize()
        {
            var left = Left.Normalize();
            var right = Right.Normalize();

            var leftLiteral = left as LiteralExpression;
            var rightLiteral = right as LiteralExpression;

            if (leftLiteral != null && rightLiteral != null)
                return ResolveLiterals(leftLiteral, rightLiteral);

            return new ArithmicExpression(Operator, left, right);
        }

        private Expression ResolveLiterals(LiteralExpression leftLiteral, LiteralExpression rightLiteral)
        {
            object left = leftLiteral.Value;
            object right = rightLiteral.Value;
            object result;

            var type = LiteralUtil.CoerceLiteralValues(ref left, leftLiteral.LiteralType, ref right, rightLiteral.LiteralType);

            switch (Operator)
            {
                case Operator.Add: result = ResolveAdd(left, right, type); break;
                case Operator.Div: result = ResolveDiv(left, right, type); break;
                case Operator.Mod: result = ResolveMod(left, right, type); break;
                case Operator.Mul: result = ResolveMul(left, right, type); break;
                case Operator.Sub: result = ResolveSub(left, right, type); break;

                default:
                    throw new NotSupportedException();
            }

            if (result == null)
            {
                throw new ODataException(String.Format(
                    ErrorMessages.Expression_IncompatibleTypes,
                    Operator,
                    type
                ));
            }

            return new LiteralExpression(result, type);
        }

        private object ResolveAdd(object left, object right, LiteralType type)
        {
            switch (type)
            {
                case LiteralType.Decimal: return (decimal)left + (decimal)right;
                case LiteralType.Double: return (double)left + (double)right;
                // case LiteralType.Duration: return (XmlTimeSpan)left + (XmlTimeSpan)right;
                case LiteralType.Int: return (int)left + (int)right;
                case LiteralType.Long: return (long)left + (long)right;
                case LiteralType.Single: return (float)left + (float)right;
                case LiteralType.String: return (string)left + (string)right;
                default: return null;
            }
        }

        private object ResolveSub(object left, object right, LiteralType type)
        {
            switch (type)
            {
                case LiteralType.Decimal: return (decimal)left - (decimal)right;
                case LiteralType.Double: return (double)left - (double)right;
                // case LiteralType.Duration: return (XmlTimeSpan)left - (XmlTimeSpan)right;
                case LiteralType.Int: return (int)left - (int)right;
                case LiteralType.Long: return (long)left - (long)right;
                case LiteralType.Single: return (float)left - (float)right;
                default: return null;
            }
        }

        private object ResolveMul(object left, object right, LiteralType type)
        {
            switch (type)
            {
                case LiteralType.Decimal: return (decimal)left * (decimal)right;
                case LiteralType.Double: return (double)left * (double)right;
                case LiteralType.Int: return (int)left * (int)right;
                case LiteralType.Long: return (long)left * (long)right;
                case LiteralType.Single: return (float)left * (float)right;
                default: return null;
            }
        }

        private object ResolveDiv(object left, object right, LiteralType type)
        {
            switch (type)
            {
                case LiteralType.Decimal: return (decimal)left / (decimal)right;
                case LiteralType.Double: return (double)left / (double)right;
                case LiteralType.Int: return (int)left / (int)right;
                case LiteralType.Long: return (long)left / (long)right;
                case LiteralType.Single: return (float)left / (float)right;
                default: return null;
            }
        }

        private object ResolveMod(object left, object right, LiteralType type)
        {
            switch (type)
            {
                case LiteralType.Decimal: return (decimal)left % (decimal)right;
                case LiteralType.Double: return (double)left % (double)right;
                case LiteralType.Int: return (int)left % (int)right;
                case LiteralType.Long: return (long)left % (long)right;
                case LiteralType.Single: return (float)left % (float)right;
                default: return null;
            }
        }
    }

    internal class MethodCallExpression : Expression
    {
        public MethodCallType MethodCallType { get; private set; }
        public Method Method { get; private set; }
        public IList<Expression> Arguments { get; private set; }

        public override bool IsBool
        {
            get { return MethodCallType == MethodCallType.Boolean; }
        }

        public MethodCallExpression(MethodCallType type, Method method, params Expression[] arguments)
            : this(type, method, (IList<Expression>)arguments)
        {
        }

        public MethodCallExpression(MethodCallType type, Method method, IList<Expression> arguments)
            : base(ExpressionType.MethodCall)
        {
            Require.NotNull(method, "method");
            Require.NotNull(arguments, "arguments");

            MethodCallType = type;
            Method = method;
            Arguments = arguments;
        }

        public override Expression Normalize()
        {
            // There are no methods with zero argument count.

            Debug.Assert(Arguments.Count > 0);

            var arguments = new Expression[Arguments.Count];

            bool allLiterals = true;

            for (int i = 0; i < Arguments.Count; i++)
            {
                var normalized = Arguments[i].Normalize();

                allLiterals = allLiterals && normalized is LiteralExpression;

                arguments[i] = normalized;
            }

            if (allLiterals)
            {
                var literalArguments = new LiteralExpression[arguments.Length];

                for (int i = 0; i < arguments.Length; i++)
                {
                    literalArguments[i] = (LiteralExpression)arguments[i];
                }

                return Method.Normalize(literalArguments);
            }

            return new MethodCallExpression(MethodCallType, Method, arguments);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as MethodCallExpression;

            if (
                other == null ||
                MethodCallType != other.MethodCallType ||
                Method != other.Method ||
                Arguments.Count != other.Arguments.Count
            )
                return false;

            for (int i = 0; i < Arguments.Count; i++)
            {
                if (!Arguments[i].Equals(other.Arguments[i]))
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(Method.MethodType);
            sb.Append("(");

            for (int i = 0; i < Arguments.Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");

                sb.Append(Arguments[i]);
            }

            sb.Append(")");

            return sb.ToString();
        }
    }

    internal enum MethodCallType
    {
        Normal,
        Boolean
    }
}
