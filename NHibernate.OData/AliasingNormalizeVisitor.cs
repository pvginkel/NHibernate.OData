using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NHibernate.OData
{
    internal class AliasingNormalizeVisitor : NormalizeVisitor
    {
        private readonly CriterionBuildContext _context;
        private readonly System.Type _persistentClass;
        private readonly string _rootAlias;

        public AliasingNormalizeVisitor(CriterionBuildContext context, System.Type persistentClass, string rootAlias)
        {
            if (rootAlias != null && rootAlias.Length == 0)
                throw new ArgumentException("Root alias cannot be an empty string.", "rootAlias");

            _context = context;
            _persistentClass = persistentClass;
            _rootAlias = rootAlias;

            Aliases = new Dictionary<string, Alias>(StringComparer.Ordinal);
        }

        public IDictionary<string, Alias> Aliases { get; private set; }

        public override Expression MemberExpression(MemberExpression expression)
        {
            var type = _persistentClass;
            MappedClassMetadata mappedClass = null;
            var members = expression.Members;
            var lastAliasName = _rootAlias;

            // If we are inside a lambda expression
            if (_context.ExpressionLevel > 1)
            {
                // Lambda member expression MUST start with a variable name
                var lambdaContext = _context.FindLambdaContext(members[0].Name);
                if (lambdaContext == null)
                    throw new QueryException(ErrorMessages.Expression_LambdaMemberMustStartWithParameter);

                type = lambdaContext.ParameterType;
                lastAliasName = lambdaContext.ParameterAlias;

                members = members.Skip(1).ToList();
            }
            else if (members[0].Name == "$it")
            {
                // Special case: $it variable outside of lambda expression
                members = members.Skip(1).ToList();
            }

            if (type != null)
                _context.SessionFactoryContext.MappedClassMetadata.TryGetValue(type, out mappedClass);

            if (members.Count == 1)
            {
                Debug.Assert(members[0].IdExpression == null);

                string resolvedName = ResolveName(mappedClass, string.Empty, members[0].Name, ref type);

                return new ResolvedMemberExpression(
                    expression.MemberType,
                    (lastAliasName != null ? lastAliasName + "." : null) + resolvedName,
                    type
                );
            }

            var sb = new StringBuilder();

            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];

                Debug.Assert(member.IdExpression == null);

                bool isLastMember = i == members.Count - 1;
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
                        alias = new Alias(_context.CreateUniqueAliasName(), path, type);
                        Aliases.Add(path, alias);
                        _context.AddAlias(alias);
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

                var dynamicProperty = mappedClass.FindDynamicComponentProperty(fullPath, _context.CaseSensitive);

                if (dynamicProperty == null)
                    throw new QueryException(String.Format(
                        ErrorMessages.Resolve_CannotResolveDynamicComponentMember, name, mappedClassPath, type
                    ));

                type = dynamicProperty.Type;
                return dynamicProperty.Name;
            }

            var resolvedName = _context.NameResolver.ResolveName(name, ref type, _context.CaseSensitive);
            if (resolvedName != null)
            {
                type = resolvedName.Type;
                return resolvedName.Name;
            }

            throw new QueryException(String.Format(
                ErrorMessages.Resolve_CannotResolveName, name, type)
            );
        }
    }
}
