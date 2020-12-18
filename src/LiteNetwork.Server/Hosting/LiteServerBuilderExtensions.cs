using LiteNetwork.Common.Hosting;
using LiteNetwork.Server.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LiteNetwork.Server.Hosting
{
    public static class LiteServerBuilderExtensions
    {
        /// <summary>
        /// Initializes a basic <see cref="LiteServer{TUser}"/> with the specified <typeparamref name="TLiteServerUser"/>.
        /// </summary>
        /// <typeparam name="TLiteServerUser">Server's user type.</typeparam>
        /// <param name="builder">A <see cref="ILiteBuilder"/> to add server.</param>
        /// <param name="configure">Delegate to configure a <see cref="LiteServerOptions"/>.</param>
        /// <returns>The <see cref="ILiteBuilder"/>.</returns>
        public static ILiteBuilder AddLiteServer<TLiteServerUser>(this ILiteBuilder builder, Action<LiteServerOptions> configure) 
            where TLiteServerUser : LiteServerUser
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddSingleton<ILiteServer<TLiteServerUser>, LiteServer<TLiteServerUser>>(serviceProvider =>
            {
                var liteServerOptions = new LiteServerOptions();
                configure(liteServerOptions);

                var server = new LiteServer<TLiteServerUser>(liteServerOptions, serviceProvider);
                return server;
            });


            builder.Services.AddHostedService(serviceProvider =>
            {
                var serverInstance = serviceProvider.GetRequiredService<ILiteServer<TLiteServerUser>>();
                return new LiteServerHostedService<TLiteServerUser>(serverInstance);
            });
            return builder;
        }

        /// <summary>
        /// Initializes a <typeparamref name="TLiteServer"/> with the specified <typeparamref name="TLiteServerUser"/>.
        /// </summary>
        /// <typeparam name="TLiteServer">Server type.</typeparam>
        /// <typeparam name="TLiteServerUser">Server's user type.</typeparam>
        /// <param name="builder">A <see cref="ILiteBuilder"/> to add server.</param>
        /// <param name="configure">Delegate to configure a <see cref="LiteServerOptions"/>.</param>
        /// <returns>The <see cref="ILiteBuilder"/>.</returns>
        public static ILiteBuilder AddLiteServer<TLiteServer, TLiteServerUser>(this ILiteBuilder builder, Action<LiteServerOptions> configure)
            where TLiteServer : class, ILiteServer<TLiteServerUser>
            where TLiteServerUser : LiteServerUser
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddSingleton<ILiteServer<TLiteServerUser>, TLiteServer>(serviceProvider =>
            {
                var liteServerOptions = new LiteServerOptions();
                configure(liteServerOptions);
                return ActivatorUtilities.CreateInstance<TLiteServer>(serviceProvider, liteServerOptions);
            });

            builder.Services.AddHostedService(serviceProvider =>
            {
                var serverInstance = serviceProvider.GetRequiredService<ILiteServer<TLiteServerUser>>();
                return new LiteServerHostedService<TLiteServerUser>(serverInstance);
            });

            return builder;
        }

        /// <summary>
        /// Initializes a <typeparamref name="TLiteServerImplementation"/> with the specified <typeparamref name="TLiteServerUser"/>.
        /// </summary>
        /// <typeparam name="TLiteServer">LiteServer abstraction</typeparam>
        /// <typeparam name="TLiteServerImplementation">LiteServer implementation.</typeparam>
        /// <typeparam name="TLiteServerUser">LiteServer user.</typeparam>
        /// <param name="builder">A <see cref="ILiteBuilder"/> to add server.</param>
        /// <param name="configure">Delegate to configure a <see cref="LiteServerOptions"/>.</param>
        /// <returns>The <see cref="ILiteBuilder"/>.</returns>
        public static ILiteBuilder AddLiteServer<TLiteServer, TLiteServerImplementation, TLiteServerUser>(this ILiteBuilder builder, Action<LiteServerOptions> configure)
            where TLiteServer : class
            where TLiteServerImplementation : class, TLiteServer, ILiteServer<TLiteServerUser>
            where TLiteServerUser : LiteServerUser
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddSingleton<TLiteServer, TLiteServerImplementation>(serviceProvider =>
            {
                var liteServerOptions = new LiteServerOptions();
                configure(liteServerOptions);

                return ActivatorUtilities.CreateInstance<TLiteServerImplementation>(serviceProvider, liteServerOptions);
            });

            builder.Services.AddSingleton<ILiteServer<TLiteServerUser>, TLiteServerImplementation>(serviceProvider =>
            {
                return (TLiteServerImplementation)serviceProvider.GetRequiredService<TLiteServer>();
            });

            builder.Services.AddHostedService(serviceProvider =>
            {
                var serverInstance = serviceProvider.GetRequiredService<ILiteServer<TLiteServerUser>>();
                return new LiteServerHostedService<TLiteServerUser>(serverInstance);
            });

            return builder;
        }
    }
}
