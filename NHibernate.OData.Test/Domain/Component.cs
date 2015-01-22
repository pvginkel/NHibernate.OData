using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData.Test.Domain
{
    public class Component
    {
        public virtual string Value { get; set; }
        public virtual int? IntValue { get; set; }
    }
}