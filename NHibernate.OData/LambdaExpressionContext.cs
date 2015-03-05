using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal class LambdaExpressionContext
    {
        public string ParameterName { get; private set; }
        public System.Type ParameterType { get; private set; }
        public string ParameterAlias { get; private set; }

        public LambdaExpressionContext(string parameterName, System.Type parameterType, string parameterAlias)
        {
            Require.NotEmpty(parameterName, "parameterName");
            Require.NotNull(parameterType, "parameterType");
            Require.NotEmpty(parameterAlias, "parameterAlias");

            ParameterName = parameterName;
            ParameterType = parameterType;
            ParameterAlias = parameterAlias;
        }
    }
}
