using Sylver.Network.Common;
using Sylver.Network.Data;
using Sylver.Network.Data.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Sylver.Network.Infrastructure
{
    internal abstract class NetReceiver : INetReceiver
    {
        private readonly NetPacketParser _packetParser;
        private readonly BlockingCollection<NetMessageData> _messageQueue;
        private readonly IPacketProcessor _packetProcessor;
        private readonly CancellationToken _cancellationToken;
        private bool _disposedValue;

        /// <summary>
        /// Creates a new <see cref="NetReceiver"/> instance.
        /// </summary>
        /// <param name="packetProcessor">Default packet processor.</param>
        public NetReceiver(IPacketProcessor packetProcessor)
        {
            _packetParser = new NetPacketParser(packetProcessor);
            _messageQueue = new BlockingCollection<NetMessageData>();
            _packetProcessor = packetProcessor;
        }

        /// <inheritdoc />
        public void SetPacketProcessor(IPacketProcessor packetProcessor)
        {
            _packetParser.PacketProcessor = packetProcessor;
        }

        /// <inheritdoc />
        public void Start(INetUser clientConnection)
        {
            Task.Factory.StartNew(() =>
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        ProcessReceivedMessages();
                    }
                    catch (OperationCanceledException)
                    {
                        // The operation has been cancelled: nothing to do
                    }
                }
            }, _cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            SocketAsyncEventArgs socketAsyncEvent = GetSocketEvent();
            socketAsyncEvent.UserToken = new NetToken(clientConnection);

            ReceiveData(clientConnection, socketAsyncEvent);
        }

        /// <summary>
        /// Receive data from a client.
        /// </summary>
        /// <param name="client">Client.</param>
        /// <param name="socketAsyncEvent">Socket async event arguments.</param>
        private void ReceiveData(INetConnection client, SocketAsyncEventArgs socketAsyncEvent)
        {
            if (!client.Socket.ReceiveAsync(socketAsyncEvent))
            {
                ProcessReceive(socketAsyncEvent.UserToken as NetToken, socketAsyncEvent);
            }
        }

        /// <summary>
        /// Process the received data.
        /// </summary>
        /// <param name="clientToken">Client token.</param>
        /// <param name="socketAsyncEvent">Socket async event arguments.</param>
        [ExcludeFromCodeCoverage]
        private void ProcessReceive(NetToken clientToken, SocketAsyncEventArgs socketAsyncEvent)
        {
            if (socketAsyncEvent == null)
            {
                throw new ArgumentNullException(nameof(socketAsyncEvent), "Cannot receive data from a null socket event.");
            }

            if (socketAsyncEvent.BytesTransferred > 0)
            {
                if (socketAsyncEvent.SocketError == SocketError.Success)
                {
                    IEnumerable<byte[]> messages = _packetParser.ParseIncomingData(clientToken, socketAsyncEvent.Buffer, socketAsyncEvent.BytesTransferred);

                    if (messages.Any())
                    {
                        foreach (byte[] message in messages)
                        {
                            if (!_messageQueue.TryAdd(new NetMessageData(clientToken.Client, message)))
                            {
                                // TODO: on error
                            }
                        }
                    }

                    if (clientToken.DataStartOffset >= socketAsyncEvent.BytesTransferred)
                    {
                        clientToken.Reset();
                    }

                    ReceiveData(clientToken.Client, socketAsyncEvent);
                }
                else
                {
                    OnError(clientToken.Client, socketAsyncEvent.SocketError);
                }
            }
            else
            {
                ClearSocketEvent(socketAsyncEvent);
                OnDisconnected(clientToken.Client);
            }
        }

        /// <summary>
        /// Fired when a receive operation has completed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Socket async event arguments.</param>
        [ExcludeFromCodeCoverage]
        protected internal void OnCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (sender == null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                ProcessReceive(e.UserToken as NetToken, e);
            }
            else
            {
                throw new InvalidOperationException($"Unknown '{e.LastOperation}' socket operation in receiver.");
            }
        }

        /// <summary>
        /// Dispose the net receiver resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the net receiver.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }
                _disposedValue = true;
            }
        }

        /// <summary>
        /// Gets a <see cref="SocketAsyncEventArgs"/> for the receive operation.
        /// </summary>
        /// <returns></returns>
        protected abstract SocketAsyncEventArgs GetSocketEvent();

        /// <summary>
        /// Clears an used <see cref="SocketAsyncEventArgs"/>.
        /// </summary>
        /// <param name="socketAsyncEvent">Socket async vent arguments to clear.</param>
        protected abstract void ClearSocketEvent(SocketAsyncEventArgs socketAsyncEvent);

        /// <summary>
        /// Client has been disconnected.
        /// </summary>
        /// <param name="client">Disconnected client.</param>
        protected abstract void OnDisconnected(INetUser client);

        /// <summary>
        /// A socket error has occured.
        /// </summary>
        /// <param name="client">Client.</param>
        /// <param name="socketError">Socket error.</param>
        protected abstract void OnError(INetUser client, SocketError socketError);

        /// <summary>
        /// Process the received message queue.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void ProcessReceivedMessages()
        {
            NetMessageData message = _messageQueue.Take(_cancellationToken);

            if (message != null && message.Connection is INetUser client)
            {
                using (INetPacketStream packet = _packetProcessor.CreatePacket(message.Data))
                {
                    client.HandleMessage(packet);
                }
            }
        }
    }
}
