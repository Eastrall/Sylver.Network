using LiteNetwork.Client.Abstractions;
using LiteNetwork.Common.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LiteNetwork.Client.Hosting
{
    public static class LiteClientBuilderExtensions
    {
        /// <summary>
        /// Initializes a basic <see cref="LiteClient"/>.
        /// </summary>
        /// <param name="builder">A <see cref="ILiteBuilder"/> to add the client.</param>
        /// <param name="configure">Delegate to configure a <see cref="LiteClientOptions"/>.</param>
        /// <returns>The <see cref="ILiteBuilder"/>.</returns>
        public static ILiteBuilder AddLiteClient(this ILiteBuilder builder, Action<LiteClientOptions> configure)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddSingleton<ILiteClient, LiteClient>(serviceProvider =>
            {
                var liteClientOptions = new LiteClientOptions();
                configure(liteClientOptions);

                var client = new LiteClient(liteClientOptions, serviceProvider);
                return client;
            });


            builder.Services.AddHostedService(serviceProvider =>
            {
                var serverInstance = serviceProvider.GetRequiredService<ILiteClient>();
                return new LiteClientHostedService(serverInstance);
            });
            return builder;
        }

        /// <summary>
        /// Initializes a custom <see cref="ILiteClient"/>.
        /// </summary>
        /// <typeparam name="TLiteClient">Custom client type.</typeparam>
        /// <param name="builder">A <see cref="ILiteBuilder"/> to add the client.</param>
        /// <param name="configure">Delegate to configure a <see cref="LiteClientOptions"/>.</param>
        /// <returns>The <see cref="ILiteBuilder"/>.</returns>
        public static ILiteBuilder AddLiteClient<TLiteClient>(this ILiteBuilder builder, Action<LiteClientOptions> configure)
            where TLiteClient : class, ILiteClient
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddSingleton(serviceProvider =>
            {
                var liteClientOptions = new LiteClientOptions();
                configure(liteClientOptions);

                return ActivatorUtilities.CreateInstance<TLiteClient>(serviceProvider, liteClientOptions);
            });

            builder.Services.AddHostedService(serviceProvider =>
            {
                var serverInstance = serviceProvider.GetRequiredService<TLiteClient>();
                return new LiteClientHostedService(serverInstance);
            });
            return builder;
        }

        /// <summary>
        /// Initializes a custom <see cref="ILiteClient"/> with a custom interface.
        /// </summary>
        /// <typeparam name="TLiteClient">LiteClient abstraction.</typeparam>
        /// <typeparam name="TLiteClientImplementation">LiteClient implementation.</typeparam>
        /// <param name="builder">A <see cref="ILiteBuilder"/> to add the client.</param>
        /// <param name="configure">Delegate to configure a <see cref="LiteClientOptions"/>.</param>
        /// <returns>The <see cref="ILiteBuilder"/>.</returns>
        public static ILiteBuilder AddLiteClient<TLiteClient, TLiteClientImplementation>(this ILiteBuilder builder, Action<LiteClientOptions> configure)
            where TLiteClient : class
            where TLiteClientImplementation : class, TLiteClient, ILiteClient
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddSingleton<TLiteClient, TLiteClientImplementation>(serviceProvider =>
            {
                var liteClientOptions = new LiteClientOptions();
                configure(liteClientOptions);

                return ActivatorUtilities.CreateInstance<TLiteClientImplementation>(serviceProvider, liteClientOptions);
            });

            builder.Services.AddSingleton<ILiteClient, TLiteClientImplementation>(serviceProvider =>
            {
                return (TLiteClientImplementation)serviceProvider.GetRequiredService<TLiteClient>();
            });

            builder.Services.AddHostedService(serviceProvider =>
            {
                var serverInstance = serviceProvider.GetRequiredService<ILiteClient>();
                return new LiteClientHostedService(serverInstance);
            });
            return builder;
        }
    }
}
