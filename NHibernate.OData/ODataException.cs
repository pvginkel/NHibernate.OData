using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace NHibernate.OData
{
    /// <summary>
    /// Represents an error occurred while parsing an OData query string.
    /// </summary>
    [Serializable]
    public class ODataException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataException"/>
        /// class.
        /// </summary>
        public ODataException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataException"/>
        /// class with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ODataException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataException"/>
        /// class with the specified error message and a reference to the inner
        /// exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the
        /// current exception, or a null reference (Nothing in Visual Basic)
        /// if no inner exception is specified.</param>
        public ODataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the System.Exception class with serialized
        ///  data.
        /// </summary>
        /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.</param>
        protected ODataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
