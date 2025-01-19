using System;
using System.Runtime.Serialization;

namespace Wolfteam.Tools.XfsEditor.Xfs.Exceptions
{
    public class XfsVersionUnsupportedException : XfsException
    {
        public XfsVersionUnsupportedException()
        {
        }

        public XfsVersionUnsupportedException(string message) : base(message)
        {
        }

        public XfsVersionUnsupportedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected XfsVersionUnsupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
