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

        public abstract T Visit<T>(IVisitor<T> visitor);
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

        public override T Visit<T>(IVisitor<T> visitor)
        {
            return visitor.LiteralExpression(this);
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
        public IList<MemberExpressionComponent> Members { get; private set; }

        public override bool IsBool
        {
            get { return MemberType == MemberType.Boolean; }
        }

        public MemberExpression(MemberType type, params string[] members)
            : this(type, members.Select(p => new MemberExpressionComponent(p, null)).ToArray())
        {
        }

        public MemberExpression(MemberType type, IList<MemberExpressionComponent> members)
            : base(ExpressionType.Member)
        {
            Require.NotNull(members, "members");

            Debug.Assert(members.Count > 0);

            MemberType = type;
            Members = members;
        }

        public override T Visit<T>(IVisitor<T> visitor)
        {
            return visitor.MemberExpression(this);
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
                if (!Equals(Members[i], other.Members[i]))
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            bool hadOne = false;

            foreach (var member in Members)
            {
                if (hadOne)
                    sb.Append('/');
                else
                    hadOne = true;

                sb.Append(member);
            }

            return sb.ToString();
        }
    }

    internal class MemberExpressionComponent
    {
        public string Name { get; private set; }

        public LiteralExpression IdExpression { get; private set; }

        public MemberExpressionComponent(string name, LiteralExpression idExpression)
        {
            Require.NotNull(name, "name");

            Name = name;
            IdExpression = idExpression;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as MemberExpressionComponent;

            return
                other != null &&
                Name == other.Name &&
                Equals(IdExpression, other.IdExpression);
        }

        public override string ToString()
        {
            if (IdExpression == null)
                return Name;
            else
                return Name + "(" + IdExpression + ")";
        }
    }

    internal class ResolvedMemberExpression : Expression
    {
        public ResolvedMemberExpression(MemberType memberType, string member, System.Type returnedType)
            : base(ExpressionType.ResolvedMember)
        {
            Require.NotNull(member, "member");

            Member = member;
            MemberType = memberType;
            ReturnedType = returnedType;
        }

        public string Member { get; private set; }

        public MemberType MemberType { get; private set; }

        public System.Type ReturnedType { get; private set; }

        public override bool IsBool
        {
            get { return MemberType == MemberType.Boolean; }
        }

        public override T Visit<T>(IVisitor<T> visitor)
        {
            return visitor.ResolvedMemberExpression(this);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as ResolvedMemberExpression;

            return
                other != null &&
                Member == other.Member &&
                IsBool == other.IsBool &&
                ReturnedType == other.ReturnedType;
        }

        public override string ToString()
        {
            return Member;
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

        public override T Visit<T>(IVisitor<T> visitor)
        {
            return visitor.ParenExpression(this);
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
            return "(" + Operator + " " + Expression + ")";
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

        public override T Visit<T>(IVisitor<T> visitor)
        {
            return visitor.BoolUnaryExpression(this);
        }
    }

    internal class ArithmeticUnaryExpression : UnaryExpression
    {
        public override bool IsBool
        {
            get { return false; }
        }

        public ArithmeticUnaryExpression(Operator @operator, Expression expression)
            : base(ExpressionType.ArithmeticUnary, expression, @operator)
        {
        }

        public override T Visit<T>(IVisitor<T> visitor)
        {
            return visitor.ArithmeticUnaryExpression(this);
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
            return "(" + Left + " " + Operator + " " + Right + ")";
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

        public override T Visit<T>(IVisitor<T> visitor)
        {
            return visitor.LogicalExpression(this);
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

        public override T Visit<T>(IVisitor<T> visitor)
        {
            return visitor.ComparisonExpression(this);
        }
    }

    internal class ArithmeticExpression : BinaryExpression
    {
        public override bool IsBool
        {
            get { return false; }
        }

        public ArithmeticExpression(Operator @operator, Expression left, Expression right)
            : base(ExpressionType.Arithmetic, @operator, left, right)
        {
        }

        public override T Visit<T>(IVisitor<T> visitor)
        {
            return visitor.ArithmeticExpression(this);
        }
    }

    internal class MethodCallExpression : Expression
    {
        public MethodCallType MethodCallType { get; private set; }
        public Method Method { get; private set; }
        public Expression[] Arguments { get; private set; }

        public override bool IsBool
        {
            get { return MethodCallType == MethodCallType.Boolean; }
        }

        public MethodCallExpression(MethodCallType type, Method method, params Expression[] arguments)
            : base(ExpressionType.MethodCall)
        {
            Require.NotNull(method, "method");
            Require.NotNull(arguments, "arguments");

            MethodCallType = type;
            Method = method;
            Arguments = arguments;
        }

        public override T Visit<T>(IVisitor<T> visitor)
        {
            return visitor.MethodCallExpression(this);
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
                Arguments.Length != other.Arguments.Length
            )
                return false;

            for (int i = 0; i < Arguments.Length; i++)
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

            for (int i = 0; i < Arguments.Length; i++)
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

    internal class LambdaExpression : Expression
    {
        public string ParameterName { get; private set; }
        public Expression Body { get; private set; }

        public override bool IsBool
        {
            get { return true; }
        }

        public LambdaExpression(string parameterName, Expression body)
            : base(ExpressionType.Lambda)
        {
            Require.NotEmpty(parameterName, "parameterName");
            Require.NotNull(body, "body");

            ParameterName = parameterName;
            Body = ExpressionUtil.CoerceBoolExpression(body);
        }

        public override T Visit<T>(IVisitor<T> visitor)
        {
            return visitor.LambdaExpression(this);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as LambdaExpression;

            if (
                other == null ||
                !ParameterName.Equals(other.ParameterName, StringComparison.Ordinal) ||
                !Body.Equals(other.Body)
            )
                return false;

            return true;
        }

        public override string ToString()
        {
            return string.Format("({0}: {1})", ParameterName, Body);
        }
    }
}
