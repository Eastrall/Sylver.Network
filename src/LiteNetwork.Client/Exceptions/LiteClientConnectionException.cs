using System;
using System.Net.Sockets;

namespace LiteNetwork.Client.Exceptions
{
    /// <summary>
    /// Exception used when a client connection fails.
    /// </summary>
    public class LiteClientConnectionException : LiteClientException
    {
        /// <summary>
        /// Gets the connection socket error.
        /// </summary>
        public SocketError SocketError { get; }

        /// <summary>
        /// Creates a new <see cref="LiteClientConnectionException"/> instance.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        internal LiteClientConnectionException(string message, Exception innerException = null!)
            : base(message, innerException)
        {
            SocketError = SocketError.HostUnreachable;
        }

        /// <summary>
        /// Creates a new <see cref="LiteClientConnectionException"/> instance.
        /// </summary>
        /// <param name="socketError">Socket error.</param>
        internal LiteClientConnectionException(SocketError socketError, Exception innerException = null!)
            : base(innerException)
        {
            SocketError = socketError;
        }
    }
}
