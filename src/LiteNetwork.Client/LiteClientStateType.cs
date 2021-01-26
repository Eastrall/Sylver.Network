namespace LiteNetwork.Client
{
    /// <summary>
    /// Defines the client connection states.
    /// </summary>
    public enum LiteClientStateType
    {
        /// <summary>
        /// The client is not connected to a remote host.
        /// </summary>
        Disconnected,

        /// <summary>
        /// The client is in connection process.
        /// </summary>
        Connecting,

        /// <summary>
        /// The client is connected to a remote host.
        /// </summary>
        Connected
    }
}