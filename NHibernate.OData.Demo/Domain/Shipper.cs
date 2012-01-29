using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData.Demo.Domain
{
    public class Shipper : IEntity
    {
        public virtual int Id { get; set; }

        public virtual string CompanyName { get; set; }

        public virtual string Phone { get; set; }
    }
}
