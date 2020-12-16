using LiteNetwork.Server;
using System;

namespace LiteNetwork.Sample.Echo.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new LiteServerOptions()
            {
                Host = "127.0.0.1",
                Port = 4444,
                ReceiveStrategy = Common.ReceiveStrategyType.Queued
            };
            using var server = new EchoServer(configuration);

            server.Start();
            Console.ReadKey();
        }
    }
}
