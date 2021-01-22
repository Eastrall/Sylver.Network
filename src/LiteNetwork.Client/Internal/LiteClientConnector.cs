using LiteNetwork.Client.Exceptions;
using LiteNetwork.Common;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LiteNetwork.Client.Internal
{
    /// <summary>
    /// Provides a mechanism to manage the lite client connection to a given endpoint.
    /// </summary>
    internal class LiteClientConnector
    {
        /// <summary>
        /// The event used when an error has been occurred during the connection process.
        /// </summary>
        public event EventHandler<LiteClientConnectionException>? Error;

        private readonly SocketAsyncEventArgs _socketEvent;
        private TaskCompletionSource<bool>? _taskCompletion;
        private readonly Socket _socket;
        private readonly string _host;
        private readonly int _port;

        /// <summary>
        /// Gets the current connection state.
        /// </summary>
        public LiteClientStateType State { get; private set; }

        /// <summary>
        /// Creates a new <see cref="LiteClientConnector"/> instance with the given socket, host and port.
        /// </summary>
        /// <param name="connectionSocket">Socket to use for connection process.</param>
        /// <param name="host">The remote host to connect.</param>
        /// <param name="port">The remote port to connect.</param>
        public LiteClientConnector(Socket connectionSocket, string host, int port)
        {
            _socket = connectionSocket;
            _host = host;
            _port = port;
            _socketEvent = new SocketAsyncEventArgs
            {
                DisconnectReuseSocket = true
            };
            _socketEvent.Completed += OnCompleted;
        }


        /// <summary>
        /// Start an asynchronous connection to a remote host.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> that representing the asynchronous operation.
        /// The returns True if the client has been connected successfully, otherwise False.</returns>
        public Task<bool> ConnectAsync()
        {
            lock (this)
            {
                if (State != LiteClientStateType.Disconnected)
                {
                    throw new InvalidOperationException($"Cannot connect with current client state: {State}");
                }

                State = LiteClientStateType.Connecting;
            }

            _taskCompletion = new TaskCompletionSource<bool>();

            Task.Run(async () =>
            {
                try
                {
                    _socketEvent.RemoteEndPoint = await LiteNetworkHelpers.CreateIpEndPointAsync(_host, _port).ConfigureAwait(false);

                    if (!_socket.ConnectAsync(_socketEvent))
                    {
                        OnCompleted(this, _socketEvent);
                    }
                }
                catch (SocketException)
                {
                    Error?.Invoke(this, new LiteClientConnectionException(SocketError.HostUnreachable));
                    _taskCompletion.SetResult(false);
                }
            });

            return _taskCompletion.Task;
        }

        private void OnCompleted(object? sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.LastOperation == SocketAsyncOperation.Connect)
                {
                    if (e.SocketError == SocketError.Success)
                    {
                        State = LiteClientStateType.Connected;
                        _taskCompletion?.SetResult(true);
                    }
                    else
                    {
                        State = LiteClientStateType.Disconnected;
                        Error?.Invoke(this, new LiteClientConnectionException(e.SocketError));
                        _taskCompletion?.SetResult(false);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Error?.Invoke(this, new LiteClientConnectionException("Cannot connect to remote host.", ex));
                _taskCompletion?.SetResult(false);
            }
        }
    }
}
