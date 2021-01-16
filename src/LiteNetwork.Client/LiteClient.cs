using LiteNetwork.Client.Abstractions;
using LiteNetwork.Client.Internal;
using LiteNetwork.Common.Internal;
using LiteNetwork.Protocol.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LiteNetwork.Client
{
    public class LiteClient : ILiteClient
    {
        public event EventHandler Connected = null!;

        public event EventHandler Disconnected = null!;

        private readonly IServiceProvider _serviceProvider = null!;
        private readonly ILogger<LiteClient>? _logger;
        private readonly LiteClientConnector _connector;
        private readonly LiteSender _sender;
        private readonly LiteClientReceiver _receiver;

        public Guid Id { get; }

        public Socket Socket { get; private set; }

        public LiteClientOptions Options { get; }

        public LiteClient(LiteClientOptions options, IServiceProvider serviceProvider = null!)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Id = Guid.NewGuid();
            Options = options;
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serviceProvider = serviceProvider;
            _connector = new LiteClientConnector(this, Options);
            _sender = new LiteSender(this);
            _receiver = new LiteClientReceiver(options.PacketProcessor, options.ReceiveStrategy, options.BufferSize);

            if (_serviceProvider is not null)
            {
                _logger = _serviceProvider.GetService<ILogger<LiteClient>>();
            }
        }

        public virtual Task HandleMessageAsync(ILitePacketStream incomingPacketStream)
        {
            return Task.CompletedTask;
        }

        public virtual void Send(ILitePacketStream packet) => _sender.Send(packet.Buffer);

        public async Task ConnectAsync()
        {
            bool isConnected = await _connector.ConnectAsync();

            if (isConnected)
            {
                _sender.Start();
                _receiver.StartReceiving(this);
                OnConnected();
            }
        }

        public Task DisconnectAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual void OnConnected()
        {
            Connected?.Invoke(this, null);
        }

        protected virtual void OnDisconnected()
        {
            Disconnected?.Invoke(this, null);
        }

        public void Dispose()
        {
            _sender.Dispose();
            _receiver.Dispose();
        }
    }
}
