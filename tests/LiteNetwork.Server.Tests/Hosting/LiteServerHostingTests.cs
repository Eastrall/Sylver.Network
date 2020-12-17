using LiteNetwork.Common.Hosting;
using LiteNetwork.Protocol.Abstractions;
using LiteNetwork.Server.Abstractions;
using LiteNetwork.Server.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Xunit;

namespace LiteNetwork.Server.Tests.Hosting
{
    public class LiteServerHostingTests
    {
        [Fact]
        public void SetupDefaultLiteServerHostTest()
        {
            IHost host = new HostBuilder()
                .ConfigureLiteNetwork(builder =>
                {
                    builder.AddLiteServer<CustomUser>(options =>
                    {
                        options.Host = "127.0.0.1";
                        options.Port = 4444;
                    });
                })
                .Build();

            using (host)
            {
                var server = host.Services.GetRequiredService<ILiteServer<CustomUser>>();

                Assert.IsType<LiteServer<CustomUser>>(server);
            }
        }

        [Fact]
        public void SetupCustomLiteServerHostTest()
        {
            IHost host = new HostBuilder()
                .ConfigureLiteNetwork(builder =>
                {
                    builder.AddLiteServer<CustomServer, CustomUser>(options =>
                    {
                        options.Host = "127.0.0.1";
                        options.Port = 4444;
                    });
                })
                .Build();

            using (host)
            {
                var server = host.Services.GetRequiredService<ILiteServer<CustomUser>>();

                Assert.IsType<CustomServer>(server);
                Assert.True(server is ILiteServer<CustomUser>);
            }
        }

        private class CustomUser : LiteServerUser
        {
        }

        private class CustomServer : LiteServer<CustomUser>
        {
            public CustomServer(LiteServerOptions options, IServiceProvider serviceProvider) 
                : base(options, serviceProvider)
            {
            }
        }
    }
}
