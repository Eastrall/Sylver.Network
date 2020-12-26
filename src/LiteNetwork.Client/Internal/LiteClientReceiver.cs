using LiteNetwork.Common;
using LiteNetwork.Common.Internal;
using LiteNetwork.Protocol.Abstractions;
using System;
using System.Net.Sockets;

namespace LiteNetwork.Client.Internal
{
    /// <summary>
    /// Overrides the basic <see cref="LiteReceiver"/> for the client needs.
    /// </summary>
    internal class LiteClientReceiver : LiteReceiver, IDisposable
    {
        private readonly byte[] _buffer;
        private readonly SocketAsyncEventArgs _socketEvent;

        /// <summary>
        /// Creates a new <see cref="LiteClientReceiver"/> instance with the given <see cref="ILitePacketProcessor"/>
        /// and a client buffer size.
        /// </summary>
        /// <param name="packetProcessor">Current packet processor used in the client.</param>
        /// <param name="receiveStrategy">The receive strategy type for every received message.</param>
        /// <param name="bufferSize">Buffer size defined in client configuration.</param>
        public LiteClientReceiver(ILitePacketProcessor packetProcessor, ReceiveStrategyType receiveStrategy, int bufferSize) 
            : base(packetProcessor, receiveStrategy)
        {
            _buffer = new byte[bufferSize];
            _socketEvent = new SocketAsyncEventArgs();
            _socketEvent.Completed += OnCompleted;
            _socketEvent.SetBuffer(_buffer, 0, _buffer.Length);
        }

        /// <inheritdoc />
        protected override void ClearSocketEvent(SocketAsyncEventArgs socketAsyncEvent)
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            _socketEvent.UserToken = null;
        }

        /// <inheritdoc />
        protected override SocketAsyncEventArgs GetSocketEvent()
        {
            return _socketEvent;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _socketEvent.SetBuffer(null, 0, 0);
            _socketEvent.UserToken = null;
            _socketEvent.Completed -= OnCompleted;
        }
    }
}
