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

    internal abstract class UnaryExpression : Expression
    {
        public Expression Expression { get; private set; }

        protected UnaryExpression(ExpressionType type, Expression expression)
            : base(type)
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
    }

    internal class BoolParenExpression : UnaryExpression
    {
        public override bool IsBool
        {
            get { return true; }
        }

        public BoolParenExpression(Expression expression)
            : base(ExpressionType.BoolParen, expression)
        {
        }

        public override string ToString()
        {
            return "(" + Expression + ")";
        }
    }

    internal class ParenExpression : UnaryExpression
    {
        public override bool IsBool
        {
            get { return false; }
        }

        public ParenExpression(Expression expression)
            : base(ExpressionType.Paren, expression)
        {
        }

        public override string ToString()
        {
            return "(" + Expression + ")";
        }
    }

    internal class BoolUnaryExpression : UnaryExpression
    {
        public KeywordType KeywordType { get; private set; }

        public override bool IsBool
        {
            get { return true; }
        }

        public BoolUnaryExpression(KeywordType type, Expression expression)
            : base(ExpressionType.Bool, expression)
        {
            KeywordType = type;
        }

        public override bool Equals(object obj)
        {
            return
                base.Equals(obj) &&
                KeywordType == ((BoolUnaryExpression)obj).KeywordType;
        }

        public override string ToString()
        {
            return KeywordType + " " + Expression;
        }
    }

    internal class ArithmicUnaryExpression : UnaryExpression
    {
        public KeywordType KeywordType { get; private set; }

        public override bool IsBool
        {
            get { return false; }
        }

        public ArithmicUnaryExpression(KeywordType type, Expression expression)
            : base(ExpressionType.ArithmicUnary, expression)
        {
            KeywordType = type;
        }

        public override bool Equals(object obj)
        {
            return
                base.Equals(obj) &&
                KeywordType == ((ArithmicUnaryExpression)obj).KeywordType;
        }

        public override string ToString()
        {
            return KeywordType + " " + Expression;
        }
    }

    internal abstract class BinaryExpression : Expression
    {
        public Expression Left { get; private set; }

        public Expression Right { get; private set; }

        protected BinaryExpression(ExpressionType type, Expression left, Expression right)
            : base(type)
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
    }

    internal class BoolExpression : BinaryExpression
    {
        public KeywordType KeywordType { get; private set; }

        public override bool IsBool
        {
            get { return true; }
        }

        public BoolExpression(KeywordType type, Expression left, Expression right)
            : base(ExpressionType.Bool, left, right)
        {
            KeywordType = type;
        }

        public override bool Equals(object obj)
        {
            return
                base.Equals(obj) &&
                ((BoolExpression)obj).KeywordType == KeywordType;
        }

        public override string ToString()
        {
            return Left + " " + KeywordType + " " + Right;
        }
    }

    internal class ArithmicExpression : BinaryExpression
    {
        public KeywordType KeywordType { get; private set; }

        public override bool IsBool
        {
            get { return false; }
        }

        public ArithmicExpression(KeywordType type, Expression left, Expression right)
            : base(ExpressionType.Arithmic, left, right)
        {
            KeywordType = type;
        }

        public override bool Equals(object obj)
        {
            return
                base.Equals(obj) &&
                KeywordType == ((ArithmicExpression)obj).KeywordType;
        }

        public override string ToString()
        {
            return Left + " " + KeywordType + " " + Right;
        }
    }

    internal enum KeywordType
    {
        Negative,
        Not,
        And,
        Or,
        Eq,
        Ne,
        Lt,
        Le,
        Gt,
        Ge,
        Add,
        Sub,
        Mul,
        Div,
        Mod,
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
