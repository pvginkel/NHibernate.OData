using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    /// <summary>
    /// Provide configuration for the OData parser.
    /// </summary>
    public class ODataParserConfiguration
    {
        /// <summary>
        /// Whether or not OData queries should be parsed case sensitive.
        /// </summary>
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// By default joins will be inner joins. Set this to true to use left outer joins.
        /// </summary>
        public bool OuterJoin { get; set; }

        /// <summary>
        /// Unescape URI query string percent-encoded parts using UTF-8 characted encoding
        /// </summary>
        public bool UTF8Unescape { get; set; }

        /// <summary>
        /// Create a new instance of the ODataParserConfiguration class.
        /// </summary>
        public ODataParserConfiguration()
        {
            CaseSensitive = true;
        }
    }
}
