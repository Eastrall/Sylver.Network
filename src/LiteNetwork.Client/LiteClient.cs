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
        /// <summary>
        /// The event used when the client has been connected.
        /// </summary>
        public event EventHandler? Connected;

        /// <summary>
        /// The event used when the client has been disconnected.
        /// </summary>
        public event EventHandler? Disconnected;

        private readonly IServiceProvider _serviceProvider = null!;
        private readonly ILogger<LiteClient>? _logger;
        private readonly LiteClientConnector _connector;
        private readonly LiteSender _sender;
        private readonly LiteClientReceiver _receiver;

        /// <inheritdoc />
        public Guid Id { get; }

        /// <inheritdoc />
        public Socket Socket { get; }

        /// <inheritdoc />
        public LiteClientOptions Options { get; }

        /// <summary>
        /// Creates a new <see cref="LiteClient"/> instance with the given <see cref="LiteClientOptions"/>.
        /// </summary>
        /// <param name="options">A client configuration options.</param>
        /// <param name="serviceProvider">Service provider to use.</param>
        public LiteClient(LiteClientOptions options, IServiceProvider serviceProvider = null!)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Id = Guid.NewGuid();
            Options = options;
            _serviceProvider = serviceProvider;
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _connector = new LiteClientConnector(Socket, Options.Host, Options.Port);
            _sender = new LiteSender(this);
            _receiver = new LiteClientReceiver(options.PacketProcessor, options.ReceiveStrategy, options.BufferSize);

            if (_serviceProvider is not null)
            {
                _logger = _serviceProvider.GetService<ILogger<LiteClient>>();
            }
        }

        /// <inheritdoc />
        public virtual Task HandleMessageAsync(ILitePacketStream incomingPacketStream)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public virtual void Send(ILitePacketStream packet) => _sender.Send(packet.Buffer);

        /// <inheritdoc />
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

        /// <inheritdoc />
        public Task DisconnectAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Fired when the client has been connected.
        /// </summary>
        protected virtual void OnConnected()
        {
            Connected?.Invoke(this, null);
        }

        /// <summary>
        /// Fired when the client has been disconnected.
        /// </summary>
        protected virtual void OnDisconnected()
        {
            Disconnected?.Invoke(this, null);
        }

        /// <summary>
        /// Dispose the client resources.
        /// </summary>
        public void Dispose()
        {
            _sender.Dispose();
            _receiver.Dispose();
        }
    }
}
