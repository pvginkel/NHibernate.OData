using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Criterion;
using NHibernate.OData.Extensions;
using NHibernate.Type;

namespace NHibernate.OData
{
    internal class ProjectionVisitor : QueryVisitorBase<IProjection>
    {
        private static readonly IType DefaultArithmeticReturnType = NHibernateUtil.Decimal; // Arithmetic operations on two columns default to decimal
        private static readonly ProjectionVisitor _instance = new ProjectionVisitor();

        private ProjectionVisitor()
        {
        }

        public static IProjection CreateProjection(Expression expression)
        {
            return expression.Visit(_instance);
        }

        public override IProjection LiteralExpression(LiteralExpression expression)
        {
            return Projections.Constant(expression.Value);
        }

        public override IProjection AliasedMemberExpression(AliasedMemberExpression expression)
        {
            return Projections.Property(expression.Member);
        }

        public override IProjection MethodCallExpression(MethodCallExpression expression)
        {
            return ProjectionMethodVisitor.CreateProjection(expression.Method, expression.Arguments);
        }

        public override IProjection ArithmicExpression(ArithmicExpression expression)
        {
            var left = CreateProjection(expression.Left);
            var right = CreateProjection(expression.Right);
            var returnType = ArtithmicReturnType(expression.Left, expression.Right);

            switch (expression.Operator)
            {
                case Operator.Add: return new ArithmeticOperatorProjection("+", returnType, left, right);
                case Operator.Sub: return new ArithmeticOperatorProjection("-", returnType, left, right);
                case Operator.Mul: return new ArithmeticOperatorProjection("*", returnType, left, right);
                case Operator.Div: return new ArithmeticOperatorProjection("/", returnType, left, right);

                case Operator.Mod:
                    return new SqlFunctionProjection(
                        "mod",
                        returnType,
                        left,
                        right
                    );

                default: throw new NotSupportedException();
            }
        }

        private IType ArtithmicReturnType(Expression left, Expression right)
        {
            if (left.Type == ExpressionType.Literal)
                return TypeFromLiteralType(((LiteralExpression)left).LiteralType);
            else if (right.Type == ExpressionType.Literal)
                return TypeFromLiteralType(((LiteralExpression)right).LiteralType);
            else
                return DefaultArithmeticReturnType;
        }

        private IType TypeFromLiteralType(LiteralType type)
        {
            switch (type)
            {
                case LiteralType.String: return NHibernateUtil.String;
                case LiteralType.Boolean: return NHibernateUtil.Boolean;
                case LiteralType.Single: return NHibernateUtil.Single;
                case LiteralType.Double: return NHibernateUtil.Double;
                case LiteralType.Decimal: return NHibernateUtil.Decimal;
                case LiteralType.Int: return NHibernateUtil.Int32;
                case LiteralType.Long: return NHibernateUtil.Int64;
                case LiteralType.Binary: return NHibernateUtil.Binary;
                case LiteralType.DateTime: return NHibernateUtil.DateTime;
                case LiteralType.Guid: return NHibernateUtil.Guid;
                case LiteralType.Duration: return NHibernateUtil.TimeSpan;
                default: throw new NotSupportedException();
            }
        }

        public override IProjection ArithmicUnaryExpression(ArithmicUnaryExpression expression)
        {
            var projection = CreateProjection(expression.Expression);

            switch (expression.Operator)
            {
                case Operator.Negative:
                    return new ArithmeticOperatorProjection(
                        "-",
                        DefaultArithmeticReturnType,
                        Projections.Constant(0),
                        projection
                    );

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
