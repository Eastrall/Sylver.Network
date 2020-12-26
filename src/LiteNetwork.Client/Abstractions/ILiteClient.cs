using LiteNetwork.Common;
using System;
using System.Threading.Tasks;

namespace LiteNetwork.Client.Abstractions
{
    public interface ILiteClient : ILiteConnection, IDisposable
    {
        LiteClientOptions Options { get; }

        Task ConnectAsync();

        Task DisconnectAsync();
    }
}
