using Microsoft.Extensions.DependencyInjection;

namespace LiteNetwork.Common.Hosting
{
    /// <summary>
    /// Provides a basic <see cref="ILiteBuilder"/> implementation.
    /// </summary>
    internal class LiteBuilder : ILiteBuilder
    {
        /// <inheritdoc />
        public IServiceCollection Services { get; }

        /// <summary>
        /// Creates a new <see cref="LiteBuilder"/> instance with the given services.
        /// </summary>
        /// <param name="services">Service collection.</param>
        public LiteBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
