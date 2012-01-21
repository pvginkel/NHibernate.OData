using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Criterion;

namespace NHibernate.OData
{
    internal class OrderBy
    {
        public OrderBy(IProjection projection, OrderByDirection direction)
        {
            Require.NotNull(projection, "projection");

            Direction = direction;
            Projection = projection;
        }

        public OrderByDirection Direction { get; private set; }

        public IProjection Projection { get; private set; }
    }
}
