using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal class AliasingNormalizeVisitor : NormalizeVisitor
    {
        public AliasingNormalizeVisitor()
        {
            Aliases = new Dictionary<string, string>(StringComparer.Ordinal);
        }

        public IDictionary<string, string> Aliases { get; private set; }

        public override Expression MemberExpression(MemberExpression expression)
        {
            if (expression.Members.Count == 1)
            {
                Debug.Assert(expression.Members[0].IdExpression == null);

                return new ResolvedMemberExpression(expression.MemberType, expression.Members[0].Name);
            }

            var sb = new StringBuilder();
            string alias = null;

            for (int i = 0; i < expression.Members.Count - 1; i++)
            {
                if (i > 0)
                    sb.Append('.');

                Debug.Assert(expression.Members[i].IdExpression == null);

                sb.Append(expression.Members[i].Name);

                string path = sb.ToString();

                if (!Aliases.TryGetValue(path, out alias))
                {
                    alias = "t" + (Aliases.Count + 1);

                    Aliases.Add(path, alias);
                }
            }

            return new ResolvedMemberExpression(
                expression.MemberType,
                alias + "." + expression.Members[expression.Members.Count - 1].Name
            );
        }
    }
}
