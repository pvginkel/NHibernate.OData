using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    public class ODataParserConfiguration
    {
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// By default joins will be inner joins. Set this to true to use left outer joins.
        /// </summary>
        public bool OuterJoin { get; set; }

        public ODataParserConfiguration()
        {
            CaseSensitive = true;
        }
    }
}
