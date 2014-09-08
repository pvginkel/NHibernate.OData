using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData.Demo.Domain
{
    public class Category : IEntity
    {
        public virtual int Id { get; set; }

        public virtual string CategoryName { get; set; }

        public virtual string Description { get; set; }

        public virtual byte[] Picture { get; set; }

        public virtual ISet<Product> Products { get; set; }
    }
}
