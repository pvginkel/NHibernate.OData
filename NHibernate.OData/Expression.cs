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
        public LiteralType LiteralType { get; private set; }

        public object Value { get; private set; }

        public override bool IsBool
        {
            get { return LiteralType == LiteralType.Boolean; }
        }

        public LiteralExpression(LiteralType type, object value)
            : base(ExpressionType.Literal)
        {
            LiteralType = type;
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as LiteralExpression;

            return
                other != null &&
                LiteralType == other.LiteralType &&
                Equals(Value, other.Value);
        }

        public override string ToString()
        {
            return Value == null ? "null" : Value.ToString();
        }
    }

    internal enum LiteralType
    {
        Normal,
        Boolean
    }

    internal class MemberExpression : Expression
    {
        public MemberType MemberType { get; private set; }
        public IList<string> Members { get; private set; }

        public override bool IsBool
        {
            get { return MemberType == MemberType.Boolean; }
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
            : base(ExpressionType.Bool, expression, @operator)
        {
        }

        public override bool Equals(object obj)
        {
            return
                base.Equals(obj) &&
                Operator == ((BoolUnaryExpression)obj).Operator;
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

        public override bool Equals(object obj)
        {
            return
                base.Equals(obj) &&
                Operator == ((ArithmicUnaryExpression)obj).Operator;
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
                Left.Equals(other.Left) &&
                Right.Equals(other.Right);
        }

        public override string ToString()
        {
            return Left + " " + Operator + " " + Right;
        }
    }

    internal class BoolExpression : BinaryExpression
    {
        public override bool IsBool
        {
            get { return true; }
        }

        public BoolExpression(Operator @operator, Expression left, Expression right)
            : base(ExpressionType.Bool, @operator, left, right)
        {
        }

        public override bool Equals(object obj)
        {
            return
                base.Equals(obj) &&
                ((BoolExpression)obj).Operator == Operator;
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

        public override bool Equals(object obj)
        {
            return
                base.Equals(obj) &&
                Operator == ((ArithmicExpression)obj).Operator;
        }
    }

    // Precedence taken from http://msdn.microsoft.com/en-us/library/Aa691323
    internal enum Operator
    {
        // Unary
        Negative,
        Not,
        // Multiplicative
        Mul,
        Div,
        Mod,
        // Additive
        Add,
        Sub,
        // Relational and type testing
        Lt,
        Le,
        Gt,
        Ge,
        // Equality
        Eq,
        Ne,
        // Conditional AND
        And,
        // Conditional OR
        Or
    }

    internal class MethodCallExpression : Expression
    {
        public MethodCallType MethodCallType { get; private set; }
        public string Method { get; private set; }
        public IList<Expression> Expressions { get; private set; }

        public override bool IsBool
        {
            get { return MethodCallType == MethodCallType.Boolean || MethodCallType == MethodCallType.BooleanCast; }
        }

        public MethodCallExpression(MethodCallType type, string method, IList<Expression> expressions)
            : base(ExpressionType.MethodCall)
        {
            if (method == null)
                throw new ArgumentNullException("method");
            if (expressions == null)
                throw new ArgumentNullException("expressions");

            MethodCallType = type;
            Method = method;
            Expressions = expressions;
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
                Expressions.Count != other.Expressions.Count
            )
                return false;

            for (int i = 0; i < Expressions.Count; i++)
            {
                if (!Expressions[i].Equals(other.Expressions[i]))
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(Method);
            sb.Append("(");

            for (int i = 0; i < Expressions.Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");

                sb.Append(Expressions[i]);
            }

            sb.Append(")");

            return sb.ToString();
        }
    }

    internal enum MethodCallType
    {
        Boolean,
        Cast,
        BooleanCast,
        Other
    }
}
