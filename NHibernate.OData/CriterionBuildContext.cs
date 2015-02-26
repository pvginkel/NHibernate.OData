using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal class CriterionBuildContext
    {
        public ODataSessionFactoryContext SessionFactoryContext { get; private set; }
        public IDictionary<string, Alias> AliasesByName { get; private set; }

        private int _aliasCounter;

        public CriterionBuildContext(ODataSessionFactoryContext sessionFactoryContext)
        {
            Require.NotNull(sessionFactoryContext, "sessionFactoryContext");

            SessionFactoryContext = sessionFactoryContext;
            AliasesByName = new Dictionary<string, Alias>();
        }

        public void AddAliases(IEnumerable<Alias> aliasesToAdd)
        {
            foreach (var alias in aliasesToAdd)
                AliasesByName.Add(alias.Name, alias);
        }

        public string CreateUniqueAliasName()
        {
            return "t" + (++_aliasCounter).ToString(CultureInfo.InvariantCulture);
        }
    }
}
