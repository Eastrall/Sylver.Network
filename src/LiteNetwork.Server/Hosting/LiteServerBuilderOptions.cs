using LiteNetwork.Protocol.Abstractions;
using LiteNetwork.Protocol;
using LiteNetwork.Common;

namespace LiteNetwork.Server.Hosting
{
    /// <summary>
    /// Builder options to use with host builder.
    /// </summary>
    public class LiteServerBuilderOptions
    {
        /// <summary>
        /// Gets or sets the server's listening host.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the server's listening port.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the maximum of pending connections queue.
        /// </summary>
        public int Backlog { get; set; } = LiteServerConfiguration.DefaultBacklog;

        /// <summary>
        /// Gets or sets the handled client buffer size.
        /// </summary>
        public int ClientBufferSize { get; set; } = LiteServerConfiguration.DefaultClientBufferSize;

        /// <summary>
        /// Gets or sets the receive strategy type.
        /// </summary>
        public ReceiveStrategyType ReceiveStrategy { get; set; }

        /// <summary>
        /// Gets the default server packet processor.
        /// </summary>
        public ILitePacketProcessor PacketProcessor { get; set; }

        /// <summary>
        /// Gets or sets the receive strategy type.
        /// </summary>
        public ReceiveStrategyType ReceiveStrategy { get; set; }

        /// <summary>
        /// Creates and initializes a new <see cref="LiteServerBuilderOptions"/> instance
        /// with a default <see cref="LitePacketProcessor"/>.
        /// </summary>
        internal LiteServerBuilderOptions()
        {
            PacketProcessor = new LitePacketProcessor();
        }
    }
}
