using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData.Demo.Domain
{
    public class Product : IEntity
    {
        public virtual int Id { get; set; }

        public virtual string ProductName { get; set; }

        public virtual Supplier Supplier { get; set; }

        public virtual Category Category { get; set; }

        public virtual string QuantityPerUnit { get; set; }

        public virtual decimal UnitPrice { get; set; }

        public virtual int UnitsInStock { get; set; }

        public virtual int UnitsOnOrder { get; set; }

        public virtual int ReorderLevel { get; set; }

        public virtual bool Discontinued { get; set; }

        public virtual ISet<OrderDetail> OrderDetails { get; set; }
    }
}
