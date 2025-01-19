using System;
using System.Runtime.Serialization;

namespace Wolfteam.Tools.XfsEditor.Xfs.Exceptions
{
    public class XfsException : Exception
    {
        public XfsException()
        {
        }

        public XfsException(string message) : base(message)
        {
        }

        public XfsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected XfsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
