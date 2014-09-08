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

        /// <summary>
        /// Unescape URI query string percent-encoded parts using UTF-8 characted encoding
        /// </summary>
        public bool UTF8Unescape { get; set; }

        public ODataParserConfiguration()
        {
            CaseSensitive = true;
        }
    }
}
