using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData.Demo.Domain
{
    public class Employee : IEntity
    {
        public virtual int Id { get; set; }

        public virtual string LastName { get; set; }

        public virtual string FirstName { get; set; }

        public virtual string Title { get; set; }

        public virtual string TitleOfCourtesy { get; set; }

        public virtual DateTime BirthDate { get; set; }

        public virtual DateTime HireDate { get; set; }

        public virtual string Address { get; set; }

        public virtual string City { get; set; }

        public virtual string Region { get; set; }

        public virtual string PostalCode { get; set; }

        public virtual int Extension { get; set; }

        public virtual byte[] Photo { get; set; }

        public virtual string Notes { get; set; }

        public virtual int ReportsTo { get; set; }

        public virtual Iesi.Collections.Generic.ISet<EmployeeTerritory> EmployeeTerritories { get; set; }

        public virtual Iesi.Collections.Generic.ISet<Order> Orders { get; set; }
    }
}
