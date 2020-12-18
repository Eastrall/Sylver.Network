using LiteNetwork.Server;
using System;

namespace LiteNetwork.Sample.Echo.Server
{
    public class EchoServer : LiteServer<ClientUser>
    {
        public EchoServer(LiteServerOptions options)
            : base(options)
        {
        }

        protected override void OnBeforeStart()
        {
            Console.WriteLine("Starting Echo server.");
        }

        protected override void OnAfterStart()
        {
            Console.WriteLine($"Echo server listining on port: {Options.Port}");
        }
    }
}
