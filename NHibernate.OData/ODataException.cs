using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace NHibernate.OData
{
    [Serializable]
    public class ODataException : Exception
    {
        public ODataException()
        {
        }

        public ODataException(string message)
            : base(message)
        {
        }

        public ODataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ODataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
