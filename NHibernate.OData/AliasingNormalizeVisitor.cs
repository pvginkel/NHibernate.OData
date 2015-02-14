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
            MappedClassMetadata mappedClass = null;

            if (type != null)
                _context.MappedClassMetadata.TryGetValue(type, out mappedClass);

            if (expression.Members.Count == 1)
            {
                Debug.Assert(expression.Members[0].IdExpression == null);

                return new ResolvedMemberExpression(expression.MemberType, ResolveName(mappedClass, string.Empty, expression.Members[0].Name, ref type));
            }

            var sb = new StringBuilder();
            string lastAlias = null;

            for (int i = 0; i < expression.Members.Count; i++)
            {
                var member = expression.Members[i];

                Debug.Assert(member.IdExpression == null);

                bool isLastMember = i == expression.Members.Count - 1;
                string resolvedName = ResolveName(mappedClass, sb.ToString(), member.Name, ref type);

                if (sb.Length > 0)
                    sb.Append('.');

                sb.Append(resolvedName);

                if (type != null && _context.MappedClassMetadata.ContainsKey(type) && !isLastMember)
                {
                    mappedClass = _context.MappedClassMetadata[type];

                    string path = (lastAlias != null ? lastAlias + "." : null) + sb;

                    if (!Aliases.TryGetValue(path, out lastAlias))
                    {
                        lastAlias = "t" + (Aliases.Count + 1).ToString(CultureInfo.InvariantCulture);
                        Aliases.Add(path, lastAlias);
                    }

                    sb.Clear();
                }
            }

            return new ResolvedMemberExpression(
                expression.MemberType,
                (lastAlias != null ? lastAlias + "." : null) + sb
            );
        }

        private string ResolveName(MappedClassMetadata mappedClass, string mappedClassPath, string name, ref System.Type type)
        {
            if (type == null)
                return name;

            // Dynamic component support
            if (type == typeof(IDictionary) && mappedClass != null)
            {
                string fullPath = mappedClassPath + "." + name;

                var dynamicProperty = mappedClass.FindDynamicComponentProperty(fullPath, _caseSensitive);

                if (dynamicProperty == null)
                    throw new QueryException(String.Format(
                        "Cannot resolve member '{0}' of dynamic component '{1}' on '{2}'", name, mappedClassPath, type
                    ));

                type = dynamicProperty.Type;
                return dynamicProperty.Name;
            }

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
