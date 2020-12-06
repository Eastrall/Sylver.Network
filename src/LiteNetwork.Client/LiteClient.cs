using LiteNetwork.Client.Abstractions;
using LiteNetwork.Common;
using LiteNetwork.Protocol.Abstractions;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LiteNetwork.Client
{
    public class LiteClient : ILiteClient
    {
        /// <inheritdoc />
        public Guid Id { get; }

        /// <summary>
        /// Gets or sets the user's connection socket
        /// </summary>
        internal Socket Socket { get; set; } = null!;

        /// <summary>
        /// Defines an action to send an <see cref="ILitePacketStream"/>.
        /// </summary>
        internal Action<ILitePacketStream>? SendAction { get; set; }

        public LiteClient()
        {
            Id = Guid.NewGuid();
        }

        public virtual Task HandleMessageAsync(ILitePacketStream incomingPacketStream)
        {
            return Task.CompletedTask;
        }

        public virtual void Send(ILitePacketStream packet)
        {
        }

        public Task ConnectAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisconnectAsync()
        {
            return Task.CompletedTask;
        }
    }
}
