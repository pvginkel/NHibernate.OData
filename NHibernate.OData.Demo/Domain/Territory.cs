using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData.Demo.Domain
{
    public class Territory : IEntity
    {
        public virtual int Id { get; set; }

        public virtual string TerritoryDescription { get; set; }

        public virtual Region Region { get; set; }

        public virtual ISet<EmployeeTerritory> EmployeeTerritories { get; set; }
    }
}
