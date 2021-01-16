using System;

namespace LiteNetwork.Client.Exceptions
{
    public class LiteClientException : Exception
    {
        public LiteClientException()
            : this(string.Empty)
        {
        }

        public LiteClientException(string message)
            : this(message, null!)
        {
        }

        public LiteClientException(Exception innerException)
            : this(string.Empty, innerException)
        {
        }

        public LiteClientException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
