using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal class OrderByParser : Parser
    {
        private readonly AliasingNormalizeVisitor _normalizeVisitor;

        public OrderByParser(string source, AliasingNormalizeVisitor normalizeVisitor)
            : base(source, ParserMode.Normal)
        {
            Require.NotNull(normalizeVisitor, "normalizeVisitor");

            _normalizeVisitor = normalizeVisitor;
        }

        public OrderBy[] Parse()
        {
            var orderBys = new List<OrderBy>();

            while (true)
            {
                var result = ParseCommon();

                var projection = ProjectionVisitor.CreateProjection(
                    result.Visit(_normalizeVisitor)
                );

                if (AtEnd)
                {
                    orderBys.Add(new OrderBy(projection, OrderByDirection.Ascending));

                    break;
                }
                else
                {
                    var direction = GetOrderByDirection(Current);

                    if (!direction.HasValue)
                        direction = OrderByDirection.Ascending;
                    else
                        MoveNext();

                    orderBys.Add(new OrderBy(projection, direction.Value));

                    if (AtEnd)
                        break;

                    if (Current != SyntaxToken.Comma)
                        throw new ODataException(ErrorMessages.OrderByParser_ExpectedNextOrEnd);
                    
                    MoveNext();
                }
            }

            return orderBys.ToArray();
        }
    }
}
