using LiteNetwork.Protocol.Abstractions;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LiteNetwork.Common
{
    /// <summary>
    /// Provides an abstraction that represents a living connection.
    /// </summary>
    public interface ILiteConnection
    {
        /// <summary>
        /// Gets the connection unique identifier.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the connection socket.
        /// </summary>
        Socket Socket { get; }

        /// <summary>
        /// Handle an incoming <see cref="ILitePacketStream"/> asynchronously.
        /// </summary>
        /// <param name="incomingPacketStream">Incoming packet.</param>
        /// <returns>A <see cref="Task"/> that completes when finished the handle message operation.</returns>
        Task HandleMessageAsync(ILitePacketStream incomingPacketStream);

        /// <summary>
        /// Sends an <see cref="ILitePacketStream"/> to the remote end point.
        /// </summary>
        /// <param name="packet">Packet stream.</param>
        void Send(ILitePacketStream packet);

        /// <summary>
        /// Sends a raw <see cref="byte[]" /> buffer to the remote end point.
        /// </summary>
        /// <param name="packetBuffer">Raw packet buffer as a byte array.</param>
        void Send(byte[] packetBuffer);
    }
}
