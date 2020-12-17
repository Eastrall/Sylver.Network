using Microsoft.Extensions.Hosting;
using System;

namespace LiteNetwork.Common.Hosting
{
    /// <summary>
    /// Provides extension methods to configure a LiteNetwork services in an <see cref="IHostBuilder"/>.
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Configures a <see cref="ILiteBuilder"/> service in to the specified <see cref="IHostBuilder"/>.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to configure.</param>
        /// <param name="configureLite">A <see cref="ILiteBuilder"/> configuration.</param>
        /// <returns>A <see cref="IHostBuilder"/> configured.</returns>
        public static IHostBuilder ConfigureLiteNetwork(this IHostBuilder hostBuilder, Action<HostBuilderContext, ILiteBuilder> configureLite)
        {
            return hostBuilder.ConfigureServices((context, collection) => collection.UseLiteNetwork(builder => configureLite(context, builder)));
        }

        /// <summary>
        /// Configures a <see cref="ILiteBuilder"/> service in to the specified <see cref="IHostBuilder"/>.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to configure.</param>
        /// <param name="configureLite">A <see cref="ILiteBuilder"/> configuration.</param>
        /// <returns>A <see cref="IHostBuilder"/> configured.</returns>
        public static IHostBuilder ConfigureLiteNetwork(this IHostBuilder hostBuilder, Action<ILiteBuilder> configureLite)
        {
            return hostBuilder.ConfigureServices((context, collection) => collection.UseLiteNetwork(builder => configureLite(builder)));
        }
    }
}
