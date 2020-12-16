using Microsoft.Extensions.DependencyInjection;
using System;

namespace LiteNetwork.Common.Hosting
{
    /// <summary>
    /// Provides extension methods for setup a LiteNetwork service in an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class LiteNetworkCollectionExtensions
    {
        /// <summary>
        /// Adds a LiteNetwork services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">An <see cref="IServiceCollection"/> to add services.</param>
        /// <param name="configure">A <see cref="ILiteBuilder"/> configuration.</param>
        /// <returns></returns>
        public static IServiceCollection UseLiteNetwork(this IServiceCollection services, Action<ILiteBuilder> configure)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            configure(new LiteBuilder(services));
            return services;
        }
    }
}
