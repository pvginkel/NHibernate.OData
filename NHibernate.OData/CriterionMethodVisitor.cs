using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace NHibernate.OData
{
    internal class CriterionMethodVisitor : QueryMethodVisitorBase<ICriterion>
    {
        private readonly CriterionBuildContext _context;

        public CriterionMethodVisitor(CriterionBuildContext context)
        {
            Require.NotNull(context, "context");

            _context = context;
        }

        public ICriterion CreateCriterion(Method method, Expression[] arguments)
        {
            return method.Visit(this, arguments);
        }

        public override ICriterion SubStringOfMethod(SubStringOfMethod method, Expression[] arguments)
        {
            if (arguments[0].Type != ExpressionType.Literal)
                return base.SubStringOfMethod(method, arguments);

            return Restrictions.Like(
                ProjectionVisitor.CreateProjection(arguments[1]),
                LiteralUtil.CoerceString(((LiteralExpression)arguments[0])),
                MatchMode.Anywhere
            );
        }

        public override ICriterion StartsWithMethod(StartsWithMethod method, Expression[] arguments)
        {
            if (arguments[1].Type != ExpressionType.Literal)
                return base.StartsWithMethod(method, arguments);

            return Restrictions.Like(
                ProjectionVisitor.CreateProjection(arguments[0]),
                LiteralUtil.CoerceString(((LiteralExpression)arguments[1])),
                MatchMode.Start
            );
        }

        public override ICriterion EndsWithMethod(EndsWithMethod method, Expression[] arguments)
        {
            if (arguments[1].Type != ExpressionType.Literal)
                return base.EndsWithMethod(method, arguments);

            return Restrictions.Like(
                ProjectionVisitor.CreateProjection(arguments[0]),
                LiteralUtil.CoerceString(((LiteralExpression)arguments[1])),
                MatchMode.End
            );
        }

        public override ICriterion AnyMethod(AnyMethod method, Expression[] arguments)
        {
            if (arguments[0].Type != ExpressionType.ResolvedMember)
                return base.AnyMethod(method, arguments);

            LambdaExpression lambdaExpression;

            if (arguments.Length < 2)
                lambdaExpression = null;
            else if (arguments[1].Type == ExpressionType.Lambda)
                lambdaExpression = (LambdaExpression)arguments[1];
            else
                return base.AnyMethod(method, arguments);

            return CreateAnyOrAllCriterion(method, (ResolvedMemberExpression)arguments[0], lambdaExpression);
        }

        public override ICriterion AllMethod(AllMethod method, Expression[] arguments)
        {
            if (arguments.Length != 2 || arguments[0].Type != ExpressionType.ResolvedMember || arguments[1].Type != ExpressionType.Lambda)
                return base.AllMethod(method, arguments);

            return CreateAnyOrAllCriterion(method, (ResolvedMemberExpression)arguments[0], (LambdaExpression)arguments[1]);
        }

        private ICriterion CreateAnyOrAllCriterion(CollectionMethod method, ResolvedMemberExpression resolvedMember, LambdaExpression lambdaExpression)
        {
            Require.That(method.MethodType == MethodType.Any || method.MethodType == MethodType.All, "Invalid method type", "method");

            // Resolved member's name may contain multiple dots if it's inside a component (i.e. 'root.Component.Collection')
            int p = resolvedMember.Member.IndexOf('.');
            if (p == -1)
                throw new ODataException(string.Format("Member '{0}' must have an alias.", resolvedMember.Member));

            var collectionHolderAliasName = resolvedMember.Member.Substring(0, p);
            var collectionMemberName = resolvedMember.Member.Substring(p + 1);

            Alias collectionHolderAlias;
            _context.AliasesByName.TryGetValue(collectionHolderAliasName, out collectionHolderAlias);

            if (collectionHolderAlias == null)
                throw new ODataException(string.Format("Unknown alias '{0}'.", collectionHolderAliasName));

            var subCriteriaAlias = _context.CreateUniqueAliasName();
            var detachedCriteria = DetachedCriteria.For(collectionHolderAlias.ReturnedType, subCriteriaAlias);

            MappedClassMetadata metadata;
            _context.SessionFactoryContext.MappedClassMetadata.TryGetValue(collectionHolderAlias.ReturnedType, out metadata);

            if (metadata == null)
                throw new ODataException(string.Format("The type '{0}' isn't a NHibernate-mapped class.", collectionHolderAlias.ReturnedType.FullName));
            if (metadata.IdentifierPropertyName == null)
                throw new ODataException(string.Format("The type '{0}' doesn't have an identifier property.", collectionHolderAlias.ReturnedType.FullName));

            detachedCriteria.Add(Restrictions.EqProperty(
                subCriteriaAlias + "." + metadata.IdentifierPropertyName,
                collectionHolderAliasName + "." + metadata.IdentifierPropertyName
            ));

            var lambdaAlias = _context.CreateUniqueAliasName();

            System.Type itemType;

            if (resolvedMember.ReturnedType == null || (itemType = TypeUtil.TryGetCollectionItemType(resolvedMember.ReturnedType)) == null)
                throw new ODataException("Cannot get collection item type");

            _context.AddAlias(new Alias(lambdaAlias, string.Empty, itemType));

            // The inner joined alias to collection items must be created in any case (whether the lambda expression is specified or not)
            detachedCriteria.CreateAlias(subCriteriaAlias + "." + collectionMemberName, lambdaAlias, JoinType.InnerJoin);

            detachedCriteria.SetProjection(Projections.Constant(1));

            if (lambdaExpression != null)
            {
                if (method.MethodType == MethodType.All)
                    lambdaExpression = (LambdaExpression)InverseVisitor.Invert(lambdaExpression);

                _context.PushLambdaContext(lambdaExpression.ParameterName, itemType, lambdaAlias);

                try
                {
                    var lambdaNormalizeVisitor = new AliasingNormalizeVisitor(
                        _context,
                        collectionHolderAlias.ReturnedType,
                        collectionHolderAliasName
                    );

                    var bodyExpression = lambdaExpression.Body.Visit(lambdaNormalizeVisitor);

                    var criterion = bodyExpression.Visit(new CriterionVisitor(_context));

                    if (criterion != null)
                    {
                        detachedCriteria.Add(criterion);

                        foreach (var alias in lambdaNormalizeVisitor.Aliases.Values)
                            detachedCriteria.CreateAlias(alias.AssociationPath, alias.Name, JoinType.LeftOuterJoin);
                    }
                }
                finally
                {
                    _context.PopLambdaContext();
                }
            }

            if (method.MethodType == MethodType.Any)
                return Subqueries.Exists(detachedCriteria);
            else
                return Subqueries.NotExists(detachedCriteria);
        }
    }
}
