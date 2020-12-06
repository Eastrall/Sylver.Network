using LiteNetwork.Common;
using System.Threading.Tasks;

namespace LiteNetwork.Client.Abstractions
{
    public interface ILiteClient : ILiteConnection
    {
        Task ConnectAsync();

        Task DisconnectAsync();
    }
}
