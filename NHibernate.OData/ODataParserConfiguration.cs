using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    public class ODataParserConfiguration
    {
        public bool CaseSensitive { get; set; }

        public ODataParserConfiguration()
        {
            CaseSensitive = true;
        }
    }
}
