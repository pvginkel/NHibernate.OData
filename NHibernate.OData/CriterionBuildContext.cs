using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NHibernate.OData
{
    internal class CriterionBuildContext
    {
        public ODataSessionFactoryContext SessionFactoryContext { get; private set; }
        public IDictionary<string, Alias> AliasesByName { get; private set; }
        public bool CaseSensitive { get; private set; }

        public bool IsInsideLambdaContext
        {
            get { return _lambdaContextStack.Count != 0; }
        }

        private int _aliasCounter;

        private readonly Stack<LambdaExpressionContext> _lambdaContextStack = new Stack<LambdaExpressionContext>();

        public CriterionBuildContext(ODataSessionFactoryContext sessionFactoryContext, bool caseSensitive)
        {
            Require.NotNull(sessionFactoryContext, "sessionFactoryContext");

            SessionFactoryContext = sessionFactoryContext;
            CaseSensitive = caseSensitive;

            AliasesByName = new Dictionary<string, Alias>(StringComparer.Ordinal);
        }

        public void AddAliases(IEnumerable<Alias> aliasesToAdd)
        {
            Require.NotNull(aliasesToAdd, "aliasesToAdd");

            foreach (var alias in aliasesToAdd)
                AddAlias(alias);
        }

        public void AddAlias(Alias alias)
        {
            Require.NotNull(alias, "alias");

            AliasesByName.Add(alias.Name, alias);
        }

        public string CreateUniqueAliasName()
        {
            return "t" + (++_aliasCounter).ToString(CultureInfo.InvariantCulture);
        }

        public void PushLambdaContext(LambdaExpression lambdaExpression, System.Type parameterType, string parameterAlias)
        {
            if (lambdaExpression.ParameterName == "$it" || _lambdaContextStack.Any(x => x.Expression.ParameterName.Equals(lambdaExpression.ParameterName, StringComparison.Ordinal)))
                throw new ODataException(string.Format("Lambda expression parameter '{0}' has been already defined in the parent scope.", lambdaExpression.ParameterName));

            _lambdaContextStack.Push(new LambdaExpressionContext(lambdaExpression, parameterType, parameterAlias));
        }

        public void PopLambdaContext()
        {
            _lambdaContextStack.Pop();
        }

        public LambdaExpressionContext FindLambdaContext(string parameterName)
        {
            Require.NotNull(parameterName, "parameterName");

            return _lambdaContextStack.FirstOrDefault(x => x.Expression.ParameterName.Equals(parameterName, StringComparison.Ordinal));
        }
    }
}
