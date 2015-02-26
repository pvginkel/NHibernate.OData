using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal class LambdaExpressionContext
    {
        public LambdaExpression Expression { get; private set; }
        public System.Type ParameterType { get; private set; }
        public string ParameterAlias { get; private set; }

        public LambdaExpressionContext(LambdaExpression expression, System.Type parameterType, string parameterAlias)
        {
            Require.NotNull(expression, "expression");
            Require.NotNull(parameterType, "parameterType");
            Require.NotEmpty(parameterAlias, "parameterAlias");

            Expression = expression;
            ParameterType = parameterType;
            ParameterAlias = parameterAlias;
        }
    }
}
