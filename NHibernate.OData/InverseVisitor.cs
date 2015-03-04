using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal class InverseVisitor : IVisitor<Expression>
    {
        private static readonly InverseVisitor _instance = new InverseVisitor();

        private InverseVisitor()
        {
        }

        public static Expression Invert(Expression expression)
        {
            return expression.Visit(_instance);
        }

        public Expression BoolUnaryExpression(BoolUnaryExpression expression)
        {
            if (expression.Operator != Operator.Not)
                throw new NotSupportedException();

            return expression.Expression;
        }

        public Expression LogicalExpression(LogicalExpression expression)
        {
            Operator op = expression.Operator;

            switch (op)
            {
                case Operator.And: op = Operator.Or; break;
                case Operator.Or: op = Operator.And; break;
                default:
                    throw new NotSupportedException();
            }

            return new LogicalExpression(op, expression.Left.Visit(this), expression.Right.Visit(this));
        }

        public Expression ComparisonExpression(ComparisonExpression expression)
        {
            Operator op = expression.Operator;

            switch (op)
            {
                case Operator.Gt: op = Operator.Le; break;
                case Operator.Ge: op = Operator.Lt; break;
                case Operator.Lt: op = Operator.Ge; break;
                case Operator.Le: op = Operator.Gt; break;
                case Operator.Eq: op = Operator.Ne; break;
                case Operator.Ne: op = Operator.Eq; break;
                default:
                    throw new NotSupportedException();
            }

            return new ComparisonExpression(op, expression.Left.Visit(this), expression.Right.Visit(this));
        }

        public Expression MethodCallExpression(MethodCallExpression expression)
        {
            var args = expression.Arguments.Select(x => x.Visit(this)).ToArray();
            var methodCallExpr = new MethodCallExpression(expression.MethodCallType, expression.Method, args);

            if (methodCallExpr.IsBool)
                return new BoolUnaryExpression(Operator.Not, methodCallExpr);
               
            return methodCallExpr;
        }

        public Expression LiteralExpression(LiteralExpression expression)
        {
            return expression;
        }

        public Expression MemberExpression(MemberExpression expression)
        {
            if (expression.IsBool)
                return new BoolUnaryExpression(Operator.Not, expression);

            return expression;
        }

        public Expression ResolvedMemberExpression(ResolvedMemberExpression expression)
        {
            return expression;
        }

        public Expression ParenExpression(ParenExpression expression)
        {
            return expression.Expression.Visit(this);
        }

        public Expression ArithmeticUnaryExpression(ArithmeticUnaryExpression expression)
        {
            return new ArithmeticUnaryExpression(expression.Operator, expression.Expression.Visit(this));
        }

        public Expression ArithmeticExpression(ArithmeticExpression expression)
        {
            return new ArithmeticExpression(expression.Operator, expression.Left.Visit(this), expression.Right.Visit(this));
        }

        public Expression LambdaExpression(LambdaExpression expression)
        {
            return new LambdaExpression(expression.ParameterName, expression.Body.Visit(this));
        }
    }
}
