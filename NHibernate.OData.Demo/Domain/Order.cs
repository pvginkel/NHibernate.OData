using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iesi.Collections.Generic;

namespace NHibernate.OData.Demo.Domain
{
    public class Order : IEntity
    {
        public virtual int Id { get; set; }

        public virtual Customer Customer { get; set; }

        public virtual Employee Employee { get; set; }

        public virtual DateTime OrderDate { get; set; }

        public virtual DateTime RequiredDate { get; set; }
        
        public virtual DateTime ShippedDate { get; set; }

        public virtual int ShipVia { get; set; }

        public virtual decimal Freight { get; set; }

        public virtual string ShipName { get; set; }
        
        public virtual string ShipAddress { get; set; }
        
        public virtual string ShipCity { get; set; }

        public virtual string ShipRegion { get; set; }
        
        public virtual string ShipPostalCode { get; set; }
        
        public virtual string ShipCountry { get; set; }

        public virtual Iesi.Collections.Generic.ISet<OrderDetail> OrderDetails { get; set; }
    }
}
