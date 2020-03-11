using Sylver.Network.Client.Internal;
using Sylver.Network.Common;
using Sylver.Network.Data;
using Sylver.Network.Infrastructure;
using System;
using System.Net.Sockets;

namespace Sylver.Network.Client
{
    public class NetClient : NetConnection, INetClient
    {
        private IPacketProcessor _packetProcessor;

        private readonly INetClientConnector _connector;
        private readonly INetSender _sender;
        private readonly INetReceiver _receiver;

        /// <inheritdoc />
        public bool IsConnected => Socket.IsConnected;

        /// <inheritdoc />
        public NetClientConfiguration ClientConfiguration { get; protected set; }

        /// <inheritdoc />
        public IPacketProcessor PacketProcessor
        {
            get => _packetProcessor;
            set
            {
                if (IsConnected)
                {
                    throw new InvalidOperationException("Cannot update packet processor when the client is already connected.");
                }

                _packetProcessor = value;
                _receiver.SetPacketProcessor(_packetProcessor);
            }
        }

        /// <summary>
        /// Creates a new <see cref="NetClient"/> instance without any configuration.
        /// </summary>
        public NetClient()
            : this(null)
        {
        }

        /// <summary>
        /// Creates and configures a new <see cref="NetClient"/> instance.
        /// </summary>
        /// <param name="clientConfiguration"></param>
        public NetClient(NetClientConfiguration clientConfiguration)
            : base(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            ClientConfiguration = clientConfiguration;
            _packetProcessor = new NetPacketProcessor();
            _connector = new NetClientConnector(this);
            _connector.Connected += OnClientConnected;
            _connector.Error += OnClientConnectionError;
            _sender = new NetClientSender();
            _receiver = new NetClientReceiver(this);
        }

        /// <inheritdoc />
        public void Connect()
        {
            if (IsConnected)
            {
                throw new InvalidOperationException("Client is already connected to remote.");
            }

            if (ClientConfiguration == null)
            {
                throw new ArgumentNullException("Client configuration is not set.", nameof(ClientConfiguration));
            }

            if (ClientConfiguration.Port <= 0)
            {
                throw new ArgumentException($"Invalid port number '{ClientConfiguration.Port}' in configuration.", nameof(ClientConfiguration.Port));
            }

            if (NetHelper.BuildIPAddress(ClientConfiguration.Host) == null)
            {
                throw new ArgumentException($"Invalid host address '{ClientConfiguration.Host}' in configuration", nameof(ClientConfiguration.Host));
            }

            if (ClientConfiguration.BufferSize <= 0)
            {
                throw new ArgumentException($"Invalid buffer size '{ClientConfiguration.BufferSize}' in configuration.", nameof(ClientConfiguration.BufferSize));
            }

            _sender.Start();
            _connector.Connect();
        }

        /// <inheritdoc />
        public void Disconnect()
        {
            _sender.Stop();
            Socket.GetSocket().Disconnect(true);
            OnDisconnected();
        }

        /// <inheritdoc />
        public void Send(INetPacketStream packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            _sender.Send(new NetMessageData(this, packet.Buffer));
        }

        /// <inheritdoc />
        public virtual void HandleMessage(INetPacketStream packet)
        {
            // Nothing to do. Must be override in child classes.
        }

        /// <summary>
        /// Dispose the <see cref="NetClient"/> resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            _connector.Dispose();
            _sender.Dispose();
            _receiver.Dispose();
            base.Dispose(disposing);
        }

        protected virtual void OnConnected() { }

        protected virtual void OnDisconnected() { }

        private void OnClientConnected(object sender, EventArgs e)
        {
            OnConnected();
            _receiver.Start(this);
        }

        private void OnClientConnectionError(object sender, SocketError e)
        {
            Console.WriteLine($"Connection error: {e.ToString()}");
        }
    }
}
