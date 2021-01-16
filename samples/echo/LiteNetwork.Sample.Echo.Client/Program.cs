using LiteNetwork.Client;
using LiteNetwork.Client.Abstractions;
using LiteNetwork.Protocol;
using System;
using System.Threading.Tasks;

namespace LiteNetwork.Sample.Echo.Client
{
    class Program
    {
        static async Task Main()
        {
            var clientOptions = new LiteClientOptions
            {
                Host = "127.0.0.1",
                Port = 4444
            };
            using var client = new LiteClient(clientOptions);

            await client.ConnectAsync();

            while (true)
            {
                string input = Console.ReadLine();

                if (input == "quit")
                {
                    await client.DisconnectAsync();
                    break;
                }

                SendInput(client, input);
            }
        }

        private static void SendInput(ILiteClient client, string input)
        {
            using var packet = new LitePacket();

            packet.WriteString(input);

            client.Send(packet);
        }
    }
}
