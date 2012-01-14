using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal class FilterParser : Parser
    {
        public FilterParser(string source)
            : base(source)
        {
        }

        public override Expression Parse()
        {
            throw new NotImplementedException();
        }
    }
}
