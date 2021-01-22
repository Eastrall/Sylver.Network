using LiteNetwork.Common;
using LiteNetwork.Protocol;
using LiteNetwork.Protocol.Abstractions;

namespace LiteNetwork.Client
{
    /// <summary>
    /// Provides a data structure that describes the client options.
    /// </summary>
    public class LiteClientOptions
    {
        /// <summary>
        /// Gets the default buffer allocated size.
        /// </summary>
        public const int DefaultBufferSize = 128;

        /// <summary>
        /// Gets or sets the remote host to connect.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the remote port to connect.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the handled client buffer size.
        /// </summary>
        public int BufferSize { get; set; } = DefaultBufferSize;

        /// <summary>
        /// Gets or sets the receive strategy type.
        /// </summary>
        public ReceiveStrategyType ReceiveStrategy { get; set; }

        /// <summary>
        /// Gets the default server packet processor.
        /// </summary>
        public ILitePacketProcessor PacketProcessor { get; set; }

        /// <summary>
        /// Creates and initializes a new <see cref="LiteClientOptions"/> instance
        /// with a default <see cref="LitePacketProcessor"/>.
        /// </summary>
        public LiteClientOptions()
        {
            PacketProcessor = new LitePacketProcessor();
        }
    }
}
