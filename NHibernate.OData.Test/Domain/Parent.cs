using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData.Test.Domain
{
    public class Parent : IEntity
    {
        public virtual int Id { get; set; }

        public virtual string Name { get; set; }

        public virtual int Int32 { get; set; }

        public virtual string LengthString { get; set; }

        public virtual DateTime DateTime { get; set; }

        public virtual Child Child { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
