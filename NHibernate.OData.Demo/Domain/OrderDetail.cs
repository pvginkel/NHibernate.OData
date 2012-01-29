using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData.Demo.Domain
{
    public class OrderDetail : IEntity
    {
        public virtual int Id { get; set; }

        public virtual Order Order { get; set; }

        public virtual Product Product { get; set; }

        public virtual decimal UnitPrice { get; set; }

        public virtual int Quantity { get; set; }

        public virtual decimal Discount { get; set; }
    }
}
