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
        private readonly CriterionBuildContext _context;
        private readonly System.Type _persistentClass;
        private readonly bool _caseSensitive;
        private readonly string _rootAlias;

        public AliasingNormalizeVisitor(CriterionBuildContext context, System.Type persistentClass, bool caseSensitive, string rootAlias)
        {
            if (rootAlias != null && rootAlias.Length == 0)
                throw new ArgumentException("Root alias cannot be an empty string.", "rootAlias");

            _context = context;
            _persistentClass = persistentClass;
            _caseSensitive = caseSensitive;
            _rootAlias = rootAlias;

            Aliases = new Dictionary<string, Alias>(StringComparer.Ordinal);
        }

        public IDictionary<string, Alias> Aliases { get; private set; }

        public override Expression MemberExpression(MemberExpression expression)
        {
            var type = _persistentClass;
            MappedClassMetadata mappedClass = null;

            if (type != null)
                _context.SessionFactoryContext.MappedClassMetadata.TryGetValue(type, out mappedClass);

            if (expression.Members.Count == 1)
            {
                Debug.Assert(expression.Members[0].IdExpression == null);

                string resolvedName = ResolveName(mappedClass, string.Empty, expression.Members[0].Name, ref type);

                return new ResolvedMemberExpression(
                    expression.MemberType,
                    (_rootAlias != null ? _rootAlias + "." : null) + resolvedName,
                    type
                );
            }

            var sb = new StringBuilder();
            string lastAliasName = _rootAlias;

            for (int i = 0; i < expression.Members.Count; i++)
            {
                var member = expression.Members[i];

                Debug.Assert(member.IdExpression == null);

                bool isLastMember = i == expression.Members.Count - 1;
                string resolvedName = ResolveName(mappedClass, sb.ToString(), member.Name, ref type);

                if (sb.Length > 0)
                    sb.Append('.');

                sb.Append(resolvedName);

                if (type != null && _context.SessionFactoryContext.MappedClassMetadata.ContainsKey(type) && !isLastMember)
                {
                    mappedClass = _context.SessionFactoryContext.MappedClassMetadata[type];

                    string path = (lastAliasName != null ? lastAliasName + "." : null) + sb;
                    Alias alias;
                   
                    if (!Aliases.TryGetValue(path, out alias))
                    {
                        alias = new Alias(
                            _context.CreateUniqueAliasName(),
                            path,
                            type
                        );

                        Aliases.Add(path, alias);
                    }

                    lastAliasName = alias.Name;

                    sb.Clear();
                }
            }

            return new ResolvedMemberExpression(
                expression.MemberType,
                (lastAliasName != null ? lastAliasName + "." : null) + sb,
                type
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
