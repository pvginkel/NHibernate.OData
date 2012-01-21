using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal class QueryNotSupportException : ODataException
    {
        public QueryNotSupportException()
            : base("Query is not supported")
        {
        }
    }
}
