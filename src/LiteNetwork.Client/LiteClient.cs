using LiteNetwork.Client.Abstractions;
using LiteNetwork.Client.Internal;
using LiteNetwork.Common.Internal;
using LiteNetwork.Protocol.Abstractions;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LiteNetwork.Client
{
    public class LiteClient : ILiteClient
    {
        private readonly LiteSender _sender;
        private readonly LiteClientReceiver _receiver;

        /// <inheritdoc />
        public Guid Id { get; }

        public Socket Socket { get; private set; }

        public LiteClientOptions Options { get; }

        public LiteClient(LiteClientOptions options)
        {
            Id = Guid.NewGuid();
            Options = options;
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _sender = new LiteSender(this);
            _receiver = new LiteClientReceiver(options.PacketProcessor, options.ReceiveStrategy, options.BufferSize);
        }

        public virtual Task HandleMessageAsync(ILitePacketStream incomingPacketStream)
        {
            return Task.CompletedTask;
        }

        public virtual void Send(ILitePacketStream packet) => _sender.Send(packet.Buffer);

        public Task ConnectAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisconnectAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual void OnConnected()
        {
        }

        protected virtual void OnDisconnected()
        {
        }

        public void Dispose()
        {
            _sender.Dispose();
            _receiver.Dispose();
        }
    }
}
