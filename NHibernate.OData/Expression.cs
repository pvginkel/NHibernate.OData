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
    }

    internal class LiteralExpression : Expression
    {
        public object Value { get; private set; }

        public override bool IsBool
        {
            get { return Value is bool; }
        }

        public LiteralExpression(object value)
            : base(ExpressionType.Literal)
        {
            Value = value;
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
            if (members == null)
                throw new ArgumentNullException("members");

            Debug.Assert(members.Count > 0);

            MemberType = type;
            Members = members;
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
            if (expression == null)
                throw new ArgumentNullException("expression");

            Expression = expression;
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
            if (expression == null)
                throw new ArgumentNullException("expression");

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
    }

    internal abstract class BinaryExpression : OperatorExpression
    {
        public Expression Left { get; private set; }

        public Expression Right { get; private set; }

        protected BinaryExpression(ExpressionType type, Operator @operator, Expression left, Expression right)
            : base(type, @operator)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");

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
            if (method == null)
                throw new ArgumentNullException("method");
            if (arguments == null)
                throw new ArgumentNullException("arguments");

            MethodCallType = type;
            Method = method;
            Arguments = arguments;
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
