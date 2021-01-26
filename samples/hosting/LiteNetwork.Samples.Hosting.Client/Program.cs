using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using LiteNetwork.Common.Hosting;
using LiteNetwork.Client.Hosting;

namespace LiteNetwork.Samples.Hosting.Client
{
    class Program
    {
        static Task Main(string[] args)
        {
            Console.Title = "LiteNetwork Hosting Sample";

            var host = new HostBuilder()
                .ConfigureLiteNetwork((context, builder) =>
                {
                    builder.AddLiteClient<ICustomClient, CustomClient>(options =>
                    {
                        options.Host = "127.0.0.1";
                        options.Port = 4444;
                    });
                })
                .UseConsoleLifetime()
                .Build();

            return host.RunAsync();
        }
    }
}
