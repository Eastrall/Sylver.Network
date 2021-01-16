using LiteNetwork.Client.Abstractions;
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
        /// The event used when an error has been occurred during the acceptation process.
        /// </summary>
        public event EventHandler<Exception>? Error;

        private readonly SocketAsyncEventArgs _socketEvent;
        private readonly ILiteClient _client;
        private readonly LiteClientOptions _options;
        private TaskCompletionSource<bool> _taskCompletion = null!;

        public LiteClientStateType State { get; private set; }

        public LiteClientConnector(ILiteClient client, LiteClientOptions options)
        {
            _client = client;
            _options = options;
            _socketEvent = new SocketAsyncEventArgs
            {
                DisconnectReuseSocket = true
            };
            _socketEvent.Completed += OnCompleted;
        }

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
                _socketEvent.RemoteEndPoint = await LiteNetworkHelpers.CreateIpEndPointAsync(_options.Host, _options.Port).ConfigureAwait(false);
                
                if (!_client.Socket.ConnectAsync(_socketEvent))
                {
                    OnCompleted(this, _socketEvent);
                }
            });

            return _taskCompletion.Task;
        }

        private void OnCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.LastOperation == SocketAsyncOperation.Connect)
                {
                    if (e.SocketError == SocketError.Success)
                    {
                        State = LiteClientStateType.Connected;
                        _taskCompletion.SetResult(true);
                    }
                    else
                    {
                        State = LiteClientStateType.Disconnected;
                        _taskCompletion.SetResult(false);
                        Error?.Invoke(this, new LiteClientConnectionException(e.SocketError));
                    }
                }
            }
            catch (StackOverflowException)
            {
                _taskCompletion.SetResult(false);
                Error?.Invoke(this, new LiteClientConnectionException(SocketError.HostUnreachable));
            }
            catch (Exception ex)
            {
                _taskCompletion.SetResult(false);
                Error?.Invoke(this, new LiteClientConnectionException("Cannot connect to remote host.", ex));
            }
        }
    }
}
