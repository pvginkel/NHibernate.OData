using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NHibernate.OData
{
    internal class AliasingNormalizeVisitor : NormalizeVisitor
    {
        private readonly ODataSessionFactoryContext _context;
        private readonly System.Type _persistentClass;
        private readonly bool _caseSensitive;

        public AliasingNormalizeVisitor(ODataSessionFactoryContext context, System.Type persistentClass, bool caseSensitive)
        {
            _context = context;
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

            for (int i = 0; i < expression.Members.Count; i++)
            {
                var member = expression.Members[i];

                Debug.Assert(member.IdExpression == null);

                bool isLastMember = i == expression.Members.Count - 1;
                string resolvedName = ResolveName(member.Name, ref type);

                if (sb.Length > 0)
                    sb.Append('.');

                sb.Append(resolvedName);

                if (_context.MappedClasses.Contains(type) && !isLastMember)
                {
                    string path = sb.ToString();
                    string alias;

                    if (!Aliases.TryGetValue(path, out alias))
                    {
                        alias = "t" + (Aliases.Count + 1).ToString(CultureInfo.InvariantCulture);

                        Aliases.Add(path, alias);
                    }

                    sb.Clear();
                    sb.Append(alias);
                }
            }

            return new ResolvedMemberExpression(
                expression.MemberType,
                sb.ToString()
            );
        }

        private string ResolveName(string name, ref System.Type type)
        {
            if (type == null || typeof(IDictionary).IsAssignableFrom(type))
                return name;

            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            if (!_caseSensitive)
                bindingFlags |= BindingFlags.IgnoreCase;

            var property = type.GetProperty(name, bindingFlags);

            if (property != null)
            {
                type = property.PropertyType;
                return property.Name;
            }

            var field = type.GetField(name, bindingFlags);

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
