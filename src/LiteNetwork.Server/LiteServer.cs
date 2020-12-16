using LiteNetwork.Common;
using LiteNetwork.Common.Exceptions;
using LiteNetwork.Protocol;
using LiteNetwork.Protocol.Abstractions;
using LiteNetwork.Server.Abstractions;
using LiteNetwork.Server.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LiteNetwork.Server
{
    /// <summary>
    /// Provides a basic <see cref="ILiteServer{TUser}"/> implementation.
    /// </summary>
    /// <typeparam name="TUser">The user type that the server will be use.</typeparam>
    public class LiteServer<TUser> : ILiteServer<TUser> where TUser : LiteServerUser
    {
        private readonly ILogger<LiteServer<TUser>>? _logger;
        private readonly ILitePacketProcessor _packetProcessor;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<Guid, TUser> _connectedUsers;
        private readonly Socket _socket;
        private readonly LiteServerAcceptor _acceptor;
        private readonly LiteServerReceiver _receiver;

        /// <inheritdoc />
        public bool IsRunning { get; private set; }

        /// <inheritdoc />
        public LiteServerOptions Configuration { get; }

        /// <inheritdoc />
        public IEnumerable<TUser> ConnectedUsers => _connectedUsers.Values;

        /// <summary>
        /// Creates a new <see cref="LiteServer{TUser}"/> instance with a server configuration.
        /// </summary>
        /// <param name="configuration">Server configuration</param>
        public LiteServer(LiteServerOptions configuration)
            : this(configuration, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="LiteServer{TUser}"/> instance with a server configuration 
        /// and a service provider.
        /// </summary>
        /// <param name="configuration">Server configuration.</param>
        /// <param name="serviceProvider">Service provider to use.</param>
        public LiteServer(LiteServerOptions configuration, IServiceProvider? serviceProvider)
        {
            Configuration = configuration;
            _packetProcessor = configuration.PacketProcessor;
            _serviceProvider = serviceProvider!;
            _connectedUsers = new ConcurrentDictionary<Guid, TUser>();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

            if (serviceProvider is not null)
            {
                _logger = serviceProvider.GetService<ILogger<LiteServer<TUser>>>();
            }

            _acceptor = new LiteServerAcceptor(_socket);
            _acceptor.OnClientAccepted += OnClientAccepted;
            _acceptor.OnError += OnAcceptorError;

            _receiver = new LiteServerReceiver(_packetProcessor, Configuration.ReceiveStrategy, Configuration.ClientBufferSize);
            _receiver.Disconnected += OnDisconnected;
            _receiver.Error += OnReceiverError;
        }

        /// <inheritdoc />
        public TUser? GetUser(Guid userId) => TryGetUser(userId, out TUser? user) ? user : default;

        /// <inheritdoc />
        public bool TryGetUser(Guid userId, out TUser? user) => _connectedUsers.TryGetValue(userId, out user);

        /// <inheritdoc />
        public async void Start()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("Server is already running.");
            }

            OnBeforeStart();

            IPEndPoint localEndPoint = await LiteNetworkHelpers.CreateIpEndPointAsync(Configuration.Host, Configuration.Port).ConfigureAwait(false);
            _socket.Bind(localEndPoint);
            _socket.Listen(Configuration.Backlog);
            _acceptor.StartAccept();
            IsRunning = true;

            OnAfterStart();
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(Start, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <inheritdoc />
        public Task StartAsync()
        {
            return Task.Factory.StartNew(Start, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <inheritdoc />
        public void Stop()
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException("Server is not running.");
            }

            OnBeforeStop();

            foreach (var connectedUser in _connectedUsers)
            {
                DisconnectUser(connectedUser.Key);
            }

            _connectedUsers.Clear();

            IsRunning = false;
            OnAfterStop();
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(Stop, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <inheritdoc />
        public Task StopAsync()
        {
            return Task.Factory.StartNew(Stop, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <inheritdoc />
        public void DisconnectUser(Guid userId)
        {
            if (!_connectedUsers.TryRemove(userId, out TUser user))
            {
                _logger?.LogError($"Cannot find user with id '{user.Id}'.");
                return;
            }

            _logger?.LogTrace($"User with id '{user.Id}' disconnected.");
            user.OnDisconnected();
            user.Dispose();
        }

        /// <inheritdoc />
        public void SendTo(TUser user, ILitePacketStream packet)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (packet is null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            user.Send(packet);
        }

        /// <inheritdoc />
        public void SendTo(IEnumerable<TUser> users, ILitePacketStream packet)
        {
            if (users is null)
            {
                throw new ArgumentNullException(nameof(users));
            }

            if (packet is null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            foreach (TUser connection in users)
            {
                SendTo(connection, packet);
            }
        }

        /// <inheritdoc />
        public void SendToAll(ILitePacketStream packet) => SendTo(_connectedUsers.Values, packet);

        /// <summary>
        /// Dispose the server resources and disconnects all the connected users.
        /// </summary>
        public void Dispose()
        {
            if (IsRunning)
            {
                Stop();
            }

            _socket.Dispose();
            _acceptor.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Executes the child business logic before starting the server.
        /// </summary>
        protected virtual void OnBeforeStart() { }

        /// <summary>
        /// Executes the child business logic after the server starts.
        /// </summary>
        protected virtual void OnAfterStart() { }

        /// <summary>
        /// Executes the child business logic before stoping the server.
        /// </summary>
        protected virtual void OnBeforeStop() { }

        /// <summary>
        /// Executes the child business logic after the server stops.
        /// </summary>
        protected virtual void OnAfterStop() { }

        /// <summary>
        /// Called when an error occurs on the server.
        /// </summary>
        /// <param name="connection">Connection where the error occured.</param>
        /// <param name="exception">Error exception.</param>
        protected virtual void OnError(ILiteConnection? connection, Exception exception)
        {
            if (connection is null)
            {
                _logger?.LogError(exception, $"An error has occured.");
            }
            else
            {
                _logger?.LogError(exception, $"An error has occured for user '{connection.Id}'.");
            }
        }

        private void OnClientAccepted(object? sender, SocketAsyncEventArgs e)
        {
            TUser user = ActivatorUtilities.CreateInstance<TUser>(_serviceProvider);

            if (!_connectedUsers.TryAdd(user.Id, user))
            {
                throw new LiteNetworkException($"Failed to add user with id: '{user.Id}'. An user with same id already exists.");
            }

            if (e.AcceptSocket is null)
            {
                throw new LiteNetworkException($"The accepted socket is null.");
            }

            user.Initialize(e.AcceptSocket);
            _logger?.LogInformation($"New user connected from '{user.Socket.RemoteEndPoint}' with id '{user.Id}'.");
            user.OnConnected();
            _receiver.StartReceiving(user);
        }

        private void OnAcceptorError(object? sender, Exception e)
        {
            OnError(null, e);
        }

        private void OnReceiverError(object? sender, Exception e)
        {
            if (e is LiteReceiverException receiveException)
            {
                OnError(receiveException.Connection, receiveException);
            }
            else
            {
                _logger?.LogError(e, "Receiver error.");
            }
        }

        private void OnDisconnected(object? sender, ILiteConnection e)
        {
            DisconnectUser(e.Id);
        }
    }
}
