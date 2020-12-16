using LiteNetwork.Common;
using LiteNetwork.Common.Internal;
using LiteNetwork.Protocol.Abstractions;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LiteNetwork.Server
{
    /// <summary>
    /// Provides a basic user implementation that can be used for a <see cref="LiteServer{TUser}"/>.
    /// </summary>
    public class LiteServerUser : ILiteConnection, IDisposable
    {
        private readonly LiteSender _sender;
        private bool _disposed;

        /// <inheritdoc />
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the user's connection socket
        /// </summary>
        public Socket Socket { get; internal set; } = null!;

        /// <summary>
        /// Creates a new <see cref="LiteServerUser"/> instance.
        /// </summary>
        public LiteServerUser()
        {
            _sender = new LiteSender(this);
        }

        /// <inheritdoc />
        public virtual Task HandleMessageAsync(ILitePacketStream incomingPacketStream)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Send(ILitePacketStream packet) => _sender.Send(packet.Buffer);

        /// <summary>
        /// Initialize the <see cref="LiteServerUser"/> with the given <see cref="System.Net.Sockets.Socket"/>.
        /// </summary>
        /// <param name="socket">Socket connection.</param>
        internal void Initialize(Socket socket)
        {
            Socket = socket;
            _sender.Start();
        }

        /// <summary>
        /// Called when this user has been Connected.
        /// </summary>
        protected internal virtual void OnConnected()
        {
        }

        /// <summary>
        /// Called when this user has been Disconnected.
        /// </summary>
        protected internal virtual void OnDisconnected()
        {
        }

        /// <summary>
        /// Dispose a <see cref="LiteServerUser"/> resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _sender.Dispose();
                Socket.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}
