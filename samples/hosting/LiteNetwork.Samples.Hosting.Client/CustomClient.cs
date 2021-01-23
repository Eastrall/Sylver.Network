using LiteNetwork.Client;
using LiteNetwork.Protocol.Abstractions;
using System;
using System.Threading.Tasks;

namespace LiteNetwork.Samples.Hosting.Client
{
    public interface ICustomClient
    {
        void DoSomething();
    }

    public class CustomClient : LiteClient, ICustomClient
    {
        public CustomClient(LiteClientOptions options, IServiceProvider serviceProvider = null)
            : base(options, serviceProvider)
        {
        }

        public override Task HandleMessageAsync(ILitePacketStream incomingPacketStream)
        {
            string receivedMessage = incomingPacketStream.ReadString();

            Console.WriteLine($"Received message: {receivedMessage}");

            return base.HandleMessageAsync(incomingPacketStream);
        }

        protected override void OnConnected()
        {
            Console.WriteLine($"Client connected to {Options.Host}:{Options.Port}");
            base.OnConnected();
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"Client disconnected.");
            base.OnDisconnected();
        }

        public void DoSomething()
        {
            throw new NotImplementedException();
        }
    }
}
