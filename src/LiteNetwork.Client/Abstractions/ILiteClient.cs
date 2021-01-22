using LiteNetwork.Common;
using System;
using System.Threading.Tasks;

namespace LiteNetwork.Client.Abstractions
{
    /// <summary>
    /// Provides a mechanism to manage a TCP client.
    /// </summary>
    public interface ILiteClient : ILiteConnection, IDisposable
    {
        /// <summary>
        /// Gets the client configuration options.
        /// </summary>
        LiteClientOptions Options { get; }

        /// <summary>
        /// Connects to a remote server asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> that finished when the connection process has been finished.</returns>
        Task ConnectAsync();

        /// <summary>
        /// Disconnects from the remote server asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> that finished when the disconnection process has been finished.</returns>
        Task DisconnectAsync();
    }
}
