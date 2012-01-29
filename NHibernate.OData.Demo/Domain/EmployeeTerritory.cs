using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData.Demo.Domain
{
    public class EmployeeTerritory : IEntity
    {
        public virtual int Id { get; set; }

        public virtual Employee Employee { get; set; }

        public virtual Territory Territory { get; set; }
    }
}
