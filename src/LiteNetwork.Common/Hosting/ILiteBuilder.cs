using Microsoft.Extensions.DependencyInjection;

namespace LiteNetwork.Common.Hosting
{
    /// <summary>
    /// Provides a basic mechanism to configuring LiteNetwork clients or servers.
    /// </summary>
    public interface ILiteBuilder
    {
        /// <summary>
        /// Gets an <see cref="IServiceCollection"/> where services are configured.
        /// </summary>
        IServiceCollection Services { get; }
    }
}
