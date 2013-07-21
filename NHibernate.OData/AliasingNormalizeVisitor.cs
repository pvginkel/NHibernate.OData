using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NHibernate.OData
{
    internal class AliasingNormalizeVisitor : NormalizeVisitor
    {
        private readonly System.Type _persistentClass;
        private readonly bool _caseSensitive;

        public AliasingNormalizeVisitor(System.Type persistentClass, bool caseSensitive)
        {
            _persistentClass = persistentClass;
            _caseSensitive = caseSensitive;

            Aliases = new Dictionary<string, string>(StringComparer.Ordinal);
        }

        public IDictionary<string, string> Aliases { get; private set; }

        public override Expression MemberExpression(MemberExpression expression)
        {
            var type = _persistentClass;

            if (expression.Members.Count == 1)
            {
                Debug.Assert(expression.Members[0].IdExpression == null);

                return new ResolvedMemberExpression(expression.MemberType, ResolveName(expression.Members[0].Name, ref type));
            }

            var sb = new StringBuilder();
            string alias = null;

            for (int i = 0; i < expression.Members.Count - 1; i++)
            {
                if (i > 0)
                    sb.Append('.');

                Debug.Assert(expression.Members[i].IdExpression == null);

                sb.Append(ResolveName(expression.Members[i].Name, ref type));

                string path = sb.ToString();

                if (!Aliases.TryGetValue(path, out alias))
                {
                    alias = "t" + (Aliases.Count + 1);

                    Aliases.Add(path, alias);
                }
            }

            return new ResolvedMemberExpression(
                expression.MemberType,
                alias + "." + ResolveName(expression.Members[expression.Members.Count - 1].Name, ref type)
            );
        }

        private string ResolveName(string name, ref System.Type type)
        {
            if (_caseSensitive)
                return name;

            var property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);

            if (property != null)
            {
                type = property.PropertyType;
                return property.Name;
            }

            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);

            if (field != null)
            {
                type = field.FieldType;
                return field.Name;
            }

            throw new QueryException(String.Format(
                "Cannot resolve name '{0}' on '{1}'", name, type)
            );
        }
    }
}
